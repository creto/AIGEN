using Aigen.Core.Config;
using Aigen.Core.Metadata;

namespace Aigen.Core.Services;

/// <summary>
/// Filtra tablas del schema y enriquece sus nombres
/// usando NamingConventionService.
/// </summary>
public class SchemaFilterService
{
    private readonly NamingConventionService _naming;

    public SchemaFilterService(NamingConventionService naming)
    {
        _naming = naming;
    }

    /// <summary>
    /// Aplica los filtros de DatabaseConfig y enriquece nombres.
    /// Modos: All, Include, Exclude.
    /// </summary>
    public List<TableMetadata> Filter(
        List<TableMetadata> allTables,
        DatabaseConfig config)
    {
        var filtered = config.TableSelection switch
        {
            "Include" => allTables
                .Where(t => config.IncludedTables
                    .Contains(t.TableName, StringComparer.OrdinalIgnoreCase))
                .ToList(),
            "Exclude" or _ => allTables
                .Where(t => !config.ExcludedTables
                    .Contains(t.TableName, StringComparer.OrdinalIgnoreCase)
                 && !config.ExcludedPrefixes
                    .Any(p => t.TableName.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
                .ToList()
        };

        foreach (var table in filtered)
            EnrichTableNames(table);

        return filtered;
    }

    /// <summary>
    /// Filtra por seleccion manual del usuario (CLI MultiSelect).
    /// </summary>
    public List<TableMetadata> FilterBySelection(
        List<TableMetadata> allTables,
        IEnumerable<string> selectedNames)
    {
        var selected = allTables
            .Where(t => selectedNames.Contains(
                t.TableName, StringComparer.OrdinalIgnoreCase))
            .ToList();

        foreach (var table in selected)
            EnrichTableNames(table);

        return selected;
    }

    private void EnrichTableNames(TableMetadata table)
    {
        var className      = _naming.ToClassName(table.TableName);
        var classNamePlural = _naming.ToClassNamePlural(className);

        table.ClassName       = className;
        table.ClassNamePlural = classNamePlural;
        table.ObjectName      = _naming.ToObjectName(className);
        table.ServiceName     = _naming.ToServiceName(className);
        table.RepositoryName  = _naming.ToRepositoryName(className);
        table.ControllerName  = _naming.ToControllerName(classNamePlural);
        table.ApiRoute        = _naming.ToApiRoute(classNamePlural);

        foreach (var col in table.Columns)
        {
            col.PropertyName   = _naming.ToPropertyName(col.ColumnName);
            col.TsPropertyName = _naming.ToTsPropertyName(col.ColumnName);
            col.DisplayName    = _naming.ToDisplayName(col.ColumnName);
            col.IsAuditField   = _naming.IsAuditField(col.ColumnName);
        }
    }
}
