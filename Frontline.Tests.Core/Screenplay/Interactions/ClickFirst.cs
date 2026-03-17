using Frontline.Tests.Core.Screenplay.Core;

namespace Frontline.Tests.Core.Screenplay.Interactions;

/// <summary>Clicks the first element matching the selector — avoids strict-mode failures when multiple matches exist (e.g. EJ2 popup list items).</summary>
public class ClickFirst(string selector) : IInteraction
{
    public string Description => $"Click first element matching '{selector}'";

    public async Task PerformAsync(Actor actor)
    {
        ArgumentNullException.ThrowIfNull(selector);

        var page = actor.UsesAbility<Abilities.BrowserAbility>().Page;
        await page.Locator(selector).First.ClickAsync();
    }
}
