using Spectre.Console.Cli;

namespace Application.Commands;

public class ListItemsSettings : CommandSettings
{
    [CommandOption("-g|--group <GROUP>")]
    public string? Group { get; set; }
}