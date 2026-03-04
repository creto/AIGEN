namespace Aigen.Templates;

/// <summary>
/// Resuelve la ruta fisica de cada plantilla .scriban.
/// Orden de busqueda: carpeta personalizada -> subdirs Backend/Frontend/SQL/Solution.
/// </summary>
public class TemplateLocator
{
    private readonly string _root;

    public TemplateLocator(string? root = null)
        => _root = root ?? Path.Combine(AppContext.BaseDirectory, "Templates");

    public string Resolve(string name)
    {
        var direct = Path.Combine(_root, name);
        if (File.Exists(direct)) return direct;

        foreach (var sub in new[] { "Backend", "Frontend", "SQL", "Solution" })
        {
            var sub_path = Path.Combine(_root, sub, name);
            if (File.Exists(sub_path)) return sub_path;
        }
        return direct; // Llamador maneja la ausencia
    }

    public IEnumerable<string> ListAll() =>
        Directory.Exists(_root)
            ? Directory.GetFiles(_root, "*.scriban", SearchOption.AllDirectories)
            : Enumerable.Empty<string>();
}
