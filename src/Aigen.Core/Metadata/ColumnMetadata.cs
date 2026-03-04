namespace Aigen.Core.Metadata;
public class ColumnMetadata
{
    public string  ColumnName      { get; set; } = string.Empty;
    public string  PropertyName    { get; set; } = string.Empty;
    public string  TsPropertyName  { get; set; } = string.Empty;
    public string  DisplayName     { get; set; } = string.Empty;
    public string  SqlType         { get; set; } = string.Empty;
    public string  CSharpType      { get; set; } = string.Empty;
    public string  TypeScriptType  { get; set; } = string.Empty;
    public bool    IsNullable      { get; set; }
    public bool    IsPrimaryKey    { get; set; }
    public bool    IsIdentity      { get; set; }
    public int     MaxLength       { get; set; }
    public int     Precision       { get; set; }
    public int     Scale           { get; set; }
    public bool    HasDefaultValue { get; set; }
    public string? DefaultValue    { get; set; }
    public bool    IsRequired      => !IsNullable && !IsIdentity;
    public int     OrdinalPosition { get; set; }
    public bool    IsAuditField    { get; set; }
    public string  CSharpTypeNullable => IsNullable && CSharpType != "string" ? $"{CSharpType}?" : CSharpType;
    public bool    HasMaxLength    => MaxLength > 0 && MaxLength < 8000;
    public bool    IsNumeric       => CSharpType is "int" or "long" or "decimal" or "double" or "float" or "short";
    public bool    IsDate          => CSharpType is "DateTime" or "DateOnly" or "DateTimeOffset";
}
