namespace Aigen.Core.Metadata;

public class StoredProcedureMetadata
{
    public string            Schema         { get; set; } = "dbo";
    public string            Name           { get; set; } = string.Empty;
    public string            FullName       => $"[{Schema}].[{Name}]";
    public SpType            Type           { get; set; } = SpType.StoredProcedure;
    public string            RelatedTable   { get; set; } = string.Empty; // tabla asociada si aplica
    public List<SpParameter> Parameters     { get; set; } = new();
    public SpCrudType        CrudType       { get; set; } = SpCrudType.Unknown;
    public string            ReturnTypeSql  { get; set; } = string.Empty;
    public bool              IsAigenGenerated => Name.StartsWith("sp_Insert_")
                                              || Name.StartsWith("sp_Update_")
                                              || Name.StartsWith("sp_Delete_")
                                              || Name.StartsWith("sp_GetById_")
                                              || Name.StartsWith("sp_GetPaged_");
}

public class SpParameter
{
    public string  Name          { get; set; } = string.Empty; // ej: @IDDependencia
    public string  SqlType       { get; set; } = string.Empty; // ej: int
    public string  CSharpType    { get; set; } = string.Empty; // ej: int
    public bool    IsOutput      { get; set; }
    public bool    IsNullable    { get; set; }
    public int     MaxLength     { get; set; }
    public int     OrdinalPosition { get; set; }
    public string  CleanName     => Name.TrimStart('@');        // ej: IDDependencia
}

public enum SpType     { StoredProcedure, Function }
public enum SpCrudType { Unknown, Insert, Update, Delete, GetById, GetPaged, GetAll, Custom }
