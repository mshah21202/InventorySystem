using System.Runtime.CompilerServices;
using Application;
using Application.Commands;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace InventorySystem;

// ReSharper disable once ClassNeverInstantiated.Global
public class Program
{
    // ReSharper disable once UnusedParameter.Local
    static int Main(string[] args)
    {
        // Setup DI container
        var services = new ServiceCollection();
        services.AddSingleton<IStockRepository, StockRepository>();
        var serviceProvider = services.BuildServiceProvider();

        if (args.Length == 0)
        {
            InteractiveMode.StartInteractiveMode(serviceProvider);
        }
        else
        {
            // Create a type registrar and register any dependencies.
            // A type registrar is an adapter for a DI framework.
            var registrar = new Application.Registrar.TypeRegistrar(services);

            var app = new CommandApp(registrar);
            app.Configure(config =>
            {
                config.ConfigureConsole(AnsiConsole.Console);
                config.SetHelpProvider<AddItemHelpProvider>();
                config.AddCommand<AddItemCommand>("add-item")
                    .WithDescription("Add a new item to the inventory.")
                    .WithExample("add-item", "Mouse", "5", "-g", "Peripherals");
                config.AddCommand<ListItemsCommand>("list-items")
                    .WithDescription("List all items in the inventory.");
            });
            return app.Run(args);
        }

        return 0;
    }
}