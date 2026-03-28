namespace Aigen.Core.Metadata;
public class ForeignKeyMetadata
{
    public string ConstraintName         { get; set; } = string.Empty;
    public string ColumnName             { get; set; } = string.Empty;
    public string ReferencedTable        { get; set; } = string.Empty;
    public string ReferencedColumn       { get; set; } = string.Empty;
    public string ReferencedSchema       { get; set; } = "dbo";
    public string NavigationPropertyName { get; set; } = string.Empty;
    public string PropertyName           { get; set; } = string.Empty;
    public string ReferencedPkCSharpType     { get; set; } = string.Empty;
    public string LocalFkCSharpType          { get; set; } = string.Empty;
}
public class IndexMetadata
{
    public string       IndexName    { get; set; } = string.Empty;
    public List<string> Columns      { get; set; } = new();
    public bool         IsUnique     { get; set; }
    public bool         IsPrimaryKey { get; set; }
}


