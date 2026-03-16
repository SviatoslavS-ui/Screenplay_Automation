using Frontline.Tests.Core.Screenplay.Core;

namespace Frontline.Tests.Core.Screenplay.Interactions;

/// <summary>Clicks the target element.</summary>
public class Click(string selector) : IInteraction
{
    public string Description => $"Click element '{selector}'";

    public async Task PerformAsync(Actor actor)
    {
        ArgumentNullException.ThrowIfNull(selector);

        var browserAbility = actor.UsesAbility<Abilities.BrowserAbility>();
        await browserAbility.Page.Locator(selector).ClickAsync();
    }
}
