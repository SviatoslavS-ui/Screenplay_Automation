using Frontline.Tests.Core.Screenplay.Core;
using static Microsoft.Playwright.Assertions;

namespace Frontline.Tests.Core.Screenplay.Interactions;

/// <summary>Circuit health guard: asserts error-ui, reconnect modal, and loading overlays are all hidden.</summary>
public class WaitForBlazorReady(int timeoutMs = 10_000) : IInteraction
{
    public string Description => "Wait for Blazor circuit to be ready";

    public async Task PerformAsync(Actor actor)
    {
        var page = actor.UsesAbility<Abilities.BrowserAbility>().Page;
        await Expect(page.Locator("#blazor-error-ui"))
            .ToBeHiddenAsync(new() { Timeout = timeoutMs });
        await Expect(page.Locator("#components-reconnect-modal"))
            .ToBeHiddenAsync(new() { Timeout = timeoutMs });
        await Expect(page.Locator(".loading-overlay, .mud-overlay, [data-loading='true']"))
            .ToBeHiddenAsync(new() { Timeout = timeoutMs });
    }
}
