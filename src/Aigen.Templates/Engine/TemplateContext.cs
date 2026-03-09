using Aigen.Core.Config;
using Aigen.Core.Config.Enums;
using Aigen.Core.Metadata;
using Scriban.Runtime;

namespace Aigen.Templates.Engine;

/// <summary>
/// Contexto completo inyectado en cada plantilla Scriban.
/// Propiedades disponibles en snake_case: entity_name, pk_type, api_route...
/// </summary>
public class TemplateContext
{
    public required TableMetadata    Table  { get; init; }
    public required DatabaseMetadata Db     { get; init; }
    public required GeneratorConfig  Config    { get; init; }
    public List<TableMetadata>?       AllTables { get; init; }
    // AllTablesScript: convierte AllTables a List<ScriptObject> para Scriban
    // Necesario porque Scriban no itera List<TableMetadata> directamente.
    // Usado en program.scriban para registrar repositorios y servicios en DI.
    public List<ScriptObject> AllTablesScript
    {
        get
        {
            if (AllTables is null) return new();
            return AllTables.Select(t =>
            {
                var obj = new ScriptObject();
                obj["RepositoryName"] = t.RepositoryName;
                obj["ServiceName"]    = t.ServiceName;
                obj["HasRepository"]  = t.HasRepository;
                obj["HasService"]     = t.HasService;
                obj["ClassNamePlural"] = t.ClassNamePlural;
                obj["KebabName"]      = ToKebab(t.ClassName);
                obj["ClassName"]      = t.ClassName;
                return obj;
            }).ToList();
        }
    }

    // ── Proyecto ─────────────────────────────────────────────
    public string ProjectName    => Config.Project.ProjectName;
    public string Author         => Config.Project.Author;
    public string GeneratedDate  => DateTime.Now.ToString("yyyy-MM-dd HH:mm");
    public string DotnetVersion  => Config.Backend.TargetFramework;
    public string RootNamespace  => Config.Project.RootNamespace;
    public string Version        => Config.Project.Version;
    public string Orm            => Config.Backend.Orm.ToString();
    public string Now            => DateTime.Now.ToString("yyyy-MM-dd HH:mm");
    public string Year           => Config.Project.Year;
    public string Description    => Config.Project.Description;

    // ── Base de datos ────────────────────────────────────────
    public string DbName   => Db.DatabaseName;
    public string DbSchema => Config.Database.Schema;
    public string DbEngine => Config.Database.Engine.ToString();

    // ── Entidad ──────────────────────────────────────────────
    public string EntityName       => Table.ClassName;
    public string EntityNamePlural => Table.ClassNamePlural;
    public string EntityNameCamel  => Table.ObjectName;
    public string ServiceName      => Table.ServiceName;
    public string RepoName         => Table.RepositoryName;
    public string ControllerName   => Table.ControllerName;
    public string ApiRoute         => Table.ApiRoute;
    public string TableName        => Table.TableName;
    public string TableType        => Table.GetTableType().ToString();

    // ── PK — resolución robusta ───────────────────────────────
    // Estrategia de 4 pasos para soportar BDs legacy (Incoder):
    //   1. PrimaryKeyColumn de AIGEN (columna marcada [Key])
    //   2. Primera columna con IsPrimaryKey = true
    //   3. Convención Incoder: primera columna que empiece con "ID"
    //   4. Primera columna como último recurso
    private ColumnMetadata? ResolvePkColumn()
    {
        if (Table.PrimaryKeyColumn is not null)
            return Table.PrimaryKeyColumn;

        var byFlag = Table.Columns.FirstOrDefault(c => c.IsPrimaryKey);
        if (byFlag is not null) return byFlag;

        var byConvention = Table.Columns.FirstOrDefault(c =>
            c.PropertyName.StartsWith("ID", StringComparison.Ordinal) ||
            c.ColumnName.StartsWith("ID", StringComparison.OrdinalIgnoreCase));
        if (byConvention is not null) return byConvention;

        return Table.Columns.FirstOrDefault();
    }

    private ColumnMetadata? _pkColumn;
    private ColumnMetadata? PkColumn => _pkColumn ??= ResolvePkColumn();

    public string PkName      => PkColumn?.PropertyName   ?? "Id";
    public string PkNameCamel => PkColumn?.TsPropertyName ?? "id";
    public string PkType      => PkColumn?.CSharpType     ?? "int";
    public string PkTsType    => PkColumn?.TypeScriptType  ?? "number";

    // ── Columnas ─────────────────────────────────────────────
    // IMPORTANTE: ToScriptObjects aplica el mismo rename que entity.scriban:
    //   si PropertyName == EntityName → PropertyName = EntityName + "Value"
    // Esto garantiza que repositorio y entidad usen el mismo nombre de propiedad.
    // Ejemplo: tabla "Aplicacion" tiene columna "Aplicacion" → propiedad "AplicacionValue"
    public IEnumerable<ScriptObject> AllColumns  => ToScriptObjects(Table.Columns, EntityName);

    // FormColumns: excluye PK, identity y audit fields
    public IEnumerable<ScriptObject> FormColumns => ToScriptObjects(
        Table.FormColumns.Where(c =>
            !c.IsPrimaryKey &&
            !c.IsIdentity  &&
            !c.IsAuditField),
        EntityName);

    public IEnumerable<ScriptObject> ListColumns => ToScriptObjects(Table.ListColumns, EntityName);

    // ── Tipo de tabla ────────────────────────────────────────
    public bool HasFullCrud  => Table.HasFullCrud();
    public bool IsReadOnly   => Table.IsReadOnly();
    public bool IsRelational => Table.IsRelationalOnly();

    // ── HasEstado — FIX: case-insensitive + verificar tipo bool ──
    // La BD puede tener "Estado", "ESTADO", "estado"
    // Solo contar como HasEstado si es tipo bool/BIT (no string)
    // Evita falso positivo en tablas con columna "Estado" de tipo varchar
    private ColumnMetadata? EstadoColumn => Table.Columns.FirstOrDefault(c =>
    c.ColumnName.Equals("Estado", StringComparison.OrdinalIgnoreCase) &&
    c.CSharpType == "bool");

    public bool HasEstado => EstadoColumn is not null;

    // Nombre REAL de la propiedad — "ESTADO" si la columna es mayúsculas
    public string EstadoPropertyName => EstadoColumn?.PropertyName ?? "Estado";

    // ── Columnas de auditoría ────────────────────────────────
    public bool HasEliminadoColumn  => Table.Columns.Any(c =>
        c.PropertyName.Equals("Eliminado", StringComparison.OrdinalIgnoreCase));
    public bool HasCreadoEn         => Table.Columns.Any(c =>
        c.PropertyName.Equals("CreadoEn", StringComparison.OrdinalIgnoreCase));
    public bool HasCreadoPor        => Table.Columns.Any(c =>
        c.PropertyName.Equals("CreadoPor", StringComparison.OrdinalIgnoreCase));
    public bool HasModificadoEn     => Table.Columns.Any(c =>
        c.PropertyName.Equals("ModificadoEn", StringComparison.OrdinalIgnoreCase));
    public bool HasModificadoPor    => Table.Columns.Any(c =>
        c.PropertyName.Equals("ModificadoPor", StringComparison.OrdinalIgnoreCase));
    public bool HasAuditColumns     => Config.Features.Auditing && Table.Columns.Any(c => c.IsAuditField);

    // ── Features ─────────────────────────────────────────────
    public bool UsePagination       => Config.Features.GeneratePagination;
    public bool UseSoftDelete       => Config.Features.SoftDelete;
    public bool UseAuditing         => Config.Features.Auditing;
    public bool UseFluentValidation =>
        Config.Features.Validation == ValidationProvider.FluentValidation;
    public bool UseAutoMapper =>
        Config.Features.Mapping == MappingProvider.AutoMapper;
    public bool UseCQRS    => Config.Backend.UseCQRS;
    public bool UseSwagger =>
        Config.Features.ApiDoc == ApiDocProvider.Swagger;

    // ── ORM ──────────────────────────────────────────────────
    public bool UseDapper   => Config.Backend.Orm == OrmType.Dapper;
    public bool UseEfCore   => Config.Backend.Orm == OrmType.EntityFrameworkCore;
    public bool UseEfDapper => Config.Backend.Orm == OrmType.EFCoreWithDapper;

    // ── Namespaces ───────────────────────────────────────────
    public string NsDomain         => $"{RootNamespace}.Domain.Entities";
    public string NsApplication    => $"{RootNamespace}.Application.{EntityNamePlural}";
    public string NsInfraRepo      => $"{RootNamespace}.Infrastructure.Persistence.Repositories";
    public string NsApi            => $"{RootNamespace}.API.Controllers";
    public string NsInfrastructure => $"{RootNamespace}.Infrastructure";

    // ── Angular ──────────────────────────────────────────────
    public string AngularFileName  => ToKebab(EntityName);
    public string AngularSelector  => "app-" + ToKebab(EntityName);
    public string AngularVersion   => Config.Frontend.FrameworkVersion;
    public string PrimaryColor     => Config.Frontend.PrimaryColor;
    public string SecondaryColor   => Config.Frontend.SecondaryColor;
    public string UiLibrary        => Config.Frontend.UiLibrary.ToString();
    public bool   GenerateFrontend => Config.Frontend.GenerateFrontend;

    // ── Conversión ColumnMetadata → ScriptObject ─────────────
    private static readonly HashSet<string> ValueTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "int", "long", "short", "byte", "float", "double", "decimal",
        "bool", "Guid", "DateTime", "DateTimeOffset", "TimeSpan", "DateOnly", "TimeOnly",
        "int32", "int64", "int16"
    };

    /// <summary>
    /// Convierte columnas a ScriptObject para Scriban.
    /// CRÍTICO: aplica el mismo rename que entity.scriban —
    /// si PropertyName == entityName (nombre de la clase), se renombra a PropertyName + "Value".
    /// Esto evita CS0542 (propiedad con mismo nombre que la clase) y sincroniza
    /// el nombre entre entidad y repositorio/DTO.
    /// </summary>
    private static IEnumerable<ScriptObject> ToScriptObjects(
        IEnumerable<ColumnMetadata> cols,
        string entityName)
    {
        foreach (var col in cols)
        {
            // Aplicar el mismo rename que entity.scriban
            var propertyName = col.PropertyName == entityName
                ? col.PropertyName + "Value"
                : col.PropertyName;

            var obj = new ScriptObject();
            obj["column_name"]           = col.ColumnName;
            obj["property_name"]         = propertyName;           // ← nombre real en la entidad
            obj["original_property_name"]= col.PropertyName;       // ← nombre original del metadata
            obj["ts_property_name"]      = col.TsPropertyName;
            obj["display_name"]          = col.DisplayName;
            obj["sql_type"]              = col.SqlType;
            obj["c_sharp_type"]          = col.CSharpType;
            obj["c_sharp_type_nullable"] = col.CSharpTypeNullable;
            obj["type_script_type"]      = col.TypeScriptType;
            obj["is_nullable"]           = col.IsNullable;
            obj["is_primary_key"]        = col.IsPrimaryKey;
            obj["is_identity"]           = col.IsIdentity;
            obj["is_required"]           = col.IsRequired;
            obj["is_audit_field"]        = col.IsAuditField;
            obj["is_numeric"]            = col.IsNumeric;
            obj["is_date"]               = col.IsDate;
            obj["max_length"]            = col.MaxLength;
            obj["has_max_length"]        = col.HasMaxLength;
            obj["precision"]             = col.Precision;
            obj["scale"]                 = col.Scale;
            obj["has_default_value"]     = col.HasDefaultValue;
            obj["default_value"]         = col.DefaultValue ?? "";
            obj["ordinal_position"]      = col.OrdinalPosition;

            // is_value_type: true para structs (int, bool, Guid, DateTime...)
            // Usado en repository_ef.scriban para decidir .HasValue / .Value
            obj["is_value_type"] = ValueTypes.Contains(col.CSharpType);

            yield return obj;
        }
    }

    private static string ToKebab(string s) =>
        System.Text.RegularExpressions.Regex
            .Replace(s, "([A-Z])", "-$1").TrimStart('-').ToLower();
}









