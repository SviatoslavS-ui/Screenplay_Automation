using Frontline.Tests.Core.Screenplay.Core;

namespace Frontline.Tests.Core.Screenplay.Interactions;

/// <summary>Waits for an element matching the selector to be visible in the DOM.</summary>
public class WaitForElement(string selector, int timeoutMs = 5000) : IInteraction
{
    public string InteractionDescription => $"Wait for element '{selector}' (timeout: {timeoutMs}ms)";

    public async Task PerformAsync(Actor actor)
    {
        ArgumentNullException.ThrowIfNull(selector);
        
        var browserAbility = actor.UsesAbility<Abilities.BrowserAbility>("BrowserAbility");
        await browserAbility.Page.WaitForSelectorAsync(selector, new() { Timeout = timeoutMs });
    }
}