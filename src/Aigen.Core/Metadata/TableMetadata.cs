namespace Aigen.Core.Metadata;
public class TableMetadata
{
    public string SchemaName      { get; set; } = string.Empty;
    public string TableName       { get; set; } = string.Empty;
    public string ClassName       { get; set; } = string.Empty;
    public string ClassNamePlural { get; set; } = string.Empty;
    public string ObjectName      { get; set; } = string.Empty;
    public string ServiceName     { get; set; } = string.Empty;
    public string RepositoryName  { get; set; } = string.Empty;
    public string ControllerName  { get; set; } = string.Empty;
    public string ApiRoute        { get; set; } = string.Empty;
    public List<ColumnMetadata>     Columns     { get; set; } = new();
    public List<string>             PrimaryKeys { get; set; } = new();
    public List<ForeignKeyMetadata> ForeignKeys { get; set; } = new();
    public List<IndexMetadata>      Indexes     { get; set; } = new();
    public bool HasSinglePrimaryKey => PrimaryKeys.Count == 1;
    public ColumnMetadata? PrimaryKeyColumn => Columns.FirstOrDefault(c => c.IsPrimaryKey);
    public IEnumerable<ColumnMetadata> FormColumns => Columns.Where(c => !c.IsPrimaryKey && !c.IsAuditField);
    public IEnumerable<ColumnMetadata> ListColumns => Columns.Where(c => !c.IsAuditField).Take(8);
    public bool HasAuditFields => Columns.Any(c => c.IsAuditField);
}
