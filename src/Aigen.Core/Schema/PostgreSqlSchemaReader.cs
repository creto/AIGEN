using Aigen.Core.Metadata;
using Aigen.Core.Services;
using Npgsql;

namespace Aigen.Core.Schema;

/// <summary>
/// Implementacion de ISchemaReader para PostgreSQL.
/// Lee el schema desde information_schema y pg_catalog.
/// </summary>
public class PostgreSqlSchemaReader : ISchemaReader
{
    private readonly NamingConventionService _naming = new();
    public async Task<bool> TestConnectionAsync(
        string connectionString, CancellationToken ct = default)
    {
        try
        {
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(ct);
            return conn.State == System.Data.ConnectionState.Open;
        }
        catch { return false; }
    }

    public async Task<DatabaseMetadata> ReadAsync(
        string connectionString,
        string schema = "public",
        CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(ct);

        var meta = new DatabaseMetadata
        {
            ServerName    = conn.Host ?? "localhost",
            DatabaseName  = conn.Database,
            Schema        = schema,
            Engine        = "PostgreSQL",
            ServerVersion = conn.ServerVersion,
            ReadAt        = DateTime.Now
        };

        var tables  = await ReadTablesAsync(conn, schema, ct);
        var columns = await ReadColumnsAsync(conn, schema, ct);
        var pks     = await ReadPrimaryKeysAsync(conn, schema, ct);
        var fks     = await ReadForeignKeysAsync(conn, schema, ct);

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
                    NavigationPropertyName = _naming.ToClassName(f.ReferencedTable),
                    PropertyName           = _naming.ToPropertyName(f.ColumnName)
                }).ToList();
        }

        meta.Tables = tables;
        return meta;
    }

    // ── Tablas ────────────────────────────────────────────────────
    private async Task<List<TableMetadata>> ReadTablesAsync(
        NpgsqlConnection conn, string schema, CancellationToken ct)
    {
        const string sql = @"
            SELECT table_name
            FROM information_schema.tables
            WHERE table_schema = @s AND table_type = 'BASE TABLE'
            ORDER BY table_name";

        var list = new List<TableMetadata>();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@s", schema);
        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
        {
            var name    = r.GetString(0);
            var cls     = _naming.ToClassName(name);
            var plural  = _naming.ToClassNamePlural(cls);
            list.Add(new TableMetadata
            {
                SchemaName     = schema,
                TableName      = name,
                ClassName      = cls,
                ClassNamePlural = plural,
                ObjectName     = _naming.ToObjectName(cls),
                ServiceName    = _naming.ToServiceName(cls),
                RepositoryName = _naming.ToRepositoryName(cls),
                ControllerName = _naming.ToControllerName(plural),
                ApiRoute       = _naming.ToApiRoute(plural)
            });
        }
        return list;
    }

    // ── Columnas ──────────────────────────────────────────────────
    private async Task<List<RawColumn>> ReadColumnsAsync(
        NpgsqlConnection conn, string schema, CancellationToken ct)
    {
        const string sql = @"
            SELECT
                c.table_name,
                c.column_name,
                c.ordinal_position,
                c.is_nullable,
                c.data_type,
                c.character_maximum_length,
                c.numeric_precision,
                c.numeric_scale,
                c.column_default,
                CASE WHEN c.column_default LIKE 'nextval%' THEN 1 ELSE 0 END AS is_identity
            FROM information_schema.columns c
            WHERE c.table_schema = @s
            ORDER BY c.table_name, c.ordinal_position";

        var list = new List<RawColumn>();
        await using var cmd = new NpgsqlCommand(sql, conn);
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
                IsIdentity      = r.GetInt32(9) == 1,
                CSharpType      = MapToCSharp(sqlType),
                TypeScriptType  = MapToTypeScript(sqlType),
                PropertyName    = _naming.ToPropertyName(colName),
                TsPropertyName  = _naming.ToTsPropertyName(colName),
                DisplayName     = _naming.ToDisplayName(colName)
            });
        }
        return list;
    }

    // ── Primary Keys ──────────────────────────────────────────────
    private static async Task<List<RawKey>> ReadPrimaryKeysAsync(
        NpgsqlConnection conn, string schema, CancellationToken ct)
    {
        const string sql = @"
            SELECT ku.table_name, ku.column_name
            FROM information_schema.table_constraints tc
            JOIN information_schema.key_column_usage ku
                ON tc.constraint_name = ku.constraint_name
                AND tc.table_schema   = ku.table_schema
            WHERE tc.constraint_type = 'PRIMARY KEY'
              AND tc.table_schema    = @s
            ORDER BY ku.table_name, ku.ordinal_position";

        var list = new List<RawKey>();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@s", schema);
        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
            list.Add(new RawKey(r.GetString(0), r.GetString(1)));
        return list;
    }

    // ── Foreign Keys ──────────────────────────────────────────────
    private static async Task<List<RawFK>> ReadForeignKeysAsync(
        NpgsqlConnection conn, string schema, CancellationToken ct)
    {
        const string sql = @"
            SELECT
                tc.constraint_name,
                kcu.table_name,
                kcu.column_name,
                ccu.table_name  AS referenced_table,
                ccu.column_name AS referenced_column
            FROM information_schema.table_constraints tc
            JOIN information_schema.key_column_usage kcu
                ON tc.constraint_name = kcu.constraint_name
               AND tc.table_schema    = kcu.table_schema
            JOIN information_schema.constraint_column_usage ccu
                ON ccu.constraint_name = tc.constraint_name
               AND ccu.table_schema    = tc.table_schema
            WHERE tc.constraint_type = 'FOREIGN KEY'
              AND tc.table_schema    = @s";

        var list = new List<RawFK>();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@s", schema);
        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
            list.Add(new RawFK(
                r.GetString(0), r.GetString(1), r.GetString(2),
                r.GetString(3), r.GetString(4)));
        return list;
    }

    // ── Mapeo de tipos ────────────────────────────────────────────
    private static string MapToCSharp(string t) => t.ToLower() switch
    {
        "bigint"                                    => "long",
        "integer" or "int4"                         => "int",
        "smallint" or "int2"                        => "short",
        "boolean" or "bool"                         => "bool",
        "numeric" or "decimal" or "real"            => "decimal",
        "double precision" or "float8"              => "double",
        "timestamp" or "timestamp without time zone"
            or "timestamp with time zone"           => "DateTime",
        "date"                                      => "DateOnly",
        "time" or "time without time zone"          => "TimeOnly",
        "uuid"                                      => "Guid",
        "bytea"                                     => "byte[]",
        "jsonb" or "json"                           => "string",
        _                                           => "string"
    };

    private static string MapToTypeScript(string t) => t.ToLower() switch
    {
        "bigint" or "integer" or "int4" or "smallint"
            or "numeric" or "decimal" or "real"
            or "double precision" or "float8"       => "number",
        "boolean" or "bool"                         => "boolean",
        "timestamp" or "date"
            or "timestamp without time zone"
            or "timestamp with time zone"           => "Date",
        _                                           => "string"
    };

    // ── Helpers ───────────────────────────────────────────────────
    private static string ToPascal(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        return string.Concat(
            name.Split(new[] { '_', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Length > 0 ? char.ToUpper(p[0]) + p[1..] : p));
    }

    private static string ToCamel(string name)
    {
        var p = ToPascal(name);
        return p.Length > 0 ? char.ToLower(p[0]) + p[1..] : p;
    }

    private static string Pluralize(string name)
    {
        if (name.EndsWith("s") || name.EndsWith("x")) return name;
        if (name.EndsWith("z")) return name[..^1] + "ces";
        if (name.EndsWith("cion") || name.EndsWith("sion")) return name + "es";
        return name + "s";
    }

    private static string ToDisplay(string col)
    {
        var c = col.TrimStart('_');
        var r = System.Text.RegularExpressions.Regex.Replace(c, "([A-Z])", " $1").Trim();
        return r.Length > 0 ? char.ToUpper(r[0]) + r[1..] : r;
    }

    // ── Tipos internos ────────────────────────────────────────────
    private class RawColumn : ColumnMetadata
    {
        public string TableName { get; set; } = string.Empty;
    }
    private record RawKey(string TableName, string ColumnName);
    private record RawFK(
        string ConstraintName, string TableName, string ColumnName,
        string ReferencedTable, string ReferencedColumn);


    // Agregar ANTES del último } de la clase PostgreSqlSchemaReader:

    public async Task<DatabaseMetadata> ReadMultiSchemaAsync(
        string connectionString,
        IEnumerable<string> schemas,
        CancellationToken ct = default)
    {
        DatabaseMetadata? result = null;
        foreach (var schema in schemas)
        {
            var partial = await ReadAsync(connectionString, schema, ct);
            if (result is null)
                result = partial;
            else
                result.Tables.AddRange(partial.Tables);
        }
        return result ?? await ReadAsync(connectionString, "public", ct);
    }
}

