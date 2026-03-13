using Frontline.Tests.Core.Screenplay.Core;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace Frontline.Tests.Core.Screenplay.Interactions;

/// <summary>Clicks an ARIA toggle button (aria-expanded) and waits for the state change; resets stale state first.</summary>
public class ClickAriaToggle : IInteraction
{
    private readonly string _locatorExpression;
    private readonly string _accessibleName;
    private readonly bool _expandToState;
    private const int DefaultTimeoutMs = 5000;

    /// <summary>Click to expand: locate button by accessible name, wait for aria-expanded=true.</summary>
    public static ClickAriaToggle ToExpand(string accessibleName) => new(accessibleName, true);

    /// <summary>Click to collapse: locate button by accessible name, wait for aria-expanded=false.</summary>
    public static ClickAriaToggle ToCollapse(string accessibleName) => new(accessibleName, false);

    private ClickAriaToggle(string accessibleName, bool expandToState)
    {
        _accessibleName = accessibleName;
        _expandToState = expandToState;
        _locatorExpression = $"[aria-label=\"{accessibleName}\"]";
    }

    public string InteractionDescription => $"Click ARIA toggle '{_accessibleName}' to {(_expandToState ? "expand" : "collapse")}";

    public async Task PerformAsync(Actor actor)
    {
        var page = actor.UsesAbility<Abilities.BrowserAbility>("BrowserAbility").Page;
        var button = page.GetByRole(AriaRole.Button, new() { Name = _accessibleName });

        await Expect(page.Locator("#blazor-error-ui")).ToBeHiddenAsync(new() { Timeout = DefaultTimeoutMs });
        await Expect(page.Locator("#components-reconnect-modal")).ToBeHiddenAsync(new() { Timeout = DefaultTimeoutMs });
        await Expect(button).ToBeEnabledAsync(new() { Timeout = DefaultTimeoutMs });

        var targetValue = _expandToState ? "true" : "false";
        var resetValue = _expandToState ? "false" : "true";

        // Reset stale state: if already at target, click away first
        var currentState = await page.GetAttributeAsync(_locatorExpression, "aria-expanded");
        if (currentState == targetValue)
        {
            await button.ClickAsync(new() { Force = true });
            await WaitForAriaExpanded(page, resetValue);
        }

        // Click to reach target state
        await button.ClickAsync(new() { Force = true });
        await WaitForAriaExpanded(page, targetValue);
    }

    private Task WaitForAriaExpanded(IPage page, string expectedValue) =>
        page.WaitForFunctionAsync(
            $"document.querySelector('{_locatorExpression}')?.getAttribute('aria-expanded') === '{expectedValue}'",
            null,
            new PageWaitForFunctionOptions { Timeout = DefaultTimeoutMs }
        );
}
