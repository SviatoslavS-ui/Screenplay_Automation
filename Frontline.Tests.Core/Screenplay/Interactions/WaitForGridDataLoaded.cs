using Frontline.Tests.Core.Screenplay.Core;
using Microsoft.Playwright;

namespace Frontline.Tests.Core.Screenplay.Interactions;

/// <summary>Waits for the grid-dataloaded attribute — either to appear (initial load) or to change (after action).</summary>
public class WaitForGridDataLoaded : IInteraction
{
    private readonly string? _previousToken;
    private readonly string _gridSelector;
    private readonly int _timeoutMs;

    /// <summary>Wait for grid-dataloaded to appear for the first time (initial page load).</summary>
    public static WaitForGridDataLoaded ForInitialLoad(string gridSelector = "#MagGrid", int timeoutMs = 15_000)
        => new(null, gridSelector, timeoutMs);

    /// <summary>Wait for grid data to reload after a mutating action (add, delete, filter).</summary>
    public static WaitForGridDataLoaded AfterAction(string previousToken, string gridSelector = "#MagGrid", int timeoutMs = 10_000)
        => new(previousToken, gridSelector, timeoutMs);

    private WaitForGridDataLoaded(string? previousToken, string gridSelector, int timeoutMs)
    {
        _previousToken = previousToken;
        _gridSelector = gridSelector;
        _timeoutMs = timeoutMs;
    }

    public string Description => _previousToken == null
        ? "Wait for grid data to load (grid-dataloaded appears)"
        : "Wait for grid data to reload (grid-dataloaded value changes)";

    public async Task PerformAsync(Actor actor)
    {
        var page = actor.UsesAbility<Abilities.BrowserAbility>().Page;

        if (_previousToken == null)
        {
            TestContext.Out.WriteLine("[WaitForGridDataLoaded] waiting for grid-dataloaded to appear");
            await page.WaitForFunctionAsync(
                $"() => !!document.querySelector('{_gridSelector}')?.getAttribute('grid-dataloaded')",
                null,
                new PageWaitForFunctionOptions { Timeout = _timeoutMs });
        }
        else
        {
            TestContext.Out.WriteLine($"[WaitForGridDataLoaded] waiting for token to change from '{_previousToken}'");
            await page.WaitForFunctionAsync(
                $"document.querySelector('{_gridSelector}')?.getAttribute('grid-dataloaded') !== '{_previousToken}'",
                null,
                new PageWaitForFunctionOptions { Timeout = _timeoutMs });
        }

        var token = await page.Locator(_gridSelector).GetAttributeAsync("grid-dataloaded");
        TestContext.Out.WriteLine($"[WaitForGridDataLoaded] grid data ready — token={token}");
    }
}
