namespace Aigen.Templates.Engine;

public interface ITemplateEngine
{
    Task<string> RenderFileAsync(
        string templatePath,
        TemplateContext context,
        CancellationToken ct = default);

    Task<string> RenderStringAsync(
        string template,
        TemplateContext context,
        CancellationToken ct = default);
}
