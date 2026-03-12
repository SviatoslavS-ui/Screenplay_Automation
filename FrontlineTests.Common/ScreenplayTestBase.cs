using Microsoft.Playwright;
using Frontline.Tests.Core.Screenplay.Abilities;
using Frontline.Tests.Core.Screenplay.Configuration;
using Frontline.Tests.Core.Screenplay.Infrastructure;

namespace FrontlineTests.Common;

/// <summary>
/// Base class for all Screenplay-based test fixtures.
/// Handles actor library lifecycle, browser initialization, and cleanup.
/// </summary>
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

    /// <summary>
    /// Override to customize actor setup per fixture.
    /// Default behavior: creates a "User" actor with a browser ability.
    /// </summary>
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
    }

    [TearDown]
    public virtual async Task TearDown()
    {
        foreach (var actor in ActorLibrary.GetAllActors())
        {
            if (actor.TryGetAbility<BrowserAbility>("BrowserAbility", out var browserAbility) && browserAbility != null)
                await browserAbility.CloseAsync();
        }

        ActorLibrary.Clear();
    }
}
