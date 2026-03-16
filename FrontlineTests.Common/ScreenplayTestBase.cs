using Microsoft.Playwright;
using NUnit.Framework.Interfaces;
using Frontline.Tests.Core.Screenplay.Abilities;
using Frontline.Tests.Core.Screenplay.Configuration;
using Frontline.Tests.Core.Screenplay.Infrastructure;

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
                // Headless mode requires an explicit viewport — NoViewport has no effect without a display.
                // In headed mode, NoViewport lets the OS window size dictate dimensions.
                ViewportSize = AppConfiguration.RunHeadless
                    ? new ViewportSize { Width = 1920, Height = 1080 }
                    : (AppConfiguration.StartMaximized ? ViewportSize.NoViewport : null)
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
        var outputRoot = Environment.GetEnvironmentVariable("TEST_ARTIFACTS_DIR")
            ?? Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestResults");
        var tracesDir = Path.Combine(outputRoot, "traces");
        var screenshotsDir = Path.Combine(outputRoot, "screenshots");

        foreach (var actor in ActorLibrary.GetAllActors())
        {
            if (actor.TryGetAbility<BrowserAbility>(out var browserAbility) && browserAbility != null)
            {
                Directory.CreateDirectory(tracesDir);
                await browserAbility.Context.Tracing.StopAsync(new()
                {
                    Path = Path.Combine(tracesDir, $"{TestContext.CurrentContext.Test.Name}.zip")
                });

                if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
                {
                    Directory.CreateDirectory(screenshotsDir);
                    await browserAbility.Page.ScreenshotAsync(new()
                    {
                        Path = Path.Combine(screenshotsDir, $"{TestContext.CurrentContext.Test.Name}.png"),
                        FullPage = true
                    });
                }

                await browserAbility.CloseAsync();
            }
        }

        ActorLibrary.Clear();
    }
}
