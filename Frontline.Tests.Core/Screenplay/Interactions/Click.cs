using Frontline.Tests.Core.Screenplay.Core;

namespace Frontline.Tests.Core.Screenplay.Interactions;

/// <summary>
/// Atomic interaction: Click an element.
/// </summary>
public class Click(string selector) : IInteraction
{
    public string InteractionDescription => $"Click element '{selector}'";

    public async Task PerformAsync(Actor actor)
    {
        ArgumentNullException.ThrowIfNull(selector);
        
        var browserAbility = actor.UsesAbility<Abilities.BrowserAbility>("BrowserAbility");
        await browserAbility.Page.ClickAsync(selector);
    }
}
