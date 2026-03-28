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

    // â”€â”€ Proyecto â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
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

    // â”€â”€ Base de datos â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public string DbName   => Db.DatabaseName;
    public string DbSchema => Config.Database.Schema;
    public string DbEngine => Config.Database.Engine.ToString();

    // â”€â”€ Entidad â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public string EntityName       => Table.ClassName;
    public string EntityNamePlural => Table.ClassNamePlural;
    public string EntityNameCamel  => Table.ObjectName;
    public string ServiceName      => Table.ServiceName;
    public string RepoName         => Table.RepositoryName;
    public string ControllerName   => Table.ControllerName;
    public string ApiRoute         => Table.ApiRoute;
    public string TableName        => Table.TableName;
    public string TableType        => Table.GetTableType().ToString();

    // â”€â”€ PK â€” resoluciÃ³n robusta â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // Estrategia de 4 pasos para soportar BDs legacy (Incoder):
    //   1. PrimaryKeyColumn de AIGEN (columna marcada [Key])
    //   2. Primera columna con IsPrimaryKey = true
    //   3. ConvenciÃ³n Incoder: primera columna que empiece con "ID"
    //   4. Primera columna como Ãºltimo recurso
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

    // â”€â”€ Columnas â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // IMPORTANTE: ToScriptObjects aplica el mismo rename que entity.scriban:
    //   si PropertyName == EntityName â†’ PropertyName = EntityName + "Value"
    // Esto garantiza que repositorio y entidad usen el mismo nombre de propiedad.
    // Ejemplo: tabla "Aplicacion" tiene columna "Aplicacion" â†’ propiedad "AplicacionValue"
    public IEnumerable<ScriptObject> AllColumns  => ToScriptObjects(Table.Columns, EntityName);

    // FormColumns: excluye PK, identity y audit fields
    public IEnumerable<ScriptObject> FormColumns => ToScriptObjects(
        Table.FormColumns.Where(c =>
            !c.IsPrimaryKey &&
            !c.IsIdentity  &&
            !c.IsAuditField),
        EntityName);

    public IEnumerable<ScriptObject> ListColumns => ToScriptObjects(Table.ListColumns, EntityName);

    // â”€â”€ Tipo de tabla â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public bool HasFullCrud  => Table.HasFullCrud();
    public bool IsReadOnly   => Table.IsReadOnly();
    public bool IsRelational => Table.IsRelationalOnly();

    // â”€â”€ HasEstado â€” FIX: case-insensitive + verificar tipo bool â”€â”€
    // La BD puede tener "Estado", "ESTADO", "estado"
    // Solo contar como HasEstado si es tipo bool/BIT (no string)
    // Evita falso positivo en tablas con columna "Estado" de tipo varchar
    private ColumnMetadata? EstadoColumn => Table.Columns.FirstOrDefault(c =>
    c.ColumnName.Equals("Estado", StringComparison.OrdinalIgnoreCase) &&
    c.CSharpType == "bool");

    public bool HasEstado => EstadoColumn is not null;

    // Nombre REAL de la propiedad â€” "ESTADO" si la columna es mayÃºsculas
    public string EstadoPropertyName => EstadoColumn?.PropertyName ?? "Estado";

    // â”€â”€ Columnas de auditorÃ­a â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
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

    // â”€â”€ Features â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
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

    // â”€â”€ ORM â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public bool UseDapper   => Config.Backend.Orm == OrmType.Dapper;
    public bool UseEfCore   => Config.Backend.Orm == OrmType.EntityFrameworkCore;
    public bool UseEfDapper => Config.Backend.Orm == OrmType.EFCoreWithDapper;

    // â”€â”€ Namespaces â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public string NsDomain         => $"{RootNamespace}.Domain.Entities";
    public string NsApplication    => $"{RootNamespace}.Application.{EntityNamePlural}";
    public string NsInfraRepo      => $"{RootNamespace}.Infrastructure.Persistence.Repositories";
    public string NsApi            => $"{RootNamespace}.API.Controllers";
    public string NsInfrastructure => $"{RootNamespace}.Infrastructure";

    // â”€â”€ Angular â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public string AngularFileName  => ToKebab(EntityName);
    public string AngularSelector  => "app-" + ToKebab(EntityName);
    public string AngularVersion   => Config.Frontend.FrameworkVersion;
    public string PrimaryColor     => Config.Frontend.PrimaryColor;
    public string SecondaryColor   => Config.Frontend.SecondaryColor;
    public string ApiBaseUrl       => Config.Frontend.ApiBaseUrl;
    public string ApiBaseProdUrl   => Config.Frontend.ApiBaseProdUrl;
    // Architecture / Microservices
    public string  ArchStyle             => Config.Architecture.Style.ToString();
    public bool    IsMicroservices        => Config.Architecture.Style == Aigen.Core.Config.Enums.OutputStyle.Microservices;
    public bool    SeparateSolution       => Config.Architecture.SeparateSolutionPerService;
    public string  GatewayTechnology      => Config.Architecture.Gateway.Technology;
    public int     GatewayPort            => Config.Architecture.Gateway.Port;
    public bool    GenerateGateway        => Config.Architecture.Gateway.GenerateGateway;
    // Lista de ClassNames del microservicio actual — usado en entity.scriban
    // para filtrar navigation properties hacia entidades externas al servicio
    public List<string> MicroserviceClassNames { get; init; } = new();
    public bool IsMicroserviceMode => MicroserviceClassNames.Count > 0;
    // ScriptArray para que Scriban pueda usar array.contains
    public Scriban.Runtime.ScriptArray MicroserviceClassNamesArray =>
        new Scriban.Runtime.ScriptArray(MicroserviceClassNames);
    public string JwtKey             => Config.Security.JwtKey;
    public string JwtIssuer          => Config.Security.JwtIssuer;
    public string JwtAudience        => Config.Security.JwtAudience;
    public int    JwtExpiresMinutes  => Config.Security.JwtExpiresMinutes;
    public int    RefreshExpiresDays => Config.Security.RefreshExpiresDays;
    public bool   UseRefreshToken    => Config.Security.UseRefreshToken;
    public string OidcProvider       => Config.Security.OidcProvider;
    public bool   UseJwt            => Config.Security.Authentication == Aigen.Core.Config.Enums.AuthenticationType.Jwt;
    public string UiLibrary        => Config.Frontend.UiLibrary.ToString();
    public bool   GenerateFrontend => Config.Frontend.GenerateFrontend;

    // â”€â”€ ConversiÃ³n ColumnMetadata â†’ ScriptObject â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private static readonly HashSet<string> ValueTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "int", "long", "short", "byte", "float", "double", "decimal",
        "bool", "Guid", "DateTime", "DateTimeOffset", "TimeSpan", "DateOnly", "TimeOnly",
        "int32", "int64", "int16"
    };

    /// <summary>
    /// Convierte columnas a ScriptObject para Scriban.
    /// CRÃTICO: aplica el mismo rename que entity.scriban â€”
    /// si PropertyName == entityName (nombre de la clase), se renombra a PropertyName + "Value".
    /// Esto evita CS0542 (propiedad con mismo nombre que la clase) y sincroniza
    /// el nombre entre entidad y repositorio/DTO.
    /// </summary>
    // FKs como ScriptObject con snake_case explícito — ScriptObject.Import no renombra objetos anidados
    public List<ScriptObject> ForeignKeysScript
    {
        get
        {
            var result = new List<ScriptObject>();
            foreach (var fk in Table.ForeignKeys)
            {
                var obj = new ScriptObject();
                obj["constraint_name"]              = fk.ConstraintName;
                obj["column_name"]                  = fk.ColumnName;
                obj["referenced_table"]             = fk.ReferencedTable;
                obj["referenced_column"]            = fk.ReferencedColumn;
                obj["referenced_schema"]            = fk.ReferencedSchema;
                obj["navigation_property_name"]     = fk.NavigationPropertyName;
                obj["property_name"]                = fk.PropertyName;
                obj["local_fk_c_sharp_type"]        = fk.LocalFkCSharpType;
                obj["referenced_pk_c_sharp_type"]   = fk.ReferencedPkCSharpType;
                result.Add(obj);
            }
            return result;
        }
    }

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
            obj["property_name"]         = propertyName;           // â† nombre real en la entidad
            obj["original_property_name"]= col.PropertyName;       // â† nombre original del metadata
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

    private static string ToKebab(string s)
    {
        // Paso 1: acrÃ³nimo + palabra normal  "MCDTAlgo" â†’ "MCDT-Algo"
        var r = System.Text.RegularExpressions.Regex
            .Replace(s, @"([A-Z]+)([A-Z][a-z])", "$1-$2");
        // Paso 2: minÃºscula/dÃ­gito + mayÃºscula  "OrdenCompra" â†’ "Orden-Compra"
        r = System.Text.RegularExpressions.Regex
            .Replace(r, @"([a-z\d])([A-Z])", "$1-$2");
        return r.ToLowerInvariant();
    }
}











