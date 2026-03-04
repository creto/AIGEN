using Aigen.Core.Config.Enums;

namespace Aigen.Core.Schema;

/// <summary>
/// Factory que retorna el ISchemaReader correcto segun el motor de BD configurado.
/// </summary>
public static class SchemaReaderFactory
{
    public static ISchemaReader Create(DatabaseEngine engine) =>
        engine switch
        {
            DatabaseEngine.SqlServer  => new SqlServerSchemaReader(),
            DatabaseEngine.PostgreSQL => new PostgreSqlSchemaReader(),
            _ => throw new NotSupportedException(
                $"Motor '{engine}' no soportado aun. Disponibles: SqlServer, PostgreSQL.")
        };
}
