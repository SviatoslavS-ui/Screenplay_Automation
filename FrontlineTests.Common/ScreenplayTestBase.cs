using Microsoft.Playwright;
using NUnit.Framework.Interfaces;
using Frontline.Tests.Core.Screenplay.Abilities;
using Frontline.Tests.Core.Screenplay.Configuration;
using Frontline.Tests.Core.Screenplay.Infrastructure;
using static Microsoft.Playwright.Assertions;

namespace FrontlineTests.Common;

/// <summary>Base class for all Screenplay-based test fixtures. Manages actor lifecycle, browser init, and cleanup.</summary>
[TestFixture]
public abstract class ScreenplayTestBase
{
    protected ActorLibrary ActorLibrary { get; private set; } = null!;

    [SetUp]
    public virtual async Task SetUp()
    {
        ActorLibrary = new ActorLibrary();
        await InitializeActorsAsync();
    }

    /// <summary>Override to customize actor setup; default creates a "User" actor with a browser ability.</summary>
    protected virtual async Task InitializeActorsAsync()
    {
        var browserAbility = new BrowserAbility();
        await browserAbility.InitializeAsync(
            new BrowserTypeLaunchOptions
            {
                Headless = AppConfiguration.RunHeadless,
                Args = AppConfiguration.StartMaximized ? ["--start-maximized"] : []
            },
            new BrowserNewContextOptions
            {
                ViewportSize = AppConfiguration.StartMaximized ? ViewportSize.NoViewport : null
            });

        ActorLibrary.GetActor("User").Can(browserAbility);

        await browserAbility.Context.Tracing.StartAsync(new()
        {
            Screenshots = true,
            Snapshots = true,
            Sources = true
        });
    }

    [TearDown]
    public virtual async Task TearDown()
    {
        foreach (var actor in ActorLibrary.GetAllActors())
        {
            if (actor.TryGetAbility<BrowserAbility>("BrowserAbility", out var browserAbility) && browserAbility != null)
            {
                Directory.CreateDirectory("traces");
                await browserAbility.Context.Tracing.StopAsync(new()
                {
                    Path = $"traces/{TestContext.CurrentContext.Test.Name}.zip"
                });

                if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
                {
                    Directory.CreateDirectory("screenshots");
                    await browserAbility.Page.ScreenshotAsync(new()
                    {
                        Path = $"screenshots/{TestContext.CurrentContext.Test.Name}.png",
                        FullPage = true
                    });
                }

                await browserAbility.CloseAsync();
            }
        }

        ActorLibrary.Clear();
    }

    /// <summary>Guard: Verify Blazor circuit is healthy (error UI hidden, no reconnect modal). Call after critical navigation or state-changing actions.</summary>
    protected async Task AssertBlazorCircuitHealthy(IPage page)
    {
        await Expect(page.Locator("#blazor-error-ui"))
            .ToBeHiddenAsync(new() { Timeout = 5000 });
        await Expect(page.Locator("#components-reconnect-modal"))
            .ToBeHiddenAsync(new() { Timeout = 5000 });
    }

    /// <summary>Guard: Wait for loading overlays to disappear. Call before critical interactions that depend on UI being ready.</summary>
    protected async Task WaitForLoadingOverlays(IPage page)
    {
        await Expect(page.Locator(".loading-overlay, .mud-overlay, [data-loading='true']"))
            .ToBeHiddenAsync(new() { Timeout = 5000 });
    }
}