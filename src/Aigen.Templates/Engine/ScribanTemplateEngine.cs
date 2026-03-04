using Scriban;
using Scriban.Runtime;
using ScribanCtx = Scriban.TemplateContext;

namespace Aigen.Templates.Engine;

/// <summary>
/// Motor de plantillas basado en Scriban.
/// Las propiedades del TemplateContext se exponen en snake_case.
/// Las funciones utilitarias (pascal, camel, kebab...) se registran
/// importando la clase estatica ScribanFunctions como ScriptObject.
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
            throw new InvalidOperationException($"Error en plantilla Scriban:\n{msgs}");
        }

        // ── Contexto principal (propiedades en snake_case) ────
        var contextObj = new ScriptObject();
        contextObj.Import(ctx, renamer: m => ToSnake(m.Name));

        // ── Funciones utilitarias ────────────────────────────
        // Scriban requiere importar metodos estaticos como ScriptObject separado.
        // Import(Type) mapea cada metodo publico estatico con el renamer dado.
        var functionsObj = new ScriptObject();
        functionsObj.Import(typeof(ScribanFunctions),
            renamer: m => ToSnake(m.Name));

        // Fecha actual como variable simple
        functionsObj["now"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

        // ── Combinar en el contexto Scriban ──────────────────
        var scribanCtx = new ScribanCtx { StrictVariables = false };
        scribanCtx.PushGlobal(functionsObj);
        scribanCtx.PushGlobal(contextObj); // contextObj tiene prioridad

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
}

/// <summary>
/// Funciones utilitarias para plantillas Scriban.
/// Se importan via ScriptObject.Import(typeof(ScribanFunctions)).
/// Scriban las expone en snake_case: Pascal->pascal, NullableCs->nullable_cs
///
/// Uso en plantillas:
///   {{ pascal "mi_texto" }}   -> "MiTexto"
///   {{ camel  "mi_texto" }}   -> "miTexto"
///   {{ kebab  "MiTexto"  }}   -> "mi-texto"
///   {{ nullable_cs "int" true }} -> "int?"
/// </summary>
public static class ScribanFunctions
{
    public static string Pascal(string? s) =>
        string.Concat((s ?? "").Split('_', ' ', '-')
            .Select(w => w.Length > 0 ? char.ToUpper(w[0]) + w[1..] : w));

    public static string Camel(string? s)
    {
        var p = Pascal(s);
        return p.Length > 0 ? char.ToLower(p[0]) + p[1..] : p;
    }

    public static string Kebab(string? s) =>
        System.Text.RegularExpressions.Regex
            .Replace(s ?? "", "([A-Z])", "-$1")
            .TrimStart('-')
            .ToLower();

    public static string Upper(string? s) => (s ?? "").ToUpperInvariant();

    public static string Lower(string? s) => (s ?? "").ToLowerInvariant();

    public static string NullableCs(string? type, object? nullable)
    {
        // Scriban puede pasar bool como bool, int (0/1) o string ("true"/"false")
        var isNullable = nullable switch
        {
            bool b   => b,
            int  i   => i != 0,
            string s => s.Equals("true", StringComparison.OrdinalIgnoreCase),
            _        => false
        };
        return isNullable && type != "string" ? (type ?? "") + "?" : type ?? "";
    }
}
