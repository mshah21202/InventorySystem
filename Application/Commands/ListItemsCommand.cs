using Spectre.Console;
using Spectre.Console.Cli;

namespace Application.Commands;

public class ListItemsCommand(IStockRepository stockRepository) : Command<ListItemsSettings>
{
    public override int Execute(CommandContext context, ListItemsSettings settings)
    {
        var items = stockRepository.GetItemsInfo();
        if (items.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No items found[/]");
            return 0;
        }

        var table = new Table();
        table.AddColumn("Name");
        table.AddColumn("Quantity");
        table.AddColumn("Group");

        foreach (var item in items)
        {
            table.AddRow(item.Name, item.Quantity.ToString(), item.Group);
        }

        AnsiConsole.Write(table);

        return 0;
    }
}