using Aigen.Core.Config;
using Aigen.Core.Config.Enums;
using Aigen.Core.Metadata;
using Scriban.Runtime;

namespace Aigen.Templates.Engine;

/// <summary>
/// Contexto completo inyectado en cada plantilla Scriban.
/// Propiedades disponibles en snake_case: entity_name, pk_type, api_route...
/// IMPORTANTE: Las columnas se convierten a ScriptObject para que Scriban
/// pueda acceder a sus propiedades via snake_case (c_sharp_type, is_nullable...).
/// </summary>
public class TemplateContext
{
    public required TableMetadata    Table  { get; init; }
    public required DatabaseMetadata Db     { get; init; }
    public required GeneratorConfig  Config { get; init; }

    // ── Proyecto ─────────────────────────────────────────────
    public string ProjectName   => Config.Project.ProjectName;
    public string Author        => Config.Project.Author;
    public string GeneratedDate => DateTime.Now.ToString("yyyy-MM-dd");
    public string DotnetVersion  => Config.Backend.TargetFramework;   // "net8.0"
    public string RootNamespace  => Config.Project.RootNamespace;      // "Doc4us.SGDEA"
    public string Version        => Config.Project.Version;            // "1.0.0"
    public string Orm            => Config.Backend.Orm.ToString();     // "EFCoreWithDapper"

    // Nota: en Scriban estas se expondrán como:
    //   {{ dotnet_version }}
    //   {{ root_namespace }}
    //   {{ version }}
    //   {{ orm }}

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

    // ── PK ───────────────────────────────────────────────────
    public string PkName      => Table.PrimaryKeyColumn?.PropertyName   ?? "Id";
    public string PkNameCamel => Table.PrimaryKeyColumn?.TsPropertyName ?? "id";
    public string PkType      => Table.PrimaryKeyColumn?.CSharpType     ?? "int";
    public string PkTsType    => Table.PrimaryKeyColumn?.TypeScriptType  ?? "number";

    // ── Columnas — convertidas a ScriptObject para Scriban ───
    // Scriban no puede leer propiedades de C# POCO en listas via Import.
    // Se deben convertir a ScriptObject (diccionario) con claves snake_case.
    public IEnumerable<ScriptObject> AllColumns  => ToScriptObjects(Table.Columns);
    public IEnumerable<ScriptObject> FormColumns => ToScriptObjects(Table.FormColumns);
    public IEnumerable<ScriptObject> ListColumns => ToScriptObjects(Table.ListColumns);

    // ── Tipo de tabla ────────────────────────────────────────
    public bool HasFullCrud  => Table.HasFullCrud();
    public bool IsReadOnly   => Table.IsReadOnly();
    public bool HasEstado    => Table.HasEstadoField();
    public bool IsRelational => Table.IsRelationalOnly();

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
    public string NsDomain      => $"{RootNamespace}.Domain.Entities";
    public string NsApplication => $"{RootNamespace}.Application.{EntityNamePlural}";
    public string NsInfraRepo   => $"{RootNamespace}.Infrastructure.Persistence.Repositories";
    public string NsApi         => $"{RootNamespace}.API.Controllers";
    public string NsInfrastructure => $"{RootNamespace}.Infrastructure";

    // ── Angular ──────────────────────────────────────────────
    public string AngularFileName  => ToKebab(EntityName);
    public string AngularSelector  => "app-" + ToKebab(EntityName);
    public string AngularVersion   => Config.Frontend.FrameworkVersion;
    public string UiLibrary        => Config.Frontend.UiLibrary.ToString();
    public bool   GenerateFrontend => Config.Frontend.GenerateFrontend;

    // ── Metadatos de generación ───────────────────────────────
    public string Year        => Config.Project.Year;
    public string Description => Config.Project.Description;

    // ── Conversión ColumnMetadata → ScriptObject ─────────────
    private static IEnumerable<ScriptObject> ToScriptObjects(
        IEnumerable<ColumnMetadata> cols)
    {
        foreach (var col in cols)
        {
            var obj = new ScriptObject();
            obj["column_name"]          = col.ColumnName;
            obj["property_name"]        = col.PropertyName;
            obj["ts_property_name"]     = col.TsPropertyName;
            obj["display_name"]         = col.DisplayName;
            obj["sql_type"]             = col.SqlType;
            obj["c_sharp_type"]         = col.CSharpType;
            obj["c_sharp_type_nullable"]= col.CSharpTypeNullable;
            obj["type_script_type"]     = col.TypeScriptType;
            obj["is_nullable"]          = col.IsNullable;
            obj["is_primary_key"]       = col.IsPrimaryKey;
            obj["is_identity"]          = col.IsIdentity;
            obj["is_required"]          = col.IsRequired;
            obj["is_audit_field"]       = col.IsAuditField;
            obj["is_numeric"]           = col.IsNumeric;
            obj["is_date"]              = col.IsDate;
            obj["max_length"]           = col.MaxLength;
            obj["has_max_length"]       = col.HasMaxLength;
            obj["precision"]            = col.Precision;
            obj["scale"]                = col.Scale;
            obj["has_default_value"]    = col.HasDefaultValue;
            obj["default_value"]        = col.DefaultValue ?? "";
            obj["ordinal_position"]     = col.OrdinalPosition;
            yield return obj;
        }
    }

    private static string ToKebab(string s) =>
        System.Text.RegularExpressions.Regex
            .Replace(s, "([A-Z])", "-$1").TrimStart('-').ToLower();
}
