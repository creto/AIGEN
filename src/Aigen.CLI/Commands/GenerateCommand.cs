#nullable enable
using System.ComponentModel;
using Aigen.CLI.UI;
using Aigen.Core.Config;
using Aigen.Core.Config.Enums;
using Aigen.Core.Metadata;
using Aigen.Core.Schema;
using Aigen.Core.Services;
using Aigen.Core.Validation;
using Aigen.Templates;
using Aigen.Templates.Engine;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Aigen.CLI.Commands;

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

        [CommandOption("--verbose")]
        [Description("Muestra errores detallados de conexion")]
        public bool Verbose { get; init; }

        [CommandOption("--no-interactive|-y")]
        [Description("Usa toda la configuracion del JSON sin preguntar nada")]
        public bool NoInteractive { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext ctx, Settings settings)
    {
        Aigen.CLI.UI.Banner.Show();

        // â”€â”€ 1. Cargar config â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        Aigen.CLI.UI.Banner.ShowStep("1/8", "Cargando configuracion");
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

        // â”€â”€ 2. Validar config â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        Aigen.CLI.UI.Banner.ShowStep("2/8", "Validando configuracion");
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

        //
        
        if (!settings.NoInteractive)
        {
        // â”€â”€ 3. ConfiguraciÃ³n interactiva (ORM, Frontend, Features) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        Aigen.CLI.UI.Banner.ShowStep("3/8", "Configuracion de generacion");

        // â”€â”€ ORM â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        var ormChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold]Â¿QuÃ© ORM deseas usar?[/]")
                .AddChoices(
                    "[green]EF Core + Dapper (HÃ­brido recomendado)[/]",
                    "[blue]Entity Framework Core (solo EF)[/]",
                    "[yellow]Dapper (solo Dapper)[/]"));

        config.Backend.Orm = ormChoice switch
        {
            var s when s.StartsWith("[green]")  => OrmType.EFCoreWithDapper,
            var s when s.StartsWith("[blue]")   => OrmType.EntityFrameworkCore,
            _                                    => OrmType.Dapper
        };

        AnsiConsole.MarkupLine($"[grey]  ORM seleccionado: [bold]{config.Backend.Orm}[/][/]");

        // â”€â”€ Target Framework â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        var tfChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold]Â¿Target framework .NET?[/]")
                .AddChoices(
                    "[green]net8.0 (LTS recomendado)[/]",
                    "[blue]net9.0[/]",
                    "[grey]net7.0[/]"));

        config.Backend.TargetFramework = tfChoice switch
        {
            var s when s.StartsWith("[green]") => "net8.0",
            var s when s.StartsWith("[blue]")  => "net9.0",
            _                                   => "net7.0"
        };

        AnsiConsole.MarkupLine($"[grey]  Framework: [bold]{config.Backend.TargetFramework}[/][/]");

        // â”€â”€ Frontend â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        var generateFrontend = AnsiConsole.Confirm(
            "Â¿Generar frontend Angular?", config.Frontend.GenerateFrontend);
        config.Frontend.GenerateFrontend = generateFrontend;

        if (generateFrontend)
        {
            var angularVersion = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Â¿VersiÃ³n de Angular?[/]")
                    .AddChoices(
                        "[green]18 (recomendado)[/]",
                        "[blue]17[/]",
                        "[grey]16[/]"));

            config.Frontend.FrameworkVersion = angularVersion switch
            {
                var s when s.StartsWith("[green]") => "18",
                var s when s.StartsWith("[blue]")  => "17",
                _                                   => "16"
            };
            AnsiConsole.MarkupLine($"[grey]  Angular: [bold]v{config.Frontend.FrameworkVersion}[/][/]");
        }

        // â”€â”€ Features â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        var featureChoices = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title("[bold]Â¿QuÃ© features habilitar?[/]")
                .PageSize(10)
                .InstructionsText("[grey](ESPACIO seleccionar Â· ENTER confirmar)[/]")
                .AddChoices(
                    "PaginaciÃ³n en GET",
                    "Soft Delete (campo Estado)",
                    "AuditorÃ­a (AudFmod, AudMachine...)",
                    "FluentValidation",
                    "Swagger / OpenAPI",
                    "Generar Dockerfile",
                    "Generar Docker Compose",
                    "Generar Tests unitarios")
                .Select("PaginaciÃ³n en GET")
                .Select("Soft Delete (campo Estado)")
                .Select("AuditorÃ­a (AudFmod, AudMachine...)")
                .Select("FluentValidation")
                .Select("Swagger / OpenAPI"));

        config.Features.GeneratePagination    = featureChoices.Contains("PaginaciÃ³n en GET");
        config.Features.SoftDelete            = featureChoices.Contains("Soft Delete (campo Estado)");
        config.Features.Auditing              = featureChoices.Contains("AuditorÃ­a (AudFmod, AudMachine...)");
        config.Features.Validation            = featureChoices.Contains("FluentValidation")
                                                    ? ValidationProvider.FluentValidation
                                                    : ValidationProvider.DataAnnotations;
        config.Features.ApiDoc                = featureChoices.Contains("Swagger / OpenAPI")
                                                    ? ApiDocProvider.Swagger
                                                    : ApiDocProvider.None;
        config.Features.GenerateDockerfile    = featureChoices.Contains("Generar Dockerfile");
        config.Features.GenerateDockerCompose = featureChoices.Contains("Generar Docker Compose");
        config.Features.GenerateTests         = featureChoices.Contains("Generar Tests unitarios");

        }
        else
        {
            Aigen.CLI.UI.Banner.ShowStep("3/8", "Configuracion de generacion");
            Aigen.CLI.UI.Banner.ShowInfo($"ORM: {config.Backend.Orm} | Strategy: {config.Backend.CrudStrategy}");
            Aigen.CLI.UI.Banner.ShowSuccess("Usando configuracion del JSON (--no-interactive)");
        }
        Aigen.CLI.UI.Banner.ShowSuccess("Configuracion de generacion lista");




        // â”€â”€ 3. Conectar BD â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        Aigen.CLI.UI.Banner.ShowStep("4/8", "Conectando a la base de datos");
        var reader    = SchemaReaderFactory.Create(config.Database.Engine);
        var connected = false;
        var connError = string.Empty;

        await AnsiConsole.Status()
            .StartAsync("Probando conexion...", async _ =>
            {
                try
                {
                    connected = await reader.TestConnectionAsync(
                        config.Database.ConnectionString);
                }
                catch (Exception ex)
                {
                    connected = false;
                    connError = ex.Message;
                }
            });

        if (!connected)
        {
            Aigen.CLI.UI.Banner.ShowError("No se pudo conectar a la base de datos.");
            if (!string.IsNullOrEmpty(connError))
                AnsiConsole.MarkupLine($"[red]   Error: {Markup.Escape(connError)}[/]");

            AnsiConsole.MarkupLine("\n[grey]Verifica en aigen.json:[/]");
            AnsiConsole.MarkupLine($"[grey]  Server:   {Markup.Escape(config.Database.ConnectionString.Split(';').FirstOrDefault() ?? "")}[/]");
            AnsiConsole.MarkupLine($"[grey]  Engine:   {config.Database.Engine}[/]");
            AnsiConsole.MarkupLine($"[grey]  Schema:   {config.Database.Schema}[/]");
            return 1;
        }
        Aigen.CLI.UI.Banner.ShowSuccess("Conexion exitosa");

        // â”€â”€ 4. Leer schema â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        Aigen.CLI.UI.Banner.ShowStep("5/8", "Leyendo schema de la base de datos");
        Aigen.Core.Metadata.DatabaseMetadata db = null!;
        var readError = string.Empty;

        await AnsiConsole.Status()
            .StartAsync("Leyendo tablas, columnas, FK...", async _ =>
            {
                try
                {
                    var schemas = config.Database.Schemas.Any()
                        ? config.Database.Schemas
                        : new List<string> { config.Database.Schema };

                    db = await reader.ReadMultiSchemaAsync(
                        config.Database.ConnectionString,
                        schemas);
                }
                catch (Exception ex)
                {
                    readError = ex.Message;
                }
            });

        if (!string.IsNullOrEmpty(readError))
        {
            Aigen.CLI.UI.Banner.ShowError($"Error al leer schema: {readError}");
            return 1;
        }

        Aigen.CLI.UI.Banner.ShowInfo(
            $"BD: [bold]{db.DatabaseName}[/] | " +
            $"Tablas: [bold]{db.TotalTables}[/] | " +
            $"Columnas: [bold]{db.TotalColumns}[/]");

        // â”€â”€ 5. Seleccionar tablas â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        Aigen.CLI.UI.Banner.ShowStep("6/8", "Seleccionando tablas a generar");
        var naming   = new NamingConventionService();
        var filter   = new SchemaFilterService(naming);
        var filtered = filter.Filter(db.Tables, config.Database);

        if (!filtered.Any())
        {
            Aigen.CLI.UI.Banner.ShowWarning("No se encontraron tablas con los prefijos configurados.");
            
            return 1;
        }
        List<TableMetadata> tablesToGenerate;
        if (!settings.NoInteractive)
        {
            // ── Modo de selección ──────────────────────────────────────
            AnsiConsole.MarkupLine(
                $"[grey]  {filtered.Count} tablas disponibles[/]");

            var selectionMode = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]¿Cómo quieres seleccionar las tablas?[/]")
                    .AddChoices(
                        $"[green]Todas ({filtered.Count} tablas)[/]",
                        "[yellow]Solo CRUD completo (excluye TA_, TS_, TH_)[/]",
                        "[blue]Por prefijo (TM_, TB_, TP_...)[/]",
                        "[grey]Manual (selección una por una)[/]"));

            if (selectionMode.StartsWith("[green]"))
            {
                tablesToGenerate = filtered;
                Aigen.CLI.UI.Banner.ShowSuccess(
                    $"Seleccionadas todas las tablas: [bold]{tablesToGenerate.Count}[/]");
            }
            else if (selectionMode.StartsWith("[yellow]"))
            {
                tablesToGenerate = filtered
                    .Where(t => Aigen.Core.Metadata.TableMetadataExtensions.HasFullCrud(t))
                    .ToList();
                Aigen.CLI.UI.Banner.ShowSuccess(
                    $"Seleccionadas tablas con CRUD completo: [bold]{tablesToGenerate.Count}[/]");
            }
            else if (selectionMode.StartsWith("[blue]"))
            {
                var prefixOptions = filtered
                    .Select(t => t.TableName.Contains('_')
                        ? t.TableName[..(t.TableName.IndexOf('_') + 1)]
                        : "(sin prefijo)")
                    .Distinct().OrderBy(p => p).ToList();

                var selectedPrefixes = AnsiConsole.Prompt(
                    new MultiSelectionPrompt<string>()
                        .Title("[bold]Selecciona los prefijos a generar:[/]")
                        .PageSize(15)
                        .InstructionsText("[grey](ESPACIO seleccionar · ENTER confirmar)[/]")
                        .AddChoices(prefixOptions));

                tablesToGenerate = filtered.Where(t =>
                {
                    var prefix = t.TableName.Contains('_')
                        ? t.TableName[..(t.TableName.IndexOf('_') + 1)]
                        : "(sin prefijo)";
                    return selectedPrefixes.Contains(prefix);
                }).ToList();

                Aigen.CLI.UI.Banner.ShowSuccess(
                    $"Seleccionadas [bold]{tablesToGenerate.Count}[/] tablas de prefijos: {string.Join(", ", selectedPrefixes)}");
            }
            else
            {
                var selected = AnsiConsole.Prompt(
                    new MultiSelectionPrompt<string>()
                        .Title("[bold]Selecciona las tablas a generar:[/]")
                        .PageSize(20)
                        .MoreChoicesText("[grey](↑↓ para navegar)[/]")
                        .InstructionsText("[grey](ESPACIO seleccionar · ENTER confirmar)[/]")
                        .AddChoices(filtered.Select(t => t.TableName)));

                tablesToGenerate = filtered
                    .Where(t => selected.Contains(t.TableName))
                    .ToList();
            }
        }
        else
        {
            // --no-interactive: usar todas las tablas filtradas por el config JSON
            // (tableSelection: All|Include|Exclude ya aplicado en "filtered")
            // Para modo SP se recomienda pasar --no-interactive con tableSelection=Include
            tablesToGenerate = filtered;
            Aigen.CLI.UI.Banner.ShowInfo($"Tablas seleccionadas: {tablesToGenerate.Count} (--no-interactive, todas)");
        }

        // â”€â”€ 6. Confirmar â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        Aigen.CLI.UI.Banner.ShowStep("7/8", "Confirmacion");
        var outPath = config.ResolveOutputPath();

        var summaryTable = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Tabla")
            .AddColumn("Clase")
            .AddColumn("Tipo")
            .AddColumn("CRUD")
            .AddColumn("Frontend");

        foreach (var t in tablesToGenerate.Take(20))
            summaryTable.AddRow(
                t.TableName,
                t.ClassName,
                Aigen.Core.Metadata.TableMetadataExtensions.GetTableTypeLabel(t),
                Aigen.Core.Metadata.TableMetadataExtensions.HasFullCrud(t)
                    ? "[green]Completo[/]" : "[grey]Solo lectura[/]",
                config.Frontend.GenerateFrontend
                    ? (Aigen.Core.Metadata.TableMetadataExtensions.IsReadOnly(t)
                        ? "[grey]No[/]" : "[blue]Si[/]")
                    : "[grey]Off[/]");

        if (tablesToGenerate.Count > 20)
            summaryTable.AddRow(
                $"[grey]... y {tablesToGenerate.Count - 20} mas[/]",
                "", "", "", "");

        AnsiConsole.Write(summaryTable);
        Aigen.CLI.UI.Banner.ShowInfo($"Total: [bold]{tablesToGenerate.Count}[/] tablas â†’ {outPath}");

        if (settings.DryRun)
        {
            AnsiConsole.MarkupLine("\n[yellow] --dry-run: no se escriben archivos.[/]");
            AnsiConsole.MarkupLine($"[grey]  Se generarian ~{tablesToGenerate.Count * 6} archivos de codigo.[/]");
            return 0;
        }

        if (!AnsiConsole.Confirm(
            $"\nGenerar [bold]{config.Project.ProjectName}[/] con [bold]{tablesToGenerate.Count}[/] tabla(s)?"))
            return 0;

        // â”€â”€ 7. Generar â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        Aigen.CLI.UI.Banner.ShowStep("8/8", "Generando codigo");

        var engine    = new ScribanTemplateEngine();
        var locator   = new TemplateLocator(settings.TemplatesPath);
        var generator = new FileGeneratorService(engine, locator);

        GenerationResult result = null!;

        await AnsiConsole.Progress()
            .AutoClear(false)
            .StartAsync(async ctx2 =>
            {
                var task = ctx2.AddTask(
                    "[green]Generando archivos[/]",
                    maxValue: tablesToGenerate.Count);

                var progress = new Progress<GenerationProgress>(p =>
                {
                    task.Description = $"[green]{p.TableName}[/] ({p.Current}/{p.Total})";
                    task.Value       = p.Current;
                });

                result = await generator.GenerateAsync(
                    config, db, tablesToGenerate, progress);
            });

        // â”€â”€ Resumen final â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        AnsiConsole.WriteLine();
        if (result.IsSuccess)
        {
            Aigen.CLI.UI.Banner.ShowSuccess(
                $"[bold]{result.TotalFiles}[/] archivos generados en:");
            AnsiConsole.MarkupLine($"[bold dodgerblue1]  {Markup.Escape(outPath)}[/]");
        }
        else
        {
            Aigen.CLI.UI.Banner.ShowWarning(
                $"Completado con [bold]{result.Errors.Count}[/] error(es) | " +
                $"[bold]{result.TotalFiles}[/] archivos OK");
        }

        if (result.Warnings.Any())
        {
            AnsiConsole.MarkupLine("\n[yellow]Advertencias:[/]");
            foreach (var w in result.Warnings.Take(10))
                Aigen.CLI.UI.Banner.ShowWarning(Markup.Escape(w));
        }

        if (result.Errors.Any())
        {
            AnsiConsole.MarkupLine("\n[red]Errores:[/]");
            foreach (var e in result.Errors.Take(10))
                Aigen.CLI.UI.Banner.ShowError(Markup.Escape(e));
        }

        return result.IsSuccess ? 0 : 1;
    }
}
