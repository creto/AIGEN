namespace Aigen.Templates;

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
            var subPath = Path.Combine(_root, sub, name);
            if (File.Exists(subPath)) return subPath;
        }
        return direct;
    }

    public IEnumerable<string> ListAll() =>
        Directory.Exists(_root)
            ? Directory.GetFiles(_root, "*.scriban", SearchOption.AllDirectories)
            : Enumerable.Empty<string>();
}
