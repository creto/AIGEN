namespace Aigen.Core.Metadata;

/// <summary>
/// Extension methods para clasificar tablas y determinar
/// qué operaciones CRUD se generan para cada una.
///
/// Soporta tres escenarios:
///   1. Estándar Incoder   → prefijos TM_/TB_/TP_/TR_/TC_/TA_/TS_/TH_/TI_/TX_
///   2. Normalizado        → sin prefijo, clasificado por heurística
///   3. Híbrido            → mezcla de ambos en la misma BD
/// </summary>
public static class TableMetadataExtensions
{
    // ── Mapa de prefijos Incoder ──────────────────────────────
    private static readonly Dictionary<string, TableType> PrefixMap =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["TM_"]  = TableType.Movement,
            ["TB_"]  = TableType.Basic,
            ["TBR_"] = TableType.BasicRelated,
            ["TP_"]  = TableType.Parameter,
            ["TR_"]  = TableType.Relational,
            ["TC_"]  = TableType.Composition,
            ["TA_"]  = TableType.Audit,
            ["TS_"]  = TableType.System,
            ["TH_"]  = TableType.Historical,
            ["TI_"]  = TableType.Image,
            ["TX_"]  = TableType.Dictionary,
        };

    // ── Keywords para heurística — SIN solapamientos entre categorías ──
    // REGLA: una keyword no puede aparecer en dos categorías distintas

    private static readonly HashSet<string> AuditKeywords =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Log", "Audit", "Auditoria", "AuditLog", "EventLog",
            "Trace", "Trazabilidad", "ChangeLog", "ActivityLog"
            // "Historial" removido — pertenece solo a Historical
            // "History"   removido — pertenece solo a Historical
        };

    private static readonly HashSet<string> HistoricalKeywords =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Historic", "Historico", "Historial", "History",
            "Archive", "Archivo", "Snapshot", "Backup"
            // "Old", "Legacy" removidos — demasiado genéricos, falsos positivos
        };

    private static readonly HashSet<string> SystemKeywords =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Config", "Configuration", "Setting", "Parameter",
            "Parametro", "SystemParam", "AppConfig", "GlobalConfig"
        };

    private static readonly HashSet<string> RelationalKeywords =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Mapping", "Junction", "Bridge", "Association"
            // "Rel", "Map", "Link" removidos — demasiado cortos, falsos positivos
            // Ej: "Reporte" contiene "Rel" → falso positivo
        };

    /// <summary>
    /// Determina el TableType de una tabla.
    /// Prioridad: prefijo Incoder → nombre → estructura → Movement por defecto.
    /// </summary>
    public static TableType GetTableType(this TableMetadata t)
    {
        // 1. Prefijo Incoder exacto — orden desc por longitud (TBR_ antes que TB_)
        foreach (var kv in PrefixMap.OrderByDescending(k => k.Key.Length))
            if (t.TableName.StartsWith(kv.Key, StringComparison.OrdinalIgnoreCase))
                return kv.Value;

        // 2. Heurística para tablas sin prefijo
        return ClassifyByHeuristic(t);
    }

    /// <summary>
    /// Clasifica tablas sin prefijo usando heurísticas de nombre y estructura.
    /// </summary>
    private static TableType ClassifyByHeuristic(TableMetadata t)
    {
        var name = t.TableName;

        // ── Nivel 1: Por nombre (keywords exactas sin solapamiento) ──
        // Orden importa: Historical antes que Audit para evitar que
        // "Historial" (que podría confundirse) siempre vaya a Historical
        if (HistoricalKeywords.Any(k =>
                name.Contains(k, StringComparison.OrdinalIgnoreCase)))
            return TableType.Historical;

        if (AuditKeywords.Any(k =>
                name.Contains(k, StringComparison.OrdinalIgnoreCase)))
            return TableType.Audit;

        if (SystemKeywords.Any(k =>
                name.Contains(k, StringComparison.OrdinalIgnoreCase)))
            return TableType.System;

        if (RelationalKeywords.Any(k =>
                name.Contains(k, StringComparison.OrdinalIgnoreCase)))
            return TableType.Relational;

        // ── Nivel 2: Por estructura de columnas ───────────────
        var pkCount  = t.PrimaryKeys.Count;
        var fkCount  = t.ForeignKeys.Count;
        var colCount = t.Columns.Count;

        // Tabla relacional pura: PK compuesta exactamente por 2 FKs, muy pocas columnas
        if (pkCount == 2 && fkCount >= 2 && colCount <= 4)
            return TableType.Relational;

        // Tabla de auditoría por estructura: tiene columna Operacion + timestamp
        var hasOperation = t.Columns.Any(c =>
            c.ColumnName.Equals("Operacion",  StringComparison.OrdinalIgnoreCase) ||
            c.ColumnName.Equals("Operation",  StringComparison.OrdinalIgnoreCase) ||
            c.ColumnName.Equals("Action",     StringComparison.OrdinalIgnoreCase));
        var hasTimestamp = t.Columns.Any(c =>
            c.CSharpType is "DateTime" or "DateTimeOffset");

        if (hasOperation && hasTimestamp)
            return TableType.Audit;

        // Tabla básica (catálogo): sin FKs o solo 1, pocas columnas (≤6)
        // colCount > 6 → probablemente es una entidad con lógica → Movement
        if (fkCount <= 1 && colCount <= 6)
            return TableType.Basic;

        // Tabla paramétrica: muchas FKs, pocas columnas propias
        if (fkCount >= 3 && colCount <= fkCount + 4)
            return TableType.Parameter;

        // Por defecto: tabla transaccional con CRUD completo
        return TableType.Movement;
    }

    // ── Capacidades derivadas ─────────────────────────────────

    public static bool HasFullCrud(this TableMetadata t) =>
        t.GetTableType() is
            TableType.Movement     or
            TableType.Basic        or
            TableType.BasicRelated or
            TableType.Parameter    or
            TableType.Composition  or
            TableType.Unknown;

    public static bool IsReadOnly(this TableMetadata t) =>
        t.GetTableType() is
            TableType.Audit      or
            TableType.System     or
            TableType.Historical or
            TableType.Dictionary;

    public static bool HasEstadoField(this TableMetadata t) =>
        t.Columns.Any(c =>
            c.ColumnName.Equals("Estado", StringComparison.OrdinalIgnoreCase));

    public static bool IsRelationalOnly(this TableMetadata t) =>
        t.GetTableType() == TableType.Relational;

    /// <summary>
    /// Etiqueta para CLI/UI. Tablas sin prefijo muestran "(inferido)".
    /// </summary>
    public static string GetTableTypeLabel(this TableMetadata t)
    {
        var hasPrefix = PrefixMap.Keys.Any(p =>
            t.TableName.StartsWith(p, StringComparison.OrdinalIgnoreCase));
        var type = t.GetTableType();
        return hasPrefix ? type.ToString() : $"{type} (inferido)";
    }
}
