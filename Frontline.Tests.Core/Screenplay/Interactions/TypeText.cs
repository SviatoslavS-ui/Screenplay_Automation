using Frontline.Tests.Core.Screenplay.Core;
using NUnit.Framework;

namespace Frontline.Tests.Core.Screenplay.Interactions;

/// <summary>Types text character-by-character into a field. Use instead of Fill when the target
/// requires keyboard events (e.g. Blazor SfAutoComplete search triggers).</summary>
public class TypeText(string selector, string text, int delayMs = 200) : IInteraction
{
    public string Description => $"Type '{text}' into '{selector}'";

    public async Task PerformAsync(Actor actor)
    {
        var locator = actor.UsesAbility<Abilities.BrowserAbility>().Page.Locator(selector);
        var currentDelay = delayMs;

        for (var attempt = 1; attempt <= 3; attempt++)
        {
            await locator.ClearAsync();
            await locator.PressSequentiallyAsync(text, new() { Delay = currentDelay });

            var actual = await locator.InputValueAsync();
            TestContext.Out.WriteLine($"[TypeText] attempt={attempt} expected='{text}' actual='{actual}' match={actual == text}");

            if (actual == text)
                return;
           
            TestContext.Out.WriteLine($"[TypeText] value mismatch — waiting for Blazor then retrying at {currentDelay + 100}ms/char");
            await actor.Performs(new WaitForBlazorReady());
            currentDelay += 100;
        }
    }
}
