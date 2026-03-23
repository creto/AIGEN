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
            "usuarioCreacion", "usuarioModificacion",
            // PostgreSQL lowercase aliases
            "creadoen", "creadopor",
            "modificadoen", "modificadopor",
            "eliminadoen", "eliminadopor",
            "fechacreacion", "fechamodificacion",
            "usuariocreacion", "usuariomodificacion"
        };

    // â”€â”€ Prefijos histÃ³ricos â€” generan sufijo "Hist" â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // Evita colisiones de ClassName entre tablas histÃ³ricas y sus contrapartes:
    //   TH_Serie  â†’ SerieHist   (no colisiona con TB_Serie  â†’ Serie)
    //   TH_Fondo  â†’ FondoHist   (no colisiona con TB_Fondo  â†’ Fondo)
    //   TAR_Trd   â†’ TrdHist     (no colisiona con TB_Trd    â†’ Trd)
    // TAR_ va primero (mÃ¡s largo) para match correcto antes de TH_
    private static readonly string[] HistoricalPrefixes = { "TAR_", "TH_" };

    // â”€â”€ Tablas â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>
    /// Clase C# limpia. TM_FacturaVenta â†’ FacturaVenta
    /// Tablas histÃ³ricas reciben sufijo Hist para evitar colisiones:
    ///   TH_Serie â†’ SerieHist | TAR_Trd â†’ TrdHist
    /// </summary>
    public string ToClassName(string tableName)
    {
        // 1. Prefijos histÃ³ricos primero (TAR_ antes que TH_ por longitud)
        foreach (var prefix in HistoricalPrefixes)
        {
            if (tableName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                var nameWithoutPrefix = tableName[prefix.Length..];
                nameWithoutPrefix = RemoveSuffixes(nameWithoutPrefix);
                return ToPascalCase(nameWithoutPrefix) + "Hist";
            }
        }

        // 2. Prefijos normales â€” solo eliminar
        var name = RemovePrefixes(tableName);
        name = RemoveSuffixes(name);
        var pascal = ToPascalCase(name);

        // Normalizar doble mayuscula inicial seguida de minuscula:
        // "EXpedientes" -> "Expedientes" (error de capitalizacion en BD)
        // Solo aplica al inicio: ^[A-Z][A-Z][a-z]
        pascal = System.Text.RegularExpressions.Regex.Replace(
            pascal,
            @"^([A-Z])([A-Z])([a-z])",
            m => m.Groups[1].Value + m.Groups[2].Value.ToLower() + m.Groups[3].Value);

        return pascal;
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

    // â”€â”€ Columnas â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    public string ToPropertyName(string colName)   => ToPascalCase(NormalizePostgresName(colName));

    /// <summary>
    /// Normaliza nombres de columnas PostgreSQL (lowercase sin separadores)
    /// reintroduciendo separadores para que ToPascalCase los procese correctamente.
    /// Ejemplo: "creadoen" â†’ "creado_en" â†’ "CreadoEn"
    /// </summary>
    private static string NormalizePostgresName(string name)
    {
        if (name.Contains('_') || name.Contains(' ')) return name; // ya tiene separadores
        return name.ToLower() switch
        {
            "creadoen"       => "creado_en",
            "creadopor"      => "creado_por",
            "modificadoen"   => "modificado_en",
            "modificadopor"  => "modificado_por",
            "eliminadoen"    => "eliminado_en",
            "eliminadopor"   => "eliminado_por",
            "fechacreacion"  => "fecha_creacion",
            "fechamodificacion" => "fecha_modificacion",
            "usuariocreacion"   => "usuario_creacion",
            "usuariomodificacion" => "usuario_modificacion",
            "estadoactivo"   => "estado_activo",
            "tipodocumento"  => "tipo_documento",
            "numerodocumento" => "numero_documento",
            "razonsocial"    => "razon_social",
            "nombrecompleto" => "nombre_completo",
            "primernombre"   => "primer_nombre",
            "segundonombre"  => "segundo_nombre",
            "primerapellido" => "primer_apellido",
            "segundoapellido" => "segundo_apellido",
            _                => name
        };
    }
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

    // â”€â”€ Angular â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>FacturaVenta -> factura-venta</summary>
    public string ToAngularFileName(string className)
    {
        // Paso 1: acrónimo seguido de palabra normal  → "MCDTAlgo" → "MCDT-Algo"
        var s = System.Text.RegularExpressions.Regex
            .Replace(className, @"([A-Z]+)([A-Z][a-z])", "$1-$2");
        // Paso 2: minúscula/dígito seguido de mayúscula → "OrdenCompra" → "Orden-Compra"
        s = System.Text.RegularExpressions.Regex
            .Replace(s, @"([a-z\d])([A-Z])", "$1-$2");
        return s.ToLowerInvariant();
    }

    public string ToAngularModuleName(string name)                        => name + "Module";
    public string ToAngularComponentName(string name, string suffix = "") => name + suffix + "Component";

    // â”€â”€ Helpers estaticos â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

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
        string prev;
        do {
            prev = name;
            foreach (var prefix in TablePrefixes)
            {
                if (name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    name = name[prefix.Length..];
                    break;
                }
            }
        } while (name != prev); // continuar mientras se eliminen prefijos
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





