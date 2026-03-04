using Aigen.Core.Config;
using Aigen.Core.Config.Enums;
using Aigen.Core.Metadata;

namespace Aigen.Templates.Engine;

/// <summary>
/// Contexto completo inyectado en cada plantilla Scriban.
/// Propiedades disponibles en snake_case: entity_name, pk_type, api_route...
/// </summary>
public class TemplateContext
{
    public required TableMetadata    Table  { get; init; }
    public required DatabaseMetadata Db     { get; init; }
    public required GeneratorConfig  Config { get; init; }

    // ── Proyecto ─────────────────────────────────────────────
    public string ProjectName   => Config.Project.ProjectName;
    public string RootNamespace => Config.Project.RootNamespace;
    public string Author        => Config.Project.Author;
    public string Version       => Config.Project.Version;
    public string GeneratedDate => DateTime.Now.ToString("yyyy-MM-dd");

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

    // ── Columnas ─────────────────────────────────────────────
    public IEnumerable<ColumnMetadata> AllColumns  => Table.Columns;
    public IEnumerable<ColumnMetadata> FormColumns => Table.FormColumns;
    public IEnumerable<ColumnMetadata> ListColumns => Table.ListColumns;

    // ── Tipo de tabla ─────────────────────────────────────────
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
    public string NsDomain    => $"{RootNamespace}.Domain.Entities";
    public string NsApplication => $"{RootNamespace}.Application.{EntityNamePlural}";
    public string NsInfraRepo => $"{RootNamespace}.Infrastructure.Persistence.Repositories";
    public string NsApi       => $"{RootNamespace}.API.Controllers";

    // ── Angular ──────────────────────────────────────────────
    public string AngularFileName => ToKebab(EntityName);
    public string AngularSelector => "app-" + ToKebab(EntityName);

    private static string ToKebab(string s) =>
        System.Text.RegularExpressions.Regex
            .Replace(s, "([A-Z])", "-$1").TrimStart('-').ToLower();
}
