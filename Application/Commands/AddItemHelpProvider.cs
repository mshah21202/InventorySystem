using Spectre.Console;
using Spectre.Console.Cli.Help;
using Spectre.Console.Rendering;

namespace Application.Commands;

public class AddItemHelpProvider : IHelpProvider
{
    public IEnumerable<IRenderable> Write(ICommandModel model, ICommandInfo? command)
    {
        var result = new List<IRenderable>
        {
            new Markup("Add a new item to the inventory." + Environment.NewLine)
        };

        if (command is null) return result;
        result.Add(new Markup("Usage:" + Environment.NewLine));
        result.Add(new Text($"  {command.Name} <NAME> <QUANTITY> [-g|--group <GROUP>]" + Environment.NewLine));

        result.Add(new Markup("Arguments:" + Environment.NewLine));
        result.Add(new Markup("  <NAME>      The name of the item to add." + Environment.NewLine));
        result.Add(new Markup("  <QUANTITY>  The quantity of the item to add." + Environment.NewLine));

        result.Add(new Markup("Options:" + Environment.NewLine));
        result.Add(new Markup("  -g, --group <GROUP>  The group to add the item to." + Environment.NewLine));

        return result;
    }
}