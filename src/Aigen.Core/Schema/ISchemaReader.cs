using Aigen.Core.Metadata;
namespace Aigen.Core.Schema;
public interface ISchemaReader
{
    Task<DatabaseMetadata> ReadAsync(string connectionString, string schema = "dbo", CancellationToken cancellationToken = default);
    Task<bool> TestConnectionAsync(string connectionString, CancellationToken cancellationToken = default);
}
