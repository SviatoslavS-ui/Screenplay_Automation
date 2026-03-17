using Frontline.Tests.Core.Screenplay.Core;

namespace Frontline.Tests.Core.Screenplay.Interactions;

/// <summary>Types text character-by-character into a field. Use instead of Fill when the target
/// requires keyboard events (e.g. EJ2 autocomplete search triggers).</summary>
public class TypeText(string selector, string text, int delayMs = 50) : IInteraction
{
    public string Description => $"Type '{text}' into '{selector}'";

    public async Task PerformAsync(Actor actor)
    {
        var browserAbility = actor.UsesAbility<Abilities.BrowserAbility>();
        var locator = browserAbility.Page.Locator(selector);
        await locator.ClearAsync();
        await locator.PressSequentiallyAsync(text, new() { Delay = delayMs });
    }
}
