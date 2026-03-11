using Frontline.Tests.Core.Screenplay.Core;

namespace Frontline.Tests.Core.Screenplay.Interactions;

/// <summary>
/// Atomic interaction: Fill a text input field.
/// </summary>
public class Fill(string selector, string text) : IInteraction
{
    public string InteractionDescription => $"Fill '{selector}' with '{text}'";

    public async Task PerformAsync(Actor actor)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(text);
        
        var browserAbility = actor.UsesAbility<Abilities.BrowserAbility>("BrowserAbility");
        await browserAbility.Page.FillAsync(selector, text);
    }
}
