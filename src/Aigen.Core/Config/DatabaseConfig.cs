using Aigen.Core.Config.Enums;
namespace Aigen.Core.Config;
public class DatabaseConfig
{
    public DatabaseEngine Engine           { get; set; } = DatabaseEngine.SqlServer;
    public string         ConnectionString { get; set; } = string.Empty;
    public string         Schema           { get; set; } = "dbo";
    public string         TableSelection   { get; set; } = "All";
    public List<string>   IncludedTables   { get; set; } = new();
    public List<string>   ExcludedTables   { get; set; } = new() { "__EFMigrationsHistory", "sysdiagrams" };
    public EfStrategy     EfStrategy       { get; set; } = EfStrategy.DatabaseFirst;
}
