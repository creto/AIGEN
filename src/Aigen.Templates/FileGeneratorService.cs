using Aigen.Core.Config;
using Aigen.Core.Config.Enums;
using Aigen.Core.Metadata;
using Aigen.Templates.Engine;

namespace Aigen.Templates;

public class FileGeneratorService
{
    private readonly ITemplateEngine _engine;
    private readonly TemplateLocator _locator;

    public FileGeneratorService(ITemplateEngine engine, TemplateLocator locator)
    {
        _engine  = engine;
        _locator = locator;
    }

    public async Task<GenerationResult> GenerateAsync(
        GeneratorConfig        config,
        DatabaseMetadata       db,
        List<TableMetadata>    tables,
        IProgress<GenerationProgress>? progress = null,
        CancellationToken      ct = default)
    {
        var result  = new GenerationResult();
        var outPath = config.ResolveOutputPath();

        // ── Desambiguar ClassName duplicados ─────────────────────────────────
        // Si dos tablas generan el mismo ClassName (ej. TB_Serie y TR_Serie -> Serie),
        // la segunda recibe el prefijo de tabla como sufijo: SerieTr, SerieTb, etc.
        var classNameSeen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var t in tables)
        {
            if (!classNameSeen.Add(t.ClassName))
            {
                // Extraer prefijo: "TR_Serie" -> "Tr", "TBR_Algo" -> "Tbr"
                var rawPrefix = t.TableName.Split('_')[0]; // "TR", "TBR", "TM"...
                var suffix    = rawPrefix.Length > 0
                    ? char.ToUpper(rawPrefix[0]) + rawPrefix[1..].ToLower()
                    : "Alt";
                var disambig  = t.ClassName + suffix;        // "SerieTr"

                // Recalcular todos los nombres derivados
                t.ClassName       = disambig;
                t.ClassNamePlural = t.ClassNamePlural + suffix;    // "SeriesTr"
                t.ObjectName      = char.ToLower(disambig[0]) + disambig[1..];
                t.ServiceName     = disambig + "Service";
                t.RepositoryName  = disambig + "Repository";
                t.ControllerName  = disambig + "sController";
                t.ApiRoute        = "/api/" + disambig.ToLowerInvariant() + "s";
            }
        }
        // ─────────────────────────────────────────────────────────────────────

        for (int i = 0; i < tables.Count; i++)
        {
            var table = tables[i];
            progress?.Report(new(i + 1, tables.Count, table.ClassName));

            var ctx = new TemplateContext { Table = table, Db = db, Config = config };

            await GenerateBackendAsync(ctx, outPath, result, ct);

            // Frontend solo para tablas no-audit/no-historical
            // TA_, TH_, TAR_ solo generan Entity en backend — sin frontend
            var skipFrontend = table.TableName.StartsWith("TA_",  StringComparison.OrdinalIgnoreCase)
                            || table.TableName.StartsWith("TH_",  StringComparison.OrdinalIgnoreCase)
                            || table.TableName.StartsWith("TAR_", StringComparison.OrdinalIgnoreCase);

            if (config.Frontend.GenerateFrontend && !skipFrontend)
                await GenerateFrontendAsync(ctx, outPath, result, ct);
        }

        if (tables.Count > 0)
        {
            var solutionCtx = new TemplateContext
                { Table = tables[0], Db = db, Config = config, AllTables = tables };
            await GenerateSolutionAsync(solutionCtx, outPath, db, config, result, ct);
        }

        result.IsSuccess = result.Errors.Count == 0;
        return result;
    }

    // â”€â”€ Backend â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private async Task GenerateBackendAsync(
        TemplateContext ctx, string outPath,
        GenerationResult result, CancellationToken ct)
    {
        var t  = ctx.Table;
        var ns = ctx.ProjectName;

        // Entity (siempre â€” incluso para TH_/TA_ para que las FKs resuelvan)
        await Save(ctx, "entity.scriban",
            Path.Combine(outPath, "src", $"{ns}.Domain", "Entities",
                $"{t.ClassName}.cs"), result, ct);

        // TH_ (Historical) y TA_ (Audit) solo generan Entity â€” no Application/API
        var isAuditOrHistorical = t.TableName.StartsWith("TH_", StringComparison.OrdinalIgnoreCase)
                               || t.TableName.StartsWith("TA_", StringComparison.OrdinalIgnoreCase)
                               || t.TableName.StartsWith("TAR_", StringComparison.OrdinalIgnoreCase);
        t.HasRepository = false;
        t.HasService    = false;
        if (isAuditOrHistorical) return;

        // Tablas sin PK no pueden tener repositorio/service/controller
        if (t.PrimaryKeyColumn is null || t.PrimaryKeys.Count == 0) return;
        // DTO (siempre)
        await Save(ctx, "dto.scriban",
            Path.Combine(outPath, "src", $"{ns}.Application",
                t.ClassNamePlural, "DTOs", $"{t.ClassName}Dto.cs"), result, ct);

        // IRepository (siempre)
        await Save(ctx, "irepository.scriban",
            Path.Combine(outPath, "src", $"{ns}.Application",
                "Interfaces", $"I{t.RepositoryName}.cs"), result, ct);
        if (!ctx.HasFullCrud) return;
        t.HasRepository = true;
        t.HasService    = true;

        // Service
        await Save(ctx, "service.scriban",
            Path.Combine(outPath, "src", $"{ns}.Application",
                t.ClassNamePlural, $"{t.ServiceName}.cs"), result, ct);

        // Repository: Dapper | EF Core | EFCore+Dapper
        var orm = ctx.Config.Backend.Orm;
        if (orm == OrmType.Dapper)
        {
            await Save(ctx, "repository_dapper.scriban",
                Path.Combine(outPath, "src", $"{ns}.Infrastructure",
                    "Persistence", "Repositories", $"{t.RepositoryName}.cs"),
                result, ct);
        }
        else if (orm == OrmType.EntityFrameworkCore)
        {
            await Save(ctx, "repository_ef.scriban",
                Path.Combine(outPath, "src", $"{ns}.Infrastructure",
                    "Persistence", "Repositories", $"{t.RepositoryName}.cs"),
                result, ct);

            // EF Fluent API configuration
            await Save(ctx, "entity_configuration.scriban",
                Path.Combine(outPath, "src", $"{ns}.Infrastructure",
                    "Persistence", "Configurations",
                    $"{t.ClassName}Configuration.cs"), result, ct);
        }
        else // EFCoreWithDapper: EF para CRUD, Dapper para queries
        {
            await Save(ctx, "repository_ef.scriban",
                Path.Combine(outPath, "src", $"{ns}.Infrastructure",
                    "Persistence", "Repositories", $"{t.RepositoryName}.cs"),
                result, ct);

            await Save(ctx, "entity_configuration.scriban",
                Path.Combine(outPath, "src", $"{ns}.Infrastructure",
                    "Persistence", "Configurations",
                    $"{t.ClassName}Configuration.cs"), result, ct);
        }

        // Controller
        await Save(ctx, "controller.scriban",
            Path.Combine(outPath, "src", $"{ns}.API",
                "Controllers", $"{t.ControllerName}.cs"), result, ct);

        // Validator
        if (ctx.UseFluentValidation)
            await Save(ctx, "validator.scriban",
                Path.Combine(outPath, "src", $"{ns}.Application",
                    t.ClassNamePlural, $"{t.ClassName}Validator.cs"), result, ct);
    }

    // â”€â”€ Frontend â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private async Task GenerateFrontendAsync(
        TemplateContext ctx, string outPath,
        GenerationResult result, CancellationToken ct)
    {
        var kebab  = ctx.AngularFileName;
        var fePath = Path.Combine(outPath, "frontend", "src", "app",
            "features", kebab);

        await Save(ctx, "angular_model.scriban",
            Path.Combine(fePath, "models", $"{kebab}.model.ts"), result, ct);

        if (ctx.IsReadOnly) return;

        await Save(ctx, "angular_service.scriban",
            Path.Combine(fePath, "services", $"{kebab}.service.ts"), result, ct);

        await Save(ctx, "angular_list_component.scriban",
            Path.Combine(fePath, "components", $"{kebab}-list",
                $"{kebab}-list.component.ts"), result, ct);

        await Save(ctx, "angular_form_component.scriban",
            Path.Combine(fePath, "components", $"{kebab}-form",
                $"{kebab}-form.component.ts"), result, ct);
    }

    // â”€â”€ Solution â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private async Task GenerateSolutionAsync(
        TemplateContext ctx, string outPath,
        DatabaseMetadata db, GeneratorConfig config,
        GenerationResult result, CancellationToken ct)
    {
        var ns  = ctx.ProjectName;
        var orm = config.Backend.Orm;

        // â”€â”€ Archivos de infraestructura .NET (NUEVOS) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        // 1. Solution file (.sln)
        await Save(ctx, "solution.scriban",
            Path.Combine(outPath, $"{ns}.sln"), result, ct);

        // 2. Domain.csproj
        await Save(ctx, "domain_csproj.scriban",
            Path.Combine(outPath, "src", $"{ns}.Domain",
                $"{ns}.Domain.csproj"), result, ct);

        // 3. Application.csproj
        await Save(ctx, "application_csproj.scriban",
            Path.Combine(outPath, "src", $"{ns}.Application",
                $"{ns}.Application.csproj"), result, ct);

        // 4. Infrastructure.csproj
        await Save(ctx, "infrastructure_csproj.scriban",
            Path.Combine(outPath, "src", $"{ns}.Infrastructure",
                $"{ns}.Infrastructure.csproj"), result, ct);

        // 5. API.csproj
        await Save(ctx, "api_csproj.scriban",
            Path.Combine(outPath, "src", $"{ns}.API",
                $"{ns}.API.csproj"), result, ct);

        // â”€â”€ Archivos de aplicaciÃ³n (ya existÃ­an) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        // DbContext (EF Core)
        if (orm != OrmType.Dapper)
            await Save(ctx, "dbcontext.scriban",
                Path.Combine(outPath, "src", $"{ns}.Infrastructure",
                    "Persistence", $"{ns}DbContext.cs"), result, ct);

        // Program.cs
        await Save(ctx, "program.scriban",
            Path.Combine(outPath, "src", $"{ns}.API", "Program.cs"), result, ct);

        // appsettings.json
        await Save(ctx, "appsettings.scriban",
            Path.Combine(outPath, "src", $"{ns}.API", "appsettings.json"), result, ct);

        // EF Migrations README
        if (orm != OrmType.Dapper)
            await Save(ctx, "efcore_migration_hint.scriban",
                Path.Combine(outPath, "docs", "EF-MIGRATIONS.md"), result, ct);

        // ARCHITECTURE.md
        await Save(ctx, "architecture.scriban",
            Path.Combine(outPath, "ARCHITECTURE.md"), result, ct);

        // DEPLOYMENT.md
        await Save(ctx, "deployment.scriban",
            Path.Combine(outPath, "DEPLOYMENT.md"), result, ct);

        // -- Angular scaffolding -------------------------------------------
        if (config.Frontend.GenerateFrontend)
        {
            var fe = Path.Combine(outPath, "frontend");
            await Save(ctx, "angular_package_json.scriban",  Path.Combine(fe, "package.json"),                          result, ct);
            await Save(ctx, "angular_json.scriban",          Path.Combine(fe, "angular.json"),                          result, ct);
            await Save(ctx, "angular_tsconfig.scriban",      Path.Combine(fe, "tsconfig.json"),                         result, ct);
            await Save(ctx, "angular_tsconfig_app.scriban",  Path.Combine(fe, "tsconfig.app.json"),                     result, ct);
            await Save(ctx, "angular_proxy_conf.scriban",    Path.Combine(fe, "proxy.conf.json"),                       result, ct);
            await Save(ctx, "angular_environment.scriban",      Path.Combine(fe, "src", "environments", "environment.ts"),      result, ct);
            await Save(ctx, "angular_environment_prod.scriban", Path.Combine(fe, "src", "environments", "environment.prod.ts"), result, ct);
            await Save(ctx, "angular_error_interceptor.scriban", Path.Combine(fe, "src", "app", "core", "interceptors", "error.interceptor.ts"), result, ct);
            await Save(ctx, "angular_auth_interceptor.scriban",  Path.Combine(fe, "src", "app", "core", "interceptors", "auth.interceptor.ts"),  result, ct);
            await Save(ctx, "angular_main.scriban",          Path.Combine(fe, "src", "main.ts"),                        result, ct);
            await Save(ctx, "angular_index_html.scriban",    Path.Combine(fe, "src", "index.html"),                     result, ct);
            await Save(ctx, "angular_styles.scriban",        Path.Combine(fe, "src", "styles.scss"),                    result, ct);
            await Save(ctx, "angular_app_config.scriban",    Path.Combine(fe, "src", "app", "app.config.ts"),           result, ct);
            await Save(ctx, "angular_app_routes.scriban",    Path.Combine(fe, "src", "app", "app.routes.ts"),           result, ct);
            await Save(ctx, "angular_app_component.scriban", Path.Combine(fe, "src", "app", "app.component.ts"),        result, ct);
            await Save(ctx, "angular_dashboard.scriban",     Path.Combine(fe, "src", "app", "dashboard", "dashboard.component.ts"), result, ct);
        }




        
    }

    // â”€â”€ Helper â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private async Task Save(
        TemplateContext ctx, string templateName,
        string outputPath, GenerationResult result, CancellationToken ct)
    {
        try
        {
            var tplPath = _locator.Resolve(templateName);
            if (!File.Exists(tplPath))
            {
                result.Warnings.Add($"Plantilla no encontrada: {templateName}");
                return;
            }
            var content = await _engine.RenderFileAsync(tplPath, ctx, ct);
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            await File.WriteAllTextAsync(outputPath, content,
                System.Text.Encoding.UTF8, ct);
            result.Files.Add(outputPath);
        }
        catch (Exception ex)
        {
            result.Errors.Add($"[{Path.GetFileName(outputPath)}] {ex.Message}");
        }
    }
}

public class GenerationResult
{
    public List<string> Files    { get; } = new();
    public List<string> Errors   { get; } = new();
    public List<string> Warnings { get; } = new();
    public bool IsSuccess        { get; set; }
    public int TotalFiles        => Files.Count;
}

public record GenerationProgress(int Current, int Total, string TableName);











