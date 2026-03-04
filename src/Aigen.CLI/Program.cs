#nullable enable
using Aigen.CLI.Commands;
using Aigen.Core.Schema;
using Aigen.Core.Services;
using Aigen.Templates;
using Aigen.Templates.Engine;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

// ── Contenedor DI ─────────────────────────────────────────
var services = new ServiceCollection();

// Schema readers
services.AddTransient<SqlServerSchemaReader>();
services.AddTransient<ISchemaReader, SqlServerSchemaReader>();

// Naming + Filter
services.AddTransient<NamingConventionService>();
services.AddTransient<SchemaFilterService>();

// Template engine
services.AddTransient<ITemplateEngine, ScribanTemplateEngine>();
services.AddTransient<TemplateLocator>();
services.AddTransient<FileGeneratorService>();

// ── Spectre.Console.Cli con DI ────────────────────────────
var registrar = new TypeRegistrar(services);
var app       = new CommandApp(registrar);

app.Configure(config =>
{
    config.SetApplicationName("aigen");
    config.SetApplicationVersion("1.0.0");

    // aigen new --config proyecto.json
    config.AddCommand<NewCommand>("new")
        .WithDescription("Lee el schema de BD y muestra metadata lista para generar.")
        .WithExample("new", "--config", "proyecto.json");

    // aigen generate --config proyecto.json  ← CONECTADO
    config.AddCommand<GenerateCommand>("generate")
        .WithDescription("Genera el proyecto completo desde BD usando plantillas Scriban.")
        .WithExample("generate", "--config", "proyecto.json")
        .WithExample("generate", "--config", "proyecto.json", "--dry-run");

    // Placeholders para fases siguientes
    config.AddCommand<PlaceholderCommand>("assist")
        .WithDescription("[Fase 3] Asistente inteligente con IA.");
});

return await app.RunAsync(args);

// ── TypeRegistrar: puente Spectre <-> Microsoft DI ────────
public sealed class TypeRegistrar : ITypeRegistrar
{
    private readonly IServiceCollection _builder;
    public TypeRegistrar(IServiceCollection builder) => _builder = builder;

    public ITypeResolver Build() =>
        new TypeResolver(_builder.BuildServiceProvider());

    public void Register(Type service, Type implementation) =>
        _builder.AddTransient(service, implementation);

    public void RegisterInstance(Type service, object implementation) =>
        _builder.AddSingleton(service, implementation);

    public void RegisterLazy(Type service, Func<object> factory) =>
        _builder.AddTransient(service, _ => factory());
}

public sealed class TypeResolver : ITypeResolver, IDisposable
{
    private readonly IServiceProvider _provider;
    public TypeResolver(IServiceProvider provider) => _provider = provider;
    public object? Resolve(Type? type) => type is null ? null : _provider.GetService(type);
    public void Dispose() { if (_provider is IDisposable d) d.Dispose(); }
}

/// <summary>Comando placeholder para features futuras.</summary>
public class PlaceholderCommand : Command<PlaceholderSettings>
{
    public override int Execute(CommandContext context, PlaceholderSettings settings)
    {
        Spectre.Console.AnsiConsole.MarkupLine(
            "[yellow] ⚠ Este comando estará disponible en una fase próxima de AIGEN.[/]");
        return 0;
    }
}
public class PlaceholderSettings : CommandSettings { }
