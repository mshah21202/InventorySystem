using Spectre.Console.Cli;

namespace Application.Settings;

public class AddItemSettings : CommandSettings
{
    [CommandArgument(0, "[NAME]")]
    public required string Name { get; set; }

    [CommandArgument(1, "[QUANTITY]")]
    public required int Quantity { get; set; }

    [CommandOption("-g|--group <GROUP>")]
    public string? Group { get; set; }
}