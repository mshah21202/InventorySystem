using Application;
using Domain;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace InventorySystem;

public static class InteractiveMode
{
    /// <summary>
    /// The menu options for the interactive mode. The key is the command and the value is the display name.
    /// </summary>
    private static readonly Dictionary<string, string> Menu = new()
    {
        { "add-item", "Add Item" },
        { "remove-item", "Remove Item" },
        { "list-items", "List Items" },
        { "clear-all", "Clear all items and groups" },
        { "exit", "Exit" }
    };

    private static IServiceProvider? _serviceProvider;

    public static void StartInteractiveMode(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        while (true)
        {
            var command = ChooseCommand();
            
            if (command == "exit")
            {
                break;
            }

            ProcessCommand(command);
        }
    }

    private static void RenderHeader()
    {
        var panel = new Panel("Inventory System")
        {
            Border = BoxBorder.Rounded
        };

        AnsiConsole.Write(panel);
    }

    private static void ProcessCommand(string command, string[]? args = null)
    {
        switch (command)
        {
            case "add-item":
            {
                string? itemName = null;
                int? quantity = null;

                if (args is { Length: 2 })
                {
                    itemName = args.ElementAtOrDefault(0);
                    quantity = int.TryParse(args.ElementAtOrDefault(1), out var q) ? q : null;
                }

                AddItemPage(itemName, quantity);
                break;
            }
            case "remove-item":
                RemoveItemPage();
                break;
            case "list-items":
            {
                ListItemsPage();
                break;
            }
            case "clear-all":
                ClearAllPage();
                break;
            case "exit":
                return;
            default:
                throw new InvalidOperationException("Invalid command.");
        }
    }

    private static string ChooseCommand()
    {
        AnsiConsole.Clear();

        RenderHeader();

        // Print full path of the items and groups files
        if (_serviceProvider != null)
        {
            var stockRepository = _serviceProvider.GetRequiredService<IStockRepository>();
            AnsiConsole.MarkupLine($"[bold]Items file:[/] {stockRepository.ItemsFullPath}");
            AnsiConsole.MarkupLine($"[bold]Groups file:[/] {stockRepository.GroupsFullPath}");
        }

        var option = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("Select an option")
            .AddChoices(Menu.Select(opt => opt.Value).ToArray()));

        return Menu.First(pair => pair.Value == option).Key;
    }

    private static void ClearAllPage()
    {
        if (_serviceProvider == null) throw new InvalidOperationException("Service provider is not found.");
        AnsiConsole.Clear();

        RenderHeader();

        AnsiConsole.MarkupLine("[bold red]Clear All[/]");

        var confirm = AnsiConsole.Confirm("Are you sure you want to clear all items and groups?");

        if (confirm)
        {
            var stockRepository = _serviceProvider.GetRequiredService<IStockRepository>();
            stockRepository.ClearItemsAsync();
            stockRepository.ClearGroupsAsync();

            AnsiConsole.MarkupLine("[bold green]Successfully cleared all items and groups.[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[bold yellow]Operation cancelled.[/]");
        }

        // Press any key to continue (go back to the main menu)
        AnsiConsole.Prompt(new ContinuePrompt());
    }

    private static void AddItemPage(string? name = null, int? quantity = null)
    {
        if (_serviceProvider == null) throw new InvalidOperationException("Service provider is not found.");
        AnsiConsole.Clear();

        RenderHeader();

        AnsiConsole.MarkupLine("[bold]Add Item[/]");

        // Get the name of the item
        if (name == null)
        {
            name = AnsiConsole.Prompt(new CancellableTextPrompt("Enter the name of the item:"));
            if (name == null) return;
        }
        else
        {
            AnsiConsole.MarkupLine($"Enter the name of the item: {name}");
        }

        // Get the quantity of the item
        if (quantity == null)
        {
            var quantityText = AnsiConsole.Prompt(new CancellableTextPrompt("Enter the quantity of the item:"));
            if (quantityText == null) return;
            
            quantity = int.TryParse(quantityText, out var q) ? q : null;
        }
        else
        {
            AnsiConsole.MarkupLine($"Enter the quantity of the item: {quantity}");
        }

        // Get all groups
        var stockRepository = _serviceProvider.GetRequiredService<IStockRepository>();
        var groups = stockRepository.GetGroups();

        // Display all groups, and allow the user choose to create a new group if needed
        var groupChoices = groups.Select(group => group.Name).ToList();
        groupChoices.Add("Create a new group");

        var groupChoice = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("Select a group")
            .AddChoices(groupChoices.ToArray()));

        // If the user chooses to create a new group, go to an alternative page to create a new group
        // and then return to this page to choose the group again
        if (groupChoice == "Create a new group")
        {
            AddGroupPage("add-item", [name, quantity.Value.ToString()]);

            return;
        }

        // Get the group ID
        var groupId = groups.First(group => group.Name == groupChoice).Id;

        // If the item already exists, update the quantity
        var existingItems = stockRepository.GetItems();
        var item = existingItems.FirstOrDefault(item => item.Name == name && item.GroupId == groupId);

        int result;
        var alreadyExists = item != null;

        if (item == null)
        {
            // Create the item
            item = new Item
            {
                Id = Guid.NewGuid(),
                Name = name,
                Quantity = quantity.Value,
                GroupId = groupId
            };

            // Add the item
            result = stockRepository.AddItemAsync(item).Result;
        }
        else
        {
            // Update the quantity
            item.Quantity += quantity.Value;
            result = stockRepository.UpdateItemAsync(item).Result;
        }

        if (result == 0)
        {
            // Success
            AnsiConsole.MarkupLine($"[bold green]Successfully {(alreadyExists ? "updated" : "added")} the item.[/]");
            // Print the item
            AnsiConsole.MarkupLine($"[bold green]Item ID:[/] {item.Id}");
            AnsiConsole.MarkupLine($"[bold green]Name:[/] {item.Name}");
            AnsiConsole.MarkupLine($"[bold green]Quantity:[/] {item.Quantity}");
            AnsiConsole.MarkupLine($"[bold green]Group ID:[/] {item.GroupId}");
        }
        else
        {
            // Failure
            AnsiConsole.MarkupLine($"[bold red]Failed to {(alreadyExists ? "update" : "add")} the item.[/]");
        }

        // Press any key to continue (go back to the main menu)
        AnsiConsole.Prompt(new ContinuePrompt());
    }

    private static void RemoveItemPage()
    {
        if (_serviceProvider == null) throw new InvalidOperationException("Service provider is not found.");
        AnsiConsole.Clear();

        RenderHeader();

        AnsiConsole.MarkupLine("[bold]Remove Item[/]");

        // Get all items
        var stockRepository = _serviceProvider.GetRequiredService<IStockRepository>();
        var items = stockRepository.GetItemsInfo();
        
        if (items.Count == 0)
        {
            AnsiConsole.MarkupLine("[bold red]No items found.[/]");
            // Press any key to continue (go back to the main menu)
            AnsiConsole.Prompt(new ContinuePrompt());
            return;
        }
        
        // Display all items (ID, name, group, quantity)
        var itemChoices = items.Select(item => $"{item.Id} - x{item.Quantity} {item.Name} ({item.Group})").ToList();
        itemChoices.Add("Cancel...");
        
        var itemChoice = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("Select an item to remove")
            .AddChoices(itemChoices.ToArray()));
        
        if (itemChoice == "Cancel...") return;
        
        // Get the item ID
        var itemId = Guid.Parse(itemChoice.Split(" - ").First());
        
        // Get the item
        var item = items.First(item => item.Id == itemId);
        
        // Ask if the user wants to remove the item or remove a specific quantity
        var choices = new[] { "Remove the item", "Remove a specific quantity", "Cancel..." };
        var choice = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("Select an option")
            .AddChoices(choices));
        
        switch (choice)
        {
            case "Cancel...":
                return;
            case "Remove the item":
            {
                // Remove the item
                var result = stockRepository.RemoveItemAsync(itemId).Result;
            
                if (result == 0)
                {
                    // Success
                    AnsiConsole.MarkupLine("[bold green]Successfully removed the item.[/]");
                    // Print the item
                    AnsiConsole.MarkupLine($"[bold green]Item ID:[/] {item.Id}");
                    AnsiConsole.MarkupLine($"[bold green]Name:[/] {item.Name}");
                    AnsiConsole.MarkupLine($"[bold green]Quantity:[/] {item.Quantity}");
                }
                else
                {
                    // Failure
                    AnsiConsole.MarkupLine("[bold red]Failed to remove the item.[/]");
                }

                break;
            }
            case "Remove a specific quantity":
            {
                // Get the quantity to remove
                var quantityToRemove = AnsiConsole.Ask<int>("Enter the quantity to remove:");
            
                if (quantityToRemove > item.Quantity)
                {
                    AnsiConsole.MarkupLine("[bold red]The quantity to remove is greater than the current quantity.[/]");
                    // Press any key to continue (go back to the main menu)
                    AnsiConsole.Prompt(new ContinuePrompt());
                    return;
                }
            
                // Get the item
                var itemToUpdate = stockRepository.GetItem(itemId);
                if (itemToUpdate == null)
                {
                    AnsiConsole.MarkupLine("[bold red]The item does not exist.[/]");
                    // Press any key to continue (go back to the main menu)
                    AnsiConsole.Prompt(new ContinuePrompt());
                    return;
                }
                itemToUpdate.Quantity -= quantityToRemove;
                var result = stockRepository.UpdateItemAsync(itemToUpdate).Result;
            
                if (result == 0)
                {
                    // Success
                    AnsiConsole.MarkupLine("[bold green]Successfully updated the item.[/]");
                    // Print the item
                    AnsiConsole.MarkupLine($"[bold green]Item ID:[/] {item.Id}");
                    AnsiConsole.MarkupLine($"[bold green]Name:[/] {item.Name}");
                    AnsiConsole.MarkupLine($"[bold green]Quantity:[/] {item.Quantity}");
                    AnsiConsole.MarkupLine($"[bold green]Group ID:[/] {item.Group}");
                }
                else
                {
                    // Failure
                    AnsiConsole.MarkupLine("[bold red]Failed to update the item.[/]");
                }

                break;
            }
        }

        // Press any key to continue (go back to the main menu)
        AnsiConsole.Prompt(new ContinuePrompt());
    }

    private static void ListItemsPage()
    {
        if (_serviceProvider == null) throw new InvalidOperationException("Service provider is not found.");
        AnsiConsole.Clear();

        RenderHeader();

        AnsiConsole.MarkupLine("[bold]List Items[/]");

        var stockRepository = _serviceProvider.GetRequiredService<IStockRepository>();
        var items = stockRepository.GetItemsInfo();

        if (items.Count == 0)
        {
            AnsiConsole.MarkupLine("[bold red]No items found.[/]");
        }
        else
        {
            var table = new Table();
            table.AddColumn("ID");
            table.AddColumn("Name");
            table.AddColumn("Group");
            table.AddColumn("Quantity");

            foreach (var item in items)
            {
                table.AddRow(item.Id.ToString(), item.Name, item.Group, item.Quantity.ToString());
            }

            AnsiConsole.Write(table);
        }

        // Press any key to continue (go back to the main menu)
        AnsiConsole.Prompt(new ContinuePrompt());
    }

    private static void AddGroupPage(string? redirect = null, string[]? args = null)
    {
        if (_serviceProvider == null) throw new InvalidOperationException("Service provider is not found.");
        AnsiConsole.Clear();

        RenderHeader();

        AnsiConsole.MarkupLine("[bold]Add Group[/]");

        // Get the name of the group
        var name = AnsiConsole.Ask<string>("Enter the name of the group:");

        // Check if the group already exists
        var stockRepository = _serviceProvider.GetRequiredService<IStockRepository>();
        var existingGroups = stockRepository.GetGroups();
        var group = existingGroups.FirstOrDefault(group => group.Name == name);

        int result;

        if (group == null)
        {
            // Create the group
            group = new Group
            {
                Id = Guid.NewGuid(),
                Name = name
            };

            // Add the group
            result = stockRepository.AddGroupAsync(group).Result;
        }
        else
        {
            // The group already exists
            AnsiConsole.MarkupLine("[bold red]The group already exists.[/]");
            result = 1;
        }

        if (result == 0)
        {
            // Success
            AnsiConsole.MarkupLine("[bold green]Successfully added the group.[/]");
            // Print the group
            AnsiConsole.MarkupLine($"[bold green]Group ID:[/] {group.Id}");
            AnsiConsole.MarkupLine($"[bold green]Name:[/] {group.Name}");
        }
        else
        {
            // Failure
            AnsiConsole.MarkupLine("[bold red]Failed to add the group.[/]");
        }

        // Press any key to continue (go back to the main menu)
        AnsiConsole.Prompt(new ContinuePrompt());

        if (redirect != null)
        {
            ProcessCommand(redirect, args);
        }
    }
}