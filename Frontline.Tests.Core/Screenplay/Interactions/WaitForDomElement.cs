using Frontline.Tests.Core.Screenplay.Core;
using Microsoft.Playwright;

namespace Frontline.Tests.Core.Screenplay.Interactions;

/// <summary>Waits for a DOM element using JavaScript querySelector. Use when Playwright's Locator
/// visibility check is blocked by overlays (e.g. EJ2 dialog overlay over autocomplete popups).</summary>
public class WaitForDomElement(string cssSelector, int timeoutMs = 10_000) : IInteraction
{
    public string Description => $"Wait for DOM element '{cssSelector}' (JS, timeout: {timeoutMs}ms)";

    public async Task PerformAsync(Actor actor)
    {
        var page = actor.UsesAbility<Abilities.BrowserAbility>().Page;
        await page.WaitForFunctionAsync(
            $"() => !!document.querySelector(\"{cssSelector}\")",
            null,
            new PageWaitForFunctionOptions { Timeout = timeoutMs });
    }
}
