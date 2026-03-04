#nullable enable
using System.ComponentModel;
using Aigen.CLI.UI;
using Aigen.Core.Config;
using Aigen.Core.Metadata;
using Aigen.Core.Schema;
using Aigen.Core.Services;
using Aigen.Core.Validation;
using Aigen.Templates;
using Aigen.Templates.Engine;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Aigen.CLI.Commands;

/// <summary>
/// Comando: aigen generate --config mi-proyecto.json
/// </summary>
public class GenerateCommand : AsyncCommand<GenerateCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-c|--config")]
        [Description("Ruta al archivo JSON de configuracion")]
        public string ConfigPath { get; init; } = "aigen.json";

        [CommandOption("-t|--templates")]
        [Description("Carpeta de plantillas personalizada")]
        public string? TemplatesPath { get; init; }

        [CommandOption("--dry-run")]
        [Description("Muestra que se generaria sin escribir archivos")]
        public bool DryRun { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext ctx, Settings settings)
    {
        Aigen.CLI.UI.Banner.Show();

        // ── 1. Cargar config ──────────────────────────────────
        Aigen.CLI.UI.Banner.ShowStep("1/7", "Cargando configuracion");
        if (!File.Exists(settings.ConfigPath))
        {
            Aigen.CLI.UI.Banner.ShowError($"Archivo no encontrado: {settings.ConfigPath}");
            return 1;
        }

        GeneratorConfig config;
        try
        {
            var json = await File.ReadAllTextAsync(settings.ConfigPath);
            config = System.Text.Json.JsonSerializer.Deserialize<GeneratorConfig>(json,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters =
                    {
                        new System.Text.Json.Serialization.JsonStringEnumConverter()
                    }
                })!;
        }
        catch (Exception ex)
        {
            Aigen.CLI.UI.Banner.ShowError($"Error al parsear JSON: {ex.Message}");
            return 1;
        }

        // ── 2. Validar config ─────────────────────────────────
        Aigen.CLI.UI.Banner.ShowStep("2/7", "Validando configuracion");
        var validator  = new ConfigValidator();
        var validation = validator.Validate(config);

        foreach (var w in validation.Warnings)
            Aigen.CLI.UI.Banner.ShowWarning(w);

        if (!validation.IsValid)
        {
            foreach (var e in validation.Errors)
                Aigen.CLI.UI.Banner.ShowError($"[{e.Field}] {e.Message}");
            return 1;
        }
        Aigen.CLI.UI.Banner.ShowSuccess("Configuracion valida");

        // ── 3. Conectar BD ────────────────────────────────────
        Aigen.CLI.UI.Banner.ShowStep("3/7", "Conectando a la base de datos");
        var reader    = SchemaReaderFactory.Create(config.Database.Engine);
        var connected = false;

        await AnsiConsole.Status()
            .StartAsync("Probando conexion...", async _ =>
            {
                connected = await reader.TestConnectionAsync(
                    config.Database.ConnectionString);
            });

        if (!connected)
        {
            Aigen.CLI.UI.Banner.ShowError("No se pudo conectar a la base de datos.");
            return 1;
        }
        Aigen.CLI.UI.Banner.ShowSuccess("Conexion exitosa");

        // ── 4. Leer schema ────────────────────────────────────
        Aigen.CLI.UI.Banner.ShowStep("4/7", "Leyendo schema de la base de datos");
        Aigen.Core.Metadata.DatabaseMetadata db = null!;

        await AnsiConsole.Status()
            .StartAsync("Leyendo tablas, columnas, FK...", async _ =>
            {
                db = await reader.ReadAsync(
                    config.Database.ConnectionString,
                    config.Database.Schema);
            });

        Aigen.CLI.UI.Banner.ShowInfo(
            $"BD: [bold]{db.DatabaseName}[/] | " +
            $"Tablas: [bold]{db.TotalTables}[/] | " +
            $"Columnas: [bold]{db.TotalColumns}[/]");

        // ── 5. Seleccionar tablas ─────────────────────────────
        Aigen.CLI.UI.Banner.ShowStep("5/7", "Seleccionando tablas a generar");
        var naming   = new NamingConventionService();
        var filter   = new SchemaFilterService(naming);
        var filtered = filter.Filter(db.Tables, config.Database);

        var selected = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title("[bold]Selecciona las tablas a generar:[/]")
                .PageSize(20)
                .MoreChoicesText("[grey](Arriba/Abajo para navegar)[/]")
                .InstructionsText(
                    "[grey](ESPACIO para seleccionar, ENTER para confirmar)[/]")
                .AddChoices(filtered.Select(t => t.TableName)));

        var tablesToGenerate = filter.FilterBySelection(filtered, selected);

        // ── 6. Confirmar ──────────────────────────────────────
        Aigen.CLI.UI.Banner.ShowStep("6/7", "Confirmacion de generacion");
        var outPath = config.ResolveOutputPath();

        var summaryTable = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Tabla")
            .AddColumn("Clase")
            .AddColumn("Tipo")
            .AddColumn("CRUD")
            .AddColumn("Frontend");

        foreach (var t in tablesToGenerate)
            summaryTable.AddRow(
                t.TableName,
                t.ClassName,
                Aigen.Core.Metadata.TableMetadataExtensions.GetTableType(t).ToString(),
                Aigen.Core.Metadata.TableMetadataExtensions.HasFullCrud(t)
                    ? "[green]Completo[/]" : "[grey]Solo lectura[/]",
                config.Frontend.GenerateFrontend
                    ? (Aigen.Core.Metadata.TableMetadataExtensions.IsReadOnly(t)
                        ? "[grey]No[/]" : "[blue]Si[/]")
                    : "[grey]Desactivado[/]");

        AnsiConsole.Write(summaryTable);
        Aigen.CLI.UI.Banner.ShowInfo($"Destino: {outPath}");

        if (settings.DryRun)
        {
            AnsiConsole.MarkupLine("[yellow]--dry-run: no se escriben archivos.[/]");
            return 0;
        }

        if (!AnsiConsole.Confirm(
            $"Generar [bold]{config.Project.ProjectName}[/] " +
            $"con {tablesToGenerate.Count} tabla(s)?"))
            return 0;

        // ── 7. Generar ────────────────────────────────────────
        Aigen.CLI.UI.Banner.ShowStep("7/7", "Generando codigo");

        var engine    = new ScribanTemplateEngine();
        var locator   = new TemplateLocator(settings.TemplatesPath);
        var generator = new FileGeneratorService(engine, locator);

        GenerationResult result = null!;

        await AnsiConsole.Progress()
            .StartAsync(async ctx2 =>
            {
                var task = ctx2.AddTask(
                    "Generando archivos",
                    maxValue: tablesToGenerate.Count);

                var progress = new Progress<GenerationProgress>(p =>
                {
                    task.Description = $"[bold]{p.TableName}[/] ({p.Current}/{p.Total})";
                    task.Value       = p.Current;
                });

                result = await generator.GenerateAsync(
                    config, db, tablesToGenerate, progress);
            });

        // ── Resumen ───────────────────────────────────────────
        AnsiConsole.WriteLine();

        if (result.IsSuccess)
            Aigen.CLI.UI.Banner.ShowSuccess(
                $"Completado: [bold]{result.TotalFiles}[/] archivos en {outPath}");
        else
            Aigen.CLI.UI.Banner.ShowWarning(
                $"Completado con [bold]{result.Errors.Count}[/] error(es):");

        foreach (var e in result.Errors)
            Aigen.CLI.UI.Banner.ShowError(e);
        foreach (var w in result.Warnings)
            Aigen.CLI.UI.Banner.ShowWarning(w);

        return result.IsSuccess ? 0 : 1;
    }
}
