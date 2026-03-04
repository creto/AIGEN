using Spectre.Console;

namespace Aigen.CLI.UI;

public static class Banner
{
    public static void Show()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText("AIGEN").Centered().Color(Color.DodgerBlue1));
        AnsiConsole.Write(new Rule("[bold dodgerblue1]AI-Powered Software Generator  v1.0.0[/]").Centered());
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]From schema to production. Intelligently.[/]");
        AnsiConsole.WriteLine();
    }

    public static void ShowStep(string step, string message) =>
        AnsiConsole.MarkupLine($"[bold green] v [/][bold]{step}[/] {message}");

    public static void ShowInfo(string message) =>
        AnsiConsole.MarkupLine($"[bold dodgerblue1] > [/]{message}");

    public static void ShowWarning(string message) =>
        AnsiConsole.MarkupLine($"[bold yellow] ! [/]{message}");

    public static void ShowError(string message) =>
        AnsiConsole.MarkupLine($"[bold red] x [/]{message}");

    public static void ShowSuccess(string message) =>
        AnsiConsole.MarkupLine($"[bold green] v [/][green]{message}[/]");

    public static Table CreateInfoTable(string title) =>
        new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.DodgerBlue1)
            .Title($"[bold dodgerblue1]{title}[/]");
}
