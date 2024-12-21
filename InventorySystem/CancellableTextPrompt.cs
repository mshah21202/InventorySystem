using Spectre.Console;

namespace InventorySystem;

public class CancellableTextPrompt(string prompt) : IPrompt<string?>
{
    /// <summary>
    /// Gets or sets the prompt style.
    /// </summary>
    public Style? PromptStyle { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether or not an empty result is valid.
    /// </summary>
    public bool AllowEmpty { get; set; }

    public string? DefaultValue { get; set; }

    public string? Show(IAnsiConsole console)
    {
        return ShowAsync(console, CancellationToken.None).GetAwaiter().GetResult();
    }

    public async Task<string?> ShowAsync(IAnsiConsole console, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(console);

        return await console.RunExclusive(async () =>
        {
            console.WriteLine();

            AnsiConsole.Write($"{prompt} ");

            while (true)
            {
                var input = await ReadLine(console, PromptStyle, false, null, true, null, cancellationToken)
                    .ConfigureAwait(false);

                // Nothing entered?
                if (string.IsNullOrWhiteSpace(input))
                {
                    if (input == null) return input;
                    
                    if (DefaultValue != null)
                    {
                        var defaultValue = DefaultValue;
                        console.Write(defaultValue);
                        console.WriteLine();
                        return DefaultValue;
                    }

                    if (!AllowEmpty)
                    {
                        continue;
                    }
                }

                console.WriteLine();

                return input;
            }
        }).ConfigureAwait(false);
    }

    async Task<string?> ReadLine(IAnsiConsole console, Style? style, bool secret, char? mask, bool cancellable = false,
        IEnumerable<string>? items = null, CancellationToken cancellationToken = default)
    {
        if (console is null)
        {
            throw new ArgumentNullException(nameof(console));
        }

        style ??= Style.Plain;
        var text = string.Empty;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var rawKey = await console.Input.ReadKeyAsync(true, cancellationToken).ConfigureAwait(false);
            if (rawKey == null)
            {
                continue;
            }

            var key = rawKey.Value;

            if (key.Key == ConsoleKey.Escape && cancellable)
            {
                return null;
            }

            if (key.Key == ConsoleKey.Enter)
            {
                return text;
            }


            if (key.Key == ConsoleKey.Backspace)
            {
                if (text.Length > 0)
                {
                    text = text.Substring(0, text.Length - 1);

                    console.Write("\b \b");
                }

                continue;
            }

            if (!char.IsControl(key.KeyChar))
            {
                text += key.KeyChar.ToString();
                var output = key.KeyChar.ToString();
                console.Write(output, style);
            }
        }
    }
}