using System.Data;
using Aigen.Core.Metadata;
using Microsoft.Data.SqlClient;
using Aigen.Core.Config;
using Aigen.Core.Services;


namespace Aigen.Core.Schema;

public class SqlServerSchemaReader : ISchemaReader
{
    private readonly NamingConventionService _naming = new();
    public async Task<bool> TestConnectionAsync(string connectionString, CancellationToken ct = default)
    {
        try
        {
            await using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync(ct);
            return conn.State == System.Data.ConnectionState.Open;
        }
        catch { return false; }
    }

    // ============================================================
    // REEMPLAZAR el método ReadAsync COMPLETO con este código
    // ============================================================

    public async Task<DatabaseMetadata> ReadAsync(
        string connectionString,
        string schema = "dbo",
        CancellationToken ct = default)
        => await ReadMultiSchemaAsync(connectionString, new[] { schema }, ct);

    public async Task<DatabaseMetadata> ReadMultiSchemaAsync(
        string connectionString,
        IEnumerable<string> schemas,
        CancellationToken ct = default)
    {
        var schemaList = schemas.ToList();
        var firstSchema = schemaList.FirstOrDefault() ?? "dbo";

        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);

        var meta = new DatabaseMetadata
        {
            ServerName    = conn.DataSource,
            DatabaseName  = conn.Database,
            Schema        = firstSchema,
            Engine        = "SqlServer",
            ServerVersion = conn.ServerVersion,
            ReadAt        = DateTime.Now
        };

        var tables  = new List<TableMetadata>();
        var columns = new List<RawColumn>();
        var pks     = new List<RawKey>();
        var fks     = new List<RawFK>();
        var idxs    = new List<RawIdx>();

        foreach (var s in schemaList)
        {
            tables .AddRange(await ReadTablesAsync(conn, s, ct));
            columns.AddRange(await ReadColumnsAsync(conn, s, ct));
            pks    .AddRange(await ReadPrimaryKeysAsync(conn, s, ct));
            fks    .AddRange(await ReadForeignKeysAsync(conn, s, ct));
            idxs   .AddRange(await ReadIndexesAsync(conn, s, ct));
        }

        foreach (var t in tables)
        {
            var cols = columns
                .Where(c => c.TableName == t.TableName)
                .OrderBy(c => c.OrdinalPosition)
                .ToList();
            var pk = pks
                .Where(p => p.TableName == t.TableName)
                .Select(p => p.ColumnName)
                .ToList();
            foreach (var col in cols) col.IsPrimaryKey = pk.Contains(col.ColumnName);
            MarkAuditFields(cols);
            t.Columns     = cols.Cast<ColumnMetadata>().ToList();
            t.PrimaryKeys = pk;
            t.ForeignKeys = fks
                .Where(f => f.TableName == t.TableName)
                .Select(f => new ForeignKeyMetadata
                {
                    ConstraintName         = f.ConstraintName,
                    ColumnName             = f.ColumnName,
                    ReferencedTable        = f.ReferencedTable,
                    ReferencedColumn       = f.ReferencedColumn,
                    NavigationPropertyName = _naming.ToClassName(f.ReferencedTable)
                }).ToList();
            t.Indexes = idxs
                .Where(i => i.TableName == t.TableName)
                .GroupBy(i => i.IndexName)
                .Select(g => new IndexMetadata
                {
                    IndexName    = g.Key,
                    Columns      = g.Select(i => _naming.ToPropertyName(i.ColumnName)).ToList(),
                    IsUnique     = g.First().IsUnique,
                    IsPrimaryKey = g.First().IsPrimaryKey
                }).ToList();
        }

        meta.Tables = tables;
        return meta;
    }

    // ── Lectura de tablas ────────────────────────────────────────
    private async Task<List<TableMetadata>> ReadTablesAsync(
        SqlConnection conn, string schema, CancellationToken ct)
    {
        const string sql = @"SELECT TABLE_SCHEMA, TABLE_NAME
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_TYPE='BASE TABLE' AND TABLE_SCHEMA=@s
            ORDER BY TABLE_NAME";

        var list = new List<TableMetadata>();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@s", schema);
        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
            list.Add(BuildTable(schema, r.GetString(1)));
        return list;
    }

    // ── Lectura de columnas ──────────────────────────────────────
    private static async Task<List<RawColumn>> ReadColumnsAsync(
        SqlConnection conn, string schema, CancellationToken ct)
    {
        const string sql = @"SELECT
            c.TABLE_NAME, c.COLUMN_NAME, c.ORDINAL_POSITION,
            c.IS_NULLABLE, c.DATA_TYPE, c.CHARACTER_MAXIMUM_LENGTH,
            c.NUMERIC_PRECISION, c.NUMERIC_SCALE, c.COLUMN_DEFAULT,
            COLUMNPROPERTY(OBJECT_ID(c.TABLE_SCHEMA+'.'+c.TABLE_NAME),
                c.COLUMN_NAME,'IsIdentity') AS IS_IDENTITY
            FROM INFORMATION_SCHEMA.COLUMNS c
            WHERE c.TABLE_SCHEMA=@s
            ORDER BY c.TABLE_NAME, c.ORDINAL_POSITION";

        var list = new List<RawColumn>();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@s", schema);
        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
        {
            var sqlType = r.GetString(4);
            var colName = r.GetString(1);
            list.Add(new RawColumn
            {
                TableName       = r.GetString(0),
                ColumnName      = colName,
                OrdinalPosition = r.GetInt32(2),
                IsNullable      = r.GetString(3) == "YES",
                SqlType         = sqlType,
                MaxLength       = r.IsDBNull(5) ? 0 : r.GetInt32(5),
                Precision       = r.IsDBNull(6) ? 0 : Convert.ToInt32(r.GetValue(6)),
                Scale           = r.IsDBNull(7) ? 0 : Convert.ToInt32(r.GetValue(7)),
                DefaultValue    = r.IsDBNull(8) ? null : r.GetString(8),
                IsIdentity      = !r.IsDBNull(9) && r.GetInt32(9) == 1,
                CSharpType      = MapToCSharp(sqlType),
                TypeScriptType  = MapToTypeScript(sqlType),
                PropertyName    = ToPascalCase(colName),
                TsPropertyName  = ToCamelCase(colName),
                DisplayName     = ToDisplayName(colName)
            });
        }
        return list;
    }

    // ── Lectura de primary keys ──────────────────────────────────
    private static async Task<List<RawKey>> ReadPrimaryKeysAsync(
        SqlConnection conn, string schema, CancellationToken ct)
    {
        const string sql = @"SELECT ku.TABLE_NAME, ku.COLUMN_NAME
            FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
            JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE ku
                ON tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME
                AND tc.TABLE_SCHEMA   = ku.TABLE_SCHEMA
            WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
              AND tc.TABLE_SCHEMA    = @s
            ORDER BY ku.TABLE_NAME, ku.ORDINAL_POSITION";

        var list = new List<RawKey>();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@s", schema);
        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
            list.Add(new RawKey(r.GetString(0), r.GetString(1)));
        return list;
    }

    // ── Lectura de foreign keys ──────────────────────────────────
    private static async Task<List<RawFK>> ReadForeignKeysAsync(
        SqlConnection conn, string schema, CancellationToken ct)
    {
        const string sql = @"SELECT
            fk.name, tp.name, cp.name, tr.name, cr.name
            FROM sys.foreign_keys fk
            JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
            JOIN sys.tables  tp ON fkc.parent_object_id     = tp.object_id
            JOIN sys.columns cp ON fkc.parent_object_id     = cp.object_id
                               AND fkc.parent_column_id     = cp.column_id
            JOIN sys.tables  tr ON fkc.referenced_object_id = tr.object_id
            JOIN sys.columns cr ON fkc.referenced_object_id = cr.object_id
                               AND fkc.referenced_column_id = cr.column_id
            JOIN sys.schemas sc ON tp.schema_id = sc.schema_id
            WHERE sc.name = @s
            ORDER BY tp.name";

        var list = new List<RawFK>();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@s", schema);
        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
            list.Add(new RawFK(
                r.GetString(0), r.GetString(1),
                r.GetString(2), r.GetString(3), r.GetString(4)));
        return list;
    }

    // ── Lectura de indexes ───────────────────────────────────────
    private static async Task<List<RawIdx>> ReadIndexesAsync(
        SqlConnection conn, string schema, CancellationToken ct)
    {
        const string sql = @"SELECT
            t.name, i.name, col.name, i.is_unique, i.is_primary_key
            FROM sys.indexes i
            JOIN sys.index_columns ic  ON i.object_id   = ic.object_id
                                      AND i.index_id    = ic.index_id
            JOIN sys.columns col       ON ic.object_id  = col.object_id
                                      AND ic.column_id  = col.column_id
            JOIN sys.tables t          ON i.object_id   = t.object_id
            JOIN sys.schemas sc        ON t.schema_id   = sc.schema_id
            WHERE sc.name = @s AND i.name IS NOT NULL
            ORDER BY t.name, i.name";

        var list = new List<RawIdx>();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@s", schema);
        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
            list.Add(new RawIdx(
                r.GetString(0), r.GetString(1), r.GetString(2),
                r.GetBoolean(3), r.GetBoolean(4)));
        return list;
    }

    // ── Helpers ──────────────────────────────────────────────────
    private TableMetadata BuildTable(string schema, string tableName)
    {
        var className      = _naming.ToClassName(tableName);
        var classNamePlural= _naming.ToClassNamePlural(className);
        var objectName     = ToCamelCase(className);
        return new TableMetadata
        {
            SchemaName     = schema,
            TableName      = tableName,
            ClassName      = className,
            ClassNamePlural = classNamePlural,
            ObjectName     = objectName,
            ServiceName    = $"{className}Service",
            RepositoryName = $"{className}Repository",
            ControllerName = $"{classNamePlural}Controller",
            ApiRoute       = $"/api/{objectName.ToLower()}s"
        };
    }

    private static void MarkAuditFields(List<RawColumn> cols)
    {
        var auditFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "_ippublica", "_nombremaquina", "_usuario", "_browser",
            "_sessionid", "_xmlauditoria", "createdAt", "createdBy",
            "updatedAt", "updatedBy", "eliminado", "deletedAt"
        };
        foreach (var col in cols)
            col.IsAuditField = auditFields.Contains(col.ColumnName);
    }

    private static string CleanClassName(string tableName)
    {
        // Prefijos con sufijo especial para evitar colisiones
        var historicalPrefixes = new[] { "TH_", "TAR_" };
        foreach (var p in historicalPrefixes)
            if (tableName.StartsWith(p, StringComparison.OrdinalIgnoreCase))
                return ToPascalCase(tableName[p.Length..]) + "Hist";

        // Prefijos normales — eliminar y PascalCase
        var normalPrefixes = new[] { "TBR_", "TM_", "TB_", "TP_", "TR_", "TC_", "TS_", "TI_", "TX_", "TA_" };
        foreach (var p in normalPrefixes)
            if (tableName.StartsWith(p, StringComparison.OrdinalIgnoreCase))
                return ToPascalCase(tableName[p.Length..]);

        return ToPascalCase(tableName);
    }

    private static string Pluralize(string name)
    {
        if (name.EndsWith("cion", StringComparison.OrdinalIgnoreCase) ||
            name.EndsWith("ion",  StringComparison.OrdinalIgnoreCase)) return name + "es";
        if (name.EndsWith("z", StringComparison.OrdinalIgnoreCase))    return name[..^1] + "ces";
        if (name.EndsWith("s", StringComparison.OrdinalIgnoreCase))    return name;
        return name + "s";
    }

    private static string MapToCSharp(string sqlType) => sqlType.ToLower() switch
    {
        "bigint"                                      => "long",
        "int"                                         => "int",
        "smallint"                                    => "short",
        "tinyint"                                     => "byte",
        "bit"                                         => "bool",
        "decimal" or "numeric" or "money"
            or "smallmoney"                           => "decimal",
        "float"                                       => "double",
        "real"                                        => "float",
        "datetime" or "datetime2" or "smalldatetime" => "DateTime",
        "date"                                        => "DateOnly",
        "time"                                        => "TimeOnly",
        "datetimeoffset"                              => "DateTimeOffset",
        "char" or "varchar" or "nchar" or "nvarchar"
            or "text" or "ntext" or "xml"            => "string",
        "uniqueidentifier"                            => "Guid",
        "binary" or "varbinary" or "image"           => "byte[]",
        _                                             => "object"
    };

    private static string MapToTypeScript(string sqlType) => sqlType.ToLower() switch
    {
        "bigint" or "int" or "smallint" or "tinyint"
            or "decimal" or "numeric" or "money"
            or "smallmoney" or "float" or "real"     => "number",
        "bit"                                         => "boolean",
        "datetime" or "datetime2" or "smalldatetime"
            or "date" or "datetimeoffset"             => "Date",
        _                                             => "string"
    };

    private static string ToPascalCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        return string.Concat(
            name.Split(new[] { '_', ' ', '-' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Length > 0
                    ? char.ToUpper(p[0]) + p[1..].ToLower()
                    : p));
    }

    private static string ToCamelCase(string name)
    {
        var p = ToPascalCase(name);
        return p.Length > 0 ? char.ToLower(p[0]) + p[1..] : p;
    }

    private static string ToDisplayName(string columnName)
    {
        var cleaned = columnName.TrimStart('_');
        var result  = System.Text.RegularExpressions.Regex
            .Replace(cleaned, "([A-Z])", " $1").Trim();
        return result.Length > 0
            ? char.ToUpper(result[0]) + result[1..]
            : result;
    }

    // ── Tipos internos ───────────────────────────────────────────
    // RawColumn es class (no record) para poder heredar de ColumnMetadata
    private class RawColumn : ColumnMetadata
    {
        public string TableName { get; set; } = string.Empty;
    }

    private record RawKey(string TableName, string ColumnName);
    private record RawFK(
        string ConstraintName,
        string TableName,
        string ColumnName,
        string ReferencedTable,
        string ReferencedColumn);
    private record RawIdx(
        string TableName,
        string IndexName,
        string ColumnName,
        bool   IsUnique,
        bool   IsPrimaryKey);
}
