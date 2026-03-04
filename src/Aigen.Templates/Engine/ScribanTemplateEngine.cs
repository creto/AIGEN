using Scriban;
using Scriban.Runtime;
using Aigen.Templates.Engine;

namespace Aigen.Templates;

/// <summary>
/// Motor de plantillas basado en Scriban.
/// Convierte archivos .scriban en C#, TypeScript, JSON, YAML, etc.
/// Las propiedades del TemplateContext se exponen en snake_case:
///   EntityName -> entity_name
///   PkType     -> pk_type
/// </summary>
public class ScribanTemplateEngine : ITemplateEngine
{
    public async Task<string> RenderFileAsync(
        string templatePath, TemplateContext ctx, CancellationToken ct = default)
    {
        var text = await File.ReadAllTextAsync(templatePath, ct);
        return await RenderStringAsync(text, ctx, ct);
    }

    public Task<string> RenderStringAsync(
        string templateText, TemplateContext ctx, CancellationToken ct = default)
    {
        var tpl = Template.Parse(templateText);
        if (tpl.HasErrors)
        {
            var msgs = string.Join("\n", tpl.Messages.Select(m => m.ToString()));
            throw new InvalidOperationException($"Error en plantilla:\n{msgs}");
        }

        var globals = new ScriptObject();

        // Importar contexto con conversion snake_case automatica
        globals.Import(ctx, renamer: m => ToSnake(m.Name));

        // Funciones utilitarias disponibles en todas las plantillas
        globals["pascal"]    = new Func<string, string>(ToPascal);
        globals["camel"]     = new Func<string, string>(ToCamel);
        globals["kebab"]     = new Func<string, string>(ToKebab);
        globals["upper"]     = new Func<string, string>(s => s.ToUpper());
        globals["lower"]     = new Func<string, string>(s => s.ToLower());
        globals["nullable_cs"] = new Func<string, bool, string>(
            (t, n) => n && t != "string" ? t + "?" : t);
        globals["now"]       = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

        var scribanCtx = new Scriban.TemplateContext { StrictVariables = false };
        scribanCtx.PushGlobal(globals);

        return Task.FromResult(tpl.Render(scribanCtx));
    }

    private static string ToSnake(string name)
    {
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < name.Length; i++)
        {
            if (char.IsUpper(name[i]) && i > 0) sb.Append('_');
            sb.Append(char.ToLower(name[i]));
        }
        return sb.ToString();
    }

    private static string ToPascal(string s) =>
        string.Concat(s.Split('_', ' ', '-')
            .Select(w => w.Length > 0 ? char.ToUpper(w[0]) + w[1..] : w));

    private static string ToCamel(string s)
    {
        var p = ToPascal(s); return p.Length > 0 ? char.ToLower(p[0]) + p[1..] : p;
    }

    private static string ToKebab(string s) =>
        System.Text.RegularExpressions.Regex
            .Replace(s, "([A-Z])", "-$1").TrimStart('-').ToLower();
}
