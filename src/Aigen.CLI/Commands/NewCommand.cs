using System.Text.Json;
using Aigen.CLI.UI;
using Aigen.Core.Config;
using Aigen.Core.Metadata;
using Aigen.Core.Schema;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Aigen.CLI.Commands;

public class NewCommand : AsyncCommand<NewCommandSettings>
{
    private readonly ISchemaReader _schemaReader;

    public NewCommand(ISchemaReader schemaReader)
    {
        _schemaReader = schemaReader;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, NewCommandSettings settings)
    {
        Banner.Show();
        AnsiConsole.MarkupLine("[bold dodgerblue1][[aigen new]][/] Iniciando generacion...");
        AnsiConsole.WriteLine();

        // ── 1. Cargar configuracion ──────────────────────────────
        GeneratorConfig config;
        try
        {
            var configPath = settings.ConfigFile ?? "aigen.json";
            if (!File.Exists(configPath))
            {
                Banner.ShowError($"Archivo no encontrado: [bold]{configPath}[/]");
                return 1;
            }
            var json = await File.ReadAllTextAsync(configPath);
            config = JsonSerializer.Deserialize<GeneratorConfig>(json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
                })
                ?? throw new InvalidOperationException("JSON vacio o malformado.");
            Banner.ShowStep("Config", $"cargada desde [bold]{configPath}[/]");
        }
        catch (Exception ex)
        {
            Banner.ShowError($"Error al leer config: {ex.Message}");
            return 1;
        }

        ShowConfigSummary(config);
        AnsiConsole.WriteLine();

        // ── 2. Probar conexion ───────────────────────────────────
        Banner.ShowInfo($"Conectando a [bold]{config.Database.Engine}[/]...");
        bool connected = false;
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Probando conexion...", async ctx =>
            {
                connected = await _schemaReader.TestConnectionAsync(config.Database.ConnectionString);
            });

        if (!connected)
        {
            Banner.ShowError("No se pudo conectar a la base de datos.");
            Banner.ShowInfo("Verifica el ConnectionString en tu archivo de configuracion.");
            return 1;
        }
        Banner.ShowStep("Conexion", "exitosa");
        AnsiConsole.WriteLine();

        // ── 3. Leer schema ───────────────────────────────────────
        DatabaseMetadata metadata = null!;
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.BouncingBar)
            .StartAsync("Leyendo schema de la base de datos...", async ctx =>
            {
                metadata = await _schemaReader.ReadAsync(
                    config.Database.ConnectionString,
                    config.Database.Schema);
            });

        Banner.ShowStep("Schema",
            $"[bold]{metadata.DatabaseName}[/] en [bold]{metadata.ServerName}[/]");
        Banner.ShowInfo($"Tablas encontradas: [bold dodgerblue1]{metadata.TotalTables}[/] | " +
            $"Columnas: [bold]{metadata.TotalColumns}[/] | " +
            $"Relaciones FK: [bold]{metadata.TotalRelations}[/]");
        AnsiConsole.WriteLine();

        // ── 4. Filtrar tablas excluidas ──────────────────────────
        var allTables = metadata.Tables
            .Where(t => !config.Database.ExcludedTables
                .Contains(t.TableName, StringComparer.OrdinalIgnoreCase))
            .ToList();

        if (!allTables.Any())
        {
            Banner.ShowWarning("No se encontraron tablas disponibles para generar.");
            return 1;
        }

        // ── 5. Selector de tablas ────────────────────────────────
        var selectedNames = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title("[bold]Selecciona las tablas a incluir en la generacion[/]")
                .PageSize(20)
                .InstructionsText("[grey](ESPACIO para seleccionar, ENTER para confirmar)[/]")
                .AddChoices(allTables.Select(t => t.TableName)));

        if (!selectedNames.Any())
        {
            Banner.ShowWarning("Ninguna tabla seleccionada. Operacion cancelada.");
            return 0;
        }

        // Filtrar metadata con solo las tablas seleccionadas
        metadata.Tables = metadata.Tables
            .Where(t => selectedNames.Contains(t.TableName))
            .ToList();

        AnsiConsole.WriteLine();
        ShowMetadataSummary(metadata);
        ShowMetadataDetail(metadata);
        AnsiConsole.WriteLine();

        // ── 6. Confirmacion ──────────────────────────────────────
        var confirm = AnsiConsole.Confirm(
            $"[bold]Generar proyecto [dodgerblue1]{config.Project.ProjectName}[/] " +
            $"con [dodgerblue1]{metadata.TotalTables}[/] tabla(s)?[/]");

        if (!confirm)
        {
            Banner.ShowInfo("Operacion cancelada por el usuario.");
            return 0;
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold yellow] Generacion de codigo disponible en Semana 3.[/]");
        AnsiConsole.MarkupLine("[grey] La metadata esta lista y el generador de plantillas Scriban se implementara a continuacion.[/]");
        return 0;
    }

    // ── Tabla resumen de configuracion ──────────────────────────
    private static void ShowConfigSummary(GeneratorConfig c)
    {
        var t = Banner.CreateInfoTable("Configuracion del Proyecto");
        t.AddColumn("[grey]Parametro[/]").AddColumn("[bold]Valor[/]");
        t.AddRow("Proyecto",     $"[bold]{c.Project.ProjectName}[/]");
        t.AddRow("Namespace",    c.Project.RootNamespace);
        t.AddRow("Motor BD",     $"[bold dodgerblue1]{c.Database.Engine}[/]");
        t.AddRow("Schema",       c.Database.Schema);
        t.AddRow("Arquitectura", c.Architecture.Pattern.ToString());
        t.AddRow("Frontend",     c.Frontend.Framework.ToString());
        t.AddRow("Auth",         c.Security.Authentication.ToString());
        t.AddRow("Salida",       c.ResolveOutputPath());
        AnsiConsole.Write(t);
    }

    // ── Tabla resumen de metadata ────────────────────────────────
    private static void ShowMetadataSummary(DatabaseMetadata m)
    {
        var t = Banner.CreateInfoTable("Resumen del Schema");
        t.AddColumn("[grey]Metrica[/]").AddColumn("[bold]Valor[/]");
        t.AddRow("Base de datos", $"[bold]{m.DatabaseName}[/]");
        t.AddRow("Servidor",      m.ServerName);
        t.AddRow("Tablas",        $"[bold dodgerblue1]{m.TotalTables}[/]");
        t.AddRow("Columnas",      $"[bold]{m.TotalColumns}[/]");
        t.AddRow("Relaciones FK", $"[bold]{m.TotalRelations}[/]");
        AnsiConsole.Write(t);
    }

    // ── Tabla detalle por tabla ──────────────────────────────────
    private static void ShowMetadataDetail(DatabaseMetadata m)
    {
        var t = new Table()
            .Border(TableBorder.Simple)
            .AddColumn("[bold]Tabla[/]")
            .AddColumn("[bold]Clase C#[/]")
            .AddColumn("[bold]Cols[/]")
            .AddColumn("[bold]PKs[/]")
            .AddColumn("[bold]FKs[/]");

        foreach (var x in m.Tables)
            t.AddRow(
                $"[dodgerblue1]{x.TableName}[/]",
                $"[bold]{x.ClassName}[/]",
                x.Columns.Count.ToString(),
                string.Join(", ", x.PrimaryKeys),
                x.ForeignKeys.Count.ToString());

        AnsiConsole.Write(t);
    }
}

public class NewCommandSettings : CommandSettings
{
    [CommandOption("-c|--config")]
    public string? ConfigFile { get; set; }
}
