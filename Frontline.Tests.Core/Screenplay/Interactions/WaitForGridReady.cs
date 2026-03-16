using Frontline.Tests.Core.Screenplay.Core;
using Microsoft.Playwright;

namespace Frontline.Tests.Core.Screenplay.Interactions;

/// <summary>Waits for EJ2 grid aria-busy to clear after filter/sort/page. No-op if grid doesn't emit it.</summary>
public class WaitForGridReady(string gridSelector = "#MagGrid", int timeoutMs = 10_000) : IInteraction
{
    public string Description => $"Wait for grid '{gridSelector}' to finish loading";

    public async Task PerformAsync(Actor actor)
    {
        var page = actor.UsesAbility<Abilities.BrowserAbility>().Page;

        await page.Locator($"{gridSelector}[aria-busy='true']")
            .WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = timeoutMs });
    }
}
