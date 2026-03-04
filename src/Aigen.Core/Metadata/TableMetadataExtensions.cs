namespace Aigen.Core.Metadata;

public static class TableMetadataExtensions
{
    private static readonly Dictionary<string, TableType> PrefixMap =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["TM_"]  = TableType.Movement,
            ["TB_"]  = TableType.Basic,
            ["TBR_"] = TableType.BasicRelated,
            ["TP_"]  = TableType.Parameter,
            ["TR_"]  = TableType.Relational,
            ["TA_"]  = TableType.Audit,
            ["TS_"]  = TableType.System,
            ["TC_"]  = TableType.Composition,
            ["TX_"]  = TableType.Dictionary,
            ["TH_"]  = TableType.Historical,
            ["TI_"]  = TableType.Image,
        };

    public static TableType GetTableType(this TableMetadata t)
    {
        foreach (var kv in PrefixMap)
            if (t.TableName.StartsWith(kv.Key, StringComparison.OrdinalIgnoreCase))
                return kv.Value;
        return TableType.Unknown;
    }

    public static bool HasFullCrud(this TableMetadata t) =>
        t.GetTableType() is TableType.Movement or TableType.Basic
            or TableType.BasicRelated or TableType.Parameter
            or TableType.Composition or TableType.Unknown;

    public static bool IsReadOnly(this TableMetadata t) =>
        t.GetTableType() is TableType.Audit or TableType.System
            or TableType.Historical or TableType.Dictionary;

    public static bool HasEstadoField(this TableMetadata t) =>
        t.Columns.Any(c => c.ColumnName.Equals("Estado",
            StringComparison.OrdinalIgnoreCase));

    public static bool IsRelationalOnly(this TableMetadata t) =>
        t.GetTableType() == TableType.Relational;
}
