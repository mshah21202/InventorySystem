using Application.Settings;
using Domain;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Application.Commands;

public class AddItemCommand(IStockRepository stockRepository) : Command<AddItemSettings>
{
    public override int Execute(CommandContext context, AddItemSettings settings)
    {
        // If the group is specified, check if it exists, if it doesn't, create it.
        if (settings.Group is not null)
        {
            var groups = stockRepository.GetGroups();
            var group = groups.FirstOrDefault(g => g.Name == settings.Group);
            if (group is null)
            {
                group = new Group
                {
                    Id = Guid.NewGuid(),
                    Name = settings.Group
                };
                stockRepository.AddGroupAsync(group).Wait();
                AnsiConsole.MarkupLine($"[green]Group does not exist, created group '{settings.Group}'[/]");
            }
        }
        
        var item = new Item
        {
            Id = Guid.NewGuid(),
            Name = settings.Name,
            Quantity = settings.Quantity,
            GroupId = settings.Group is not null ? stockRepository.GetGroups().First(g => g.Name == settings.Group).Id : null
        };
        stockRepository.AddItemAsync(item).Wait();
        
        AnsiConsole.MarkupLine($"[green]Item '{settings.Name}' added successfully[/]");
        
        return 0;
    }
}