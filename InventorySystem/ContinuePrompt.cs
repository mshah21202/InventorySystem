using Spectre.Console;

namespace InventorySystem;

public class ContinuePrompt : IPrompt<bool>
{
    public bool Show(IAnsiConsole console)
    {
        return ShowAsync(console, CancellationToken.None).GetAwaiter().GetResult();
    }

    public async Task<bool> ShowAsync(IAnsiConsole console, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(console);

        return await console.RunExclusive(async () =>
        {
            AnsiConsole.WriteLine("Press any key to continue...");

            while (true)
            {
                var input = await console.Input.ReadKeyAsync(true, cancellationToken).ConfigureAwait(false);

                if (input != null)
                {
                    return true;
                }
            }
        }).ConfigureAwait(false);
    }
}