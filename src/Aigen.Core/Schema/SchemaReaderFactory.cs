using Aigen.Core.Config.Enums;

namespace Aigen.Core.Schema;

/// <summary>
/// Fabrica que retorna el ISchemaReader correcto segun el motor de BD.
/// </summary>
public static class SchemaReaderFactory
{
    public static ISchemaReader Create(DatabaseEngine engine) => engine switch
    {
        DatabaseEngine.SqlServer  => new SqlServerSchemaReader(),
        DatabaseEngine.PostgreSQL => new PostgreSqlSchemaReader(),
        _ => throw new NotSupportedException(
            $"Motor de BD '{engine}' no soportado en esta version. " +
            $"Motores disponibles: SqlServer, PostgreSQL.")
    };
}
