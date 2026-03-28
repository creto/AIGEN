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
    // REEMPLAZAR el mÃ©todo ReadAsync COMPLETO con este cÃ³digo
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
                .Select(f =>
                {
                    // Tipo C# de la columna FK local
                    var fkCol  = t.Columns.FirstOrDefault(c =>
                        c.ColumnName.Equals(f.ColumnName, StringComparison.OrdinalIgnoreCase));
                    var fkType = fkCol?.CSharpType ?? string.Empty;

                    // Tipo C# de la PK de la tabla referenciada
                    // Usar lista "columns" raw (ya completa) en lugar de refTable.Columns
                    // porque refTable.Columns puede estar vacío si aún no fue procesada
                    var refPkColRaw = columns.FirstOrDefault(c =>
                        c.TableName.Equals(f.ReferencedTable, StringComparison.OrdinalIgnoreCase)
                        && c.ColumnName.Equals(f.ReferencedColumn, StringComparison.OrdinalIgnoreCase));
                    var refPkType   = refPkColRaw?.CSharpType ?? string.Empty;

                    return new ForeignKeyMetadata
                    {
                        ConstraintName            = f.ConstraintName,
                        ColumnName                = f.ColumnName,
                        ReferencedTable           = f.ReferencedTable,
                        ReferencedColumn          = f.ReferencedColumn,
                        NavigationPropertyName    = _naming.ToClassName(f.ReferencedTable),
                        PropertyName              = _naming.ToPropertyName(f.ColumnName),
                        LocalFkCSharpType         = fkType,
                        ReferencedPkCSharpType    = refPkType
                    };
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

    // â”€â”€ Lectura de tablas â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
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

    // â”€â”€ Lectura de columnas â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
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

    // â”€â”€ Lectura de primary keys â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
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

        await using var cmd = new SqlCommand(sql, conn);
        var list = new List<RawKey>();
        cmd.Parameters.AddWithValue("@s", schema);
        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
            list.Add(new RawKey(r.GetString(0), r.GetString(1)));
        return list;
    }

    // â”€â”€ Lectura de foreign keys â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
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

    // â”€â”€ Lectura de indexes â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
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

    // â”€â”€ Helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
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

        // Prefijos normales â€” eliminar y PascalCase
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
        "hierarchyid"                                 => "string",
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

    // â”€â”€ Tipos internos â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
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


    // ── Stored Procedures ─────────────────────────────────────────────────────
    public async Task<List<StoredProcedureMetadata>> ReadStoredProceduresAsync(
        string connectionString, string schema = "dbo", CancellationToken ct = default)
    {
        using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);

        // Leer SPs
        var spRows    = new List<(string Name, string ReturnType)>();
        var paramRows = new List<(string SpName, string ParamName, string DataType, string Mode, string IsNullable, int MaxLen, int Ordinal)>();

        const string sqlSps = @"SELECT ROUTINE_NAME, ISNULL(DATA_TYPE,'') FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA=@s AND ROUTINE_TYPE='PROCEDURE' ORDER BY ROUTINE_NAME";
        await using (var cmd = new SqlCommand(sqlSps, conn))
        {
            cmd.Parameters.AddWithValue("@s", schema);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
                spRows.Add((r.GetString(0), r.GetString(1)));
        }

        const string sqlParams = @"SELECT SPECIFIC_NAME, ISNULL(PARAMETER_NAME,''), ISNULL(DATA_TYPE,''), ISNULL(PARAMETER_MODE,'IN'), ISNULL(IS_NULLABLE,'NO'), ISNULL(CHARACTER_MAXIMUM_LENGTH,0), ORDINAL_POSITION FROM INFORMATION_SCHEMA.PARAMETERS WHERE SPECIFIC_SCHEMA=@s ORDER BY SPECIFIC_NAME, ORDINAL_POSITION";
        await using (var cmd2 = new SqlCommand(sqlParams, conn))
        {
            cmd2.Parameters.AddWithValue("@s", schema);
            await using var r2 = await cmd2.ExecuteReaderAsync(ct);
            while (await r2.ReadAsync(ct))
                paramRows.Add((r2.GetString(0), r2.GetString(1), r2.GetString(2),
                               r2.GetString(3), r2.GetString(4),
                               r2.IsDBNull(5) ? 0 : r2.GetInt32(5), r2.GetInt32(6)));
        }

        var result = new List<StoredProcedureMetadata>();
        foreach (var sp in spRows)
        {
            var spMeta = new StoredProcedureMetadata
            {
                Schema        = schema,
                Name          = sp.Name,
                Type          = SpType.StoredProcedure,
                ReturnTypeSql = sp.ReturnType,
                CrudType      = DetectCrudType(sp.Name),
                RelatedTable  = ExtractTableFromSpName(sp.Name)
            };
            spMeta.Parameters = paramRows
                .Where(p => p.SpName == sp.Name && !string.IsNullOrEmpty(p.ParamName))
                .Select(p => new SpParameter
                {
                    Name            = p.ParamName,
                    SqlType         = p.DataType,
                    CSharpType      = SqlTypeToCSharp(p.DataType),
                    IsOutput        = p.Mode.Contains("OUT"),
                    IsNullable      = p.IsNullable == "YES",
                    MaxLength       = p.MaxLen,
                    OrdinalPosition = p.Ordinal
                }).ToList();
            result.Add(spMeta);
        }
        return result;
    }
    private static SpCrudType DetectCrudType(string name)
    {
        var n = name.ToLowerInvariant();
        if (n.StartsWith("sp_insert_"))   return SpCrudType.Insert;
        if (n.StartsWith("sp_update_"))   return SpCrudType.Update;
        if (n.StartsWith("sp_delete_"))   return SpCrudType.Delete;
        if (n.StartsWith("sp_getbyid_"))  return SpCrudType.GetById;
        if (n.StartsWith("sp_getpaged_")) return SpCrudType.GetPaged;
        if (n.StartsWith("sp_getall_"))   return SpCrudType.GetAll;
        return SpCrudType.Custom;
    }

    private static string ExtractTableFromSpName(string name)
    {
        var prefixes = new[] { "sp_Insert_","sp_Update_","sp_Delete_",
                               "sp_GetById_","sp_GetPaged_","sp_GetAll_" };
        foreach (var p in prefixes)
            if (name.StartsWith(p, StringComparison.OrdinalIgnoreCase))
                return name[p.Length..];
        return string.Empty;
    }

    private static string SqlTypeToCSharp(string sqlType) => sqlType.ToLower() switch
    {
        "int" or "integer"    => "int",
        "bigint"              => "long",
        "smallint"            => "short",
        "tinyint"             => "byte",
        "bit"                 => "bool",
        "decimal" or "numeric"=> "decimal",
        "float"               => "double",
        "real"                => "float",
        "money" or "smallmoney" => "decimal",
        "datetime" or "datetime2" or "date" => "DateTime",
        "time"                => "TimeSpan",
        "uniqueidentifier"    => "Guid",
        _                     => "string"
    };
}




