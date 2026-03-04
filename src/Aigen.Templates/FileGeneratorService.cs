using Aigen.Core.Config;
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

        for (int i = 0; i < tables.Count; i++)
        {
            var table = tables[i];
            progress?.Report(new(i + 1, tables.Count, table.ClassName));

            var ctx = new TemplateContext { Table = table, Db = db, Config = config };

            await GenerateBackendAsync(ctx, outPath, result, ct);

            if (config.Frontend.GenerateFrontend)
                await GenerateFrontendAsync(ctx, outPath, result, ct);
        }

        if (tables.Count > 0)
        {
            var solutionCtx = new TemplateContext
                { Table = tables[0], Db = db, Config = config };
            await GenerateSolutionAsync(solutionCtx, outPath, result, ct);
        }

        result.IsSuccess = result.Errors.Count == 0;
        return result;
    }

    private async Task GenerateBackendAsync(
        TemplateContext ctx, string outPath,
        GenerationResult result, CancellationToken ct)
    {
        var t  = ctx.Table;
        var ns = ctx.ProjectName;

        await Save(ctx, "entity.scriban",
            Path.Combine(outPath, "src", $"{ns}.Domain", "Entities", $"{t.ClassName}.cs"),
            result, ct);

        await Save(ctx, "dto.scriban",
            Path.Combine(outPath, "src", $"{ns}.Application",
                t.ClassNamePlural, "DTOs", $"{t.ClassName}Dto.cs"),
            result, ct);

        await Save(ctx, "irepository.scriban",
            Path.Combine(outPath, "src", $"{ns}.Application",
                "Interfaces", $"I{t.RepositoryName}.cs"),
            result, ct);

        if (!ctx.HasFullCrud) return;

        await Save(ctx, "service.scriban",
            Path.Combine(outPath, "src", $"{ns}.Application",
                t.ClassNamePlural, $"{t.ServiceName}.cs"),
            result, ct);

        var repoTpl = ctx.UseDapper
            ? "repository_dapper.scriban" : "repository_ef.scriban";
        await Save(ctx, repoTpl,
            Path.Combine(outPath, "src", $"{ns}.Infrastructure",
                "Persistence", "Repositories", $"{t.RepositoryName}.cs"),
            result, ct);

        await Save(ctx, "controller.scriban",
            Path.Combine(outPath, "src", $"{ns}.API",
                "Controllers", $"{t.ControllerName}.cs"),
            result, ct);

        if (ctx.UseFluentValidation)
            await Save(ctx, "validator.scriban",
                Path.Combine(outPath, "src", $"{ns}.Application",
                    t.ClassNamePlural, $"{t.ClassName}Validator.cs"),
                result, ct);
    }

    private async Task GenerateFrontendAsync(
        TemplateContext ctx, string outPath,
        GenerationResult result, CancellationToken ct)
    {
        var kebab  = ctx.AngularFileName;
        var fePath = Path.Combine(outPath, "frontend", "src", "app", "features", kebab);

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

    private async Task GenerateSolutionAsync(
        TemplateContext ctx, string outPath,
        GenerationResult result, CancellationToken ct)
    {
        var ns = ctx.ProjectName;
        await Save(ctx, "program.scriban",
            Path.Combine(outPath, "src", $"{ns}.API", "Program.cs"), result, ct);
        await Save(ctx, "appsettings.scriban",
            Path.Combine(outPath, "src", $"{ns}.API", "appsettings.json"), result, ct);
    }

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
