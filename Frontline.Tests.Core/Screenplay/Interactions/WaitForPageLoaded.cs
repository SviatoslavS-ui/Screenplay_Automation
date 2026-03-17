using Frontline.Tests.Core.Screenplay.Configuration;
using Frontline.Tests.Core.Screenplay.Core;
using Microsoft.Playwright;

namespace Frontline.Tests.Core.Screenplay.Interactions;

/// <summary>Waits for data-pageloaded signal; falls back to DOMContentLoaded on pages without it. Use token ctor for SPA navigation.</summary>
public class WaitForPageLoaded : IInteraction
{
    private readonly string? _previousToken;
    private readonly int _timeoutMs;

    // Short probe — OnAfterRenderAsync fires within 1-3s; pages without the signal fall back fast.
    private const int ProbeTimeoutMs = 5_000;

    /// <summary>Full page load or cross-app navigation — wait for signal to appear.</summary>
    public static WaitForPageLoaded ForFullLoad(int timeoutMs = 15_000) => new(timeoutMs);

    /// <summary>SPA navigation within same app — wait for signal value to change from the captured token.</summary>
    public static WaitForPageLoaded ForSpaNavigation(string previousToken, int timeoutMs = 15_000) => new(previousToken, timeoutMs);

    private WaitForPageLoaded(int timeoutMs = 15_000)
    {
        _previousToken = null;
        _timeoutMs = timeoutMs;
    }

    private WaitForPageLoaded(string previousToken, int timeoutMs = 15_000)
    {
        _previousToken = previousToken;
        _timeoutMs = timeoutMs;
    }

    public string Description => _previousToken == null
        ? "Wait for Blazor page to load (data-pageloaded appears)"
        : "Wait for Blazor page to reload (data-pageloaded value changes)";

    public async Task PerformAsync(Actor actor)
    {
        var page = actor.UsesAbility<Abilities.BrowserAbility>().Page;

        if (_previousToken == null)
        {
            // Fresh load: wait for data-pageloaded to settle to a real GUID (skips the intermediate "loading" value).
            try
            {
                await page.WaitForFunctionAsync(
                    $"() => {{ var v = document.querySelector('{AppConfiguration.BlazorPageContainerSelector}')?.getAttribute('data-pageloaded'); return v && v !== 'loading'; }}",
                    null,
                    new PageWaitForFunctionOptions { Timeout = ProbeTimeoutMs });
                var token = await page.Locator(AppConfiguration.BlazorPageContainerSelector)
                    .GetAttributeAsync("data-pageloaded");
                TestContext.Out.WriteLine($"[WaitForPageLoaded] signal found — token={token}");
            }
            catch (TimeoutException)
            {
                TestContext.Out.WriteLine(
                    "[WaitForPageLoaded] signal not found — falling back to DOMContentLoaded.");
                await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
            }
        }
        else
        {
            // SPA navigation: wait for token to change from the pre-navigation snapshot.
            TestContext.Out.WriteLine($"[WaitForPageLoaded] waiting for token to change from '{_previousToken}'");
            await page.WaitForFunctionAsync(
                $"document.querySelector('{AppConfiguration.BlazorPageContainerSelector}')?.getAttribute('data-pageloaded') !== '{_previousToken}'",
                null,
                new PageWaitForFunctionOptions { Timeout = _timeoutMs }
            );
            var newToken = await page.Locator(AppConfiguration.BlazorPageContainerSelector)
                .GetAttributeAsync("data-pageloaded");
            TestContext.Out.WriteLine($"[WaitForPageLoaded] token changed — new token={newToken}");
        }
    }
}
