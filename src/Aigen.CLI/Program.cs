using Aigen.CLI.Commands;
using Aigen.Core.Schema;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

var services = new ServiceCollection();
services.AddTransient<ISchemaReader, SqlServerSchemaReader>();

var registrar = new TypeRegistrar(services);
var app = new CommandApp(registrar);

app.Configure(config =>
{
    config.SetApplicationName("aigen");
    config.SetApplicationVersion("1.0.0");
    config.AddCommand<NewCommand>("new")
        .WithDescription("Genera un nuevo proyecto desde una BD o configuracion JSON.")
        .WithExample("new", "--config", "proyecto.json");
    config.AddCommand<PlaceholderCommand>("generate").WithDescription("[Semana 3] Genera capas especificas.");
    config.AddCommand<PlaceholderCommand>("preview").WithDescription("[Semana 3] Previsualiza sin escribir archivos.");
    config.AddCommand<PlaceholderCommand>("assist").WithDescription("[Fase 3] Asistente con IA.");
});

return await app.RunAsync(args);

public sealed class TypeRegistrar : ITypeRegistrar
{
    private readonly IServiceCollection _builder;
    public TypeRegistrar(IServiceCollection builder) => _builder = builder;
    public ITypeResolver Build() => new TypeResolver(_builder.BuildServiceProvider());
    public void Register(Type service, Type implementation) => _builder.AddTransient(service, implementation);
    public void RegisterInstance(Type service, object implementation) => _builder.AddSingleton(service, implementation);
    public void RegisterLazy(Type service, Func<object> factory) => _builder.AddTransient(service, _ => factory());
}
public sealed class TypeResolver : ITypeResolver, IDisposable
{
    private readonly IServiceProvider _provider;
    public TypeResolver(IServiceProvider provider) => _provider = provider;
    public object? Resolve(Type? type) => type is null ? null : _provider.GetService(type);
    public void Dispose() { if (_provider is IDisposable d) d.Dispose(); }
}
public class PlaceholderCommand : Command<PlaceholderSettings>
{
    public override int Execute(CommandContext context, PlaceholderSettings settings)
    {
        Spectre.Console.AnsiConsole.MarkupLine("[yellow]Este comando estara disponible en una fase proxima.[/]");
        return 0;
    }
}
public class PlaceholderSettings : CommandSettings { }
