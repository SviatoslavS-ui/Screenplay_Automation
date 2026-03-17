using Frontline.Tests.Core.Screenplay.Core;

namespace Frontline.Tests.Core.Screenplay.Interactions;

/// <summary>Presses a keyboard key on the currently focused element.</summary>
public class PressKey(string key) : IInteraction
{
    public string Description => $"Press '{key}' key";

    public async Task PerformAsync(Actor actor)
    {
        var page = actor.UsesAbility<Abilities.BrowserAbility>().Page;
        await page.Keyboard.PressAsync(key);
    }
}
