namespace Aigen.Core.Services;

/// <summary>
/// Servicio centralizado de convencion de nombres.
/// Transforma nombres de tablas y columnas a los formatos
/// requeridos por cada capa del proyecto generado.
/// </summary>
public class NamingConventionService
{
    private static readonly string[] TablePrefixes =
    {
        "TBR_", "TBM_",                                     // compuestos PRIMERO
        "TM_", "TC_", "TS_", "TR_", "TD_", "TB_", "TG_",
        "TA_", "TP_", "TV_", "TH_", "TL_", "TN_", "TX_",
        "TI_", "TF_", "TK_"
    };
    private static readonly string[] TableSuffixes =
    {
        "_TBL", "_TAB", "_TABLE"
    };

    private static readonly HashSet<string> AuditColumns =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "_ippublica", "_nombremaquina", "_usuario", "_browser",
            "_sessionid", "_xmlauditoria",
            "createdAt", "createdBy", "updatedAt", "updatedBy",
            "eliminado", "deletedAt", "deletedBy",
            "fechaCreacion", "fechaModificacion",
            "usuarioCreacion", "usuarioModificacion"
        };

    // ── Tablas ────────────────────────────────────────────────────

    /// <summary>Clase C# limpia. TM_FacturaVenta -> FacturaVenta</summary>
    public string ToClassName(string tableName)
    {
        var name = RemovePrefixes(tableName);
        name = RemoveSuffixes(name);
        return ToPascalCase(name);
    }

    /// <summary>
    /// Plural en espanol.
    /// Factura->Facturas | Configuracion->Configuraciones | Luz->Luces
    /// </summary>
    public string ToClassNamePlural(string className)
    {
        if (string.IsNullOrEmpty(className)) return className;

        if (className.EndsWith("cion", StringComparison.OrdinalIgnoreCase) ||
            className.EndsWith("sion", StringComparison.OrdinalIgnoreCase) ||
            className.EndsWith("dad",  StringComparison.OrdinalIgnoreCase) ||
            className.EndsWith("tad",  StringComparison.OrdinalIgnoreCase) ||
            className.EndsWith("ion",  StringComparison.OrdinalIgnoreCase))
            return className + "es";

        if (className.EndsWith("z", StringComparison.OrdinalIgnoreCase))
            return className[..^1] + "ces";

        if (className.EndsWith("s", StringComparison.OrdinalIgnoreCase) ||
            className.EndsWith("x", StringComparison.OrdinalIgnoreCase))
            return className;

        if (className.EndsWith("in", StringComparison.OrdinalIgnoreCase) ||
            className.EndsWith("on", StringComparison.OrdinalIgnoreCase) ||
            className.EndsWith("an", StringComparison.OrdinalIgnoreCase))
            return className + "es";

        return className + "s";
    }

    public string ToObjectName(string className)      => ToCamelCase(className);
    public string ToServiceName(string className)     => className + "Service";
    public string ToRepositoryName(string className)  => className + "Repository";
    public string ToControllerName(string plural)     => plural + "Controller";
    public string ToApiRoute(string plural)           => "/api/" + plural.ToLowerInvariant();

    // ── Columnas ──────────────────────────────────────────────────

    public string ToPropertyName(string colName)   => ToPascalCase(colName);
    public string ToTsPropertyName(string colName) => ToCamelCase(ToPascalCase(colName));
    public bool   IsAuditField(string colName)     => AuditColumns.Contains(colName);

    public string ToDisplayName(string columnName)
    {
        var cleaned = columnName.TrimStart('_');
        var words = cleaned.Split(new[] { '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length > 1)
            return string.Join(" ", words.Select(w =>
                w.Length > 0 ? char.ToUpper(w[0]) + w[1..].ToLower() : w));

        var result = System.Text.RegularExpressions.Regex
            .Replace(cleaned, "([A-Z])", " $1").Trim();
        return result.Length > 0 ? char.ToUpper(result[0]) + result[1..] : result;
    }

    // ── Angular ───────────────────────────────────────────────────

    /// <summary>FacturaVenta -> factura-venta</summary>
    public string ToAngularFileName(string className)
    {
        var result = System.Text.RegularExpressions.Regex
            .Replace(className, "([A-Z])", "-$1")
            .TrimStart('-')
            .ToLowerInvariant();
        return result;
    }

    public string ToAngularModuleName(string name)                        => name + "Module";
    public string ToAngularComponentName(string name, string suffix = "") => name + suffix + "Component";

    // ── Helpers estaticos ─────────────────────────────────────────

    public static string ToPascalCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        return string.Concat(
            name.Split(new[] { '_', ' ', '-' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Length > 0 ? char.ToUpper(p[0]) + p[1..] : p));
    }

    public static string ToCamelCase(string name)
    {
        var p = ToPascalCase(name);
        return p.Length > 0 ? char.ToLower(p[0]) + p[1..] : p;
    }

    private static string RemovePrefixes(string name)
    {
        foreach (var prefix in TablePrefixes)
            if (name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return name[prefix.Length..];
        return name;
    }

    private static string RemoveSuffixes(string name)
    {
        foreach (var suffix in TableSuffixes)
            if (name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                return name[..^suffix.Length];
        return name;
    }
}
