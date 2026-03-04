namespace Aigen.Core.Metadata;
public class DatabaseMetadata
{
    public string ServerName    { get; set; } = string.Empty;
    public string DatabaseName  { get; set; } = string.Empty;
    public string Schema        { get; set; } = string.Empty;
    public string Engine        { get; set; } = string.Empty;
    public string ServerVersion { get; set; } = string.Empty;
    public DateTime ReadAt      { get; set; } = DateTime.Now;
    public List<TableMetadata> Tables { get; set; } = new();
    public int TotalTables    => Tables.Count;
    public int TotalColumns   => Tables.Sum(t => t.Columns.Count);
    public int TotalRelations => Tables.Sum(t => t.ForeignKeys.Count);
    public TableMetadata? FindTable(string name) =>
        Tables.FirstOrDefault(t => t.TableName.Equals(name, StringComparison.OrdinalIgnoreCase));
}
