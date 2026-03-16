using Frontline.Tests.Core.Screenplay.Abilities;
using Frontline.Tests.Core.Screenplay.Core;
using Frontline.Tests.Core.Screenplay.Questions;
using static Microsoft.Playwright.Assertions;

namespace FrontlineTests.Common;

/// <summary>Fluent assertion extensions on Actor — replaces ask-then-Assert boilerplate.</summary>
public static class ScreenplayAssertions
{
    /// <summary>Asserts the element is visible. Fails with a clear message if not.</summary>
    public static async Task ShouldSee(this Actor actor, string selector, string? because = null)
    {
        var visible = await actor.Asks(new IsVisible(selector));
        Assert.That(visible, Is.True, because ?? $"Expected '{selector}' to be visible");
    }

    /// <summary>Asserts the element is not visible. Fails with a clear message if it is.</summary>
    public static async Task ShouldNotSee(this Actor actor, string selector, string? because = null)
    {
        var visible = await actor.Asks(new IsVisible(selector));
        Assert.That(visible, Is.False, because ?? $"Expected '{selector}' to be hidden");
    }

    /// <summary>Asserts the element's trimmed text equals the expected value.</summary>
    public static async Task ShouldRead(this Actor actor, string selector, string expected, string? because = null)
    {
        var text = await actor.Asks(new TextOf(selector));
        Assert.That(text?.Trim(), Is.EqualTo(expected),
            because ?? $"Expected '{selector}' to read '{expected}'");
    }

    /// <summary>Asserts the element's trimmed text contains the expected substring.</summary>
    public static async Task ShouldReadContaining(this Actor actor, string selector, string expected, string? because = null)
    {
        var text = await actor.Asks(new TextOf(selector));
        Assert.That(text?.Trim(), Does.Contain(expected),
            because ?? $"Expected '{selector}' to contain '{expected}'");
    }

    /// <summary>Asserts the element's trimmed text matches the expected value (via HasText question).</summary>
    public static async Task ShouldHaveText(this Actor actor, string selector, string expected, string? because = null)
    {
        var matches = await actor.Asks(new HasText(selector, expected));
        Assert.That(matches, Is.True,
            because ?? $"Expected '{selector}' to have text '{expected}'");
    }

    /// <summary>Asserts the page title contains the expected substring.</summary>
    public static async Task ShouldHaveTitle(this Actor actor, string expected, string? because = null)
    {
        var title = await actor.Asks(new PageTitle());
        Assert.That(title, Does.Contain(expected),
            because ?? $"Expected page title to contain '{expected}'");
    }

    /// <summary>Asserts the element count is greater than the specified minimum.</summary>
    public static async Task ShouldHaveAtLeast(this Actor actor, string selector, int minimum, string? because = null)
    {
        var count = await actor.Asks(new CountOf(selector));
        Assert.That(count, Is.GreaterThan(minimum - 1),
            because ?? $"Expected at least {minimum} element(s) matching '{selector}', found {count}");
    }

    /// <summary>Retrying assertion — waits until element text contains expected value. Use after actions that trigger async re-renders.</summary>
    public static async Task ShouldEventuallyRead(this Actor actor, string selector, string expected, int timeoutMs = 5_000)
    {
        var page = actor.UsesAbility<BrowserAbility>().Page;
        await Expect(page.Locator(selector))
            .ToContainTextAsync(expected, new() { Timeout = timeoutMs });
    }

    /// <summary>Retrying assertion — waits until element text no longer contains the value. Use to detect grid page changes.</summary>
    public static async Task ShouldEventuallyNotRead(this Actor actor, string selector, string notExpected, int timeoutMs = 5_000)
    {
        var page = actor.UsesAbility<BrowserAbility>().Page;
        await Expect(page.Locator(selector))
            .Not.ToContainTextAsync(notExpected, new() { Timeout = timeoutMs });
    }

    /// <summary>Retrying assertion — waits until at least one matching element becomes visible. Use after async re-renders where element may not exist yet.</summary>
    public static async Task ShouldEventuallySee(this Actor actor, string selector, int timeoutMs = 5_000)
    {
        var page = actor.UsesAbility<BrowserAbility>().Page;
        await Expect(page.Locator(selector).First)
            .ToBeVisibleAsync(new() { Timeout = timeoutMs });
    }

    /// <summary>Retrying assertion — waits until the matching element becomes hidden or removed. Use after actions that dismiss UI elements.</summary>
    public static async Task ShouldEventuallyNotSee(this Actor actor, string selector, int timeoutMs = 5_000)
    {
        var page = actor.UsesAbility<BrowserAbility>().Page;
        await Expect(page.Locator(selector).First)
            .ToBeHiddenAsync(new() { Timeout = timeoutMs });
    }
}
