using Frontline.Tests.Core.Screenplay.Core;

namespace Frontline.Tests.Core.Screenplay.Interactions;

/// <summary>Clears and fills a text input. Playwright FillAsync inherently clears before filling.</summary>
public class Fill(string selector, string text) : IInteraction
{
    public string Description => $"Clear and fill '{selector}' with '{text}'";

    public async Task PerformAsync(Actor actor)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(text);

        var browserAbility = actor.UsesAbility<Abilities.BrowserAbility>();
        await browserAbility.Page.Locator(selector).FillAsync(text);
    }
}
