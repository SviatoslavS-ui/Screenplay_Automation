using Frontline.Tests.Core.Screenplay.Core;
using Microsoft.Playwright;

namespace Frontline.Tests.Core.Screenplay.Abilities;

/// <summary>
/// Ability for an Actor to browse web applications using Playwright.
/// This ability manages the browser, context, and page lifecycle.
/// </summary>
public class BrowserAbility : IAbility
{
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;
    private IPlaywright? _playwright;

    public string AbilityName => "BrowserAbility";

    public IBrowser Browser => _browser ?? throw new InvalidOperationException("Browser not initialized. Call InitializeAsync first.");
    public IBrowserContext Context => _context ?? throw new InvalidOperationException("Context not initialized. Call InitializeAsync first.");
    public IPage Page => _page ?? throw new InvalidOperationException("Page not initialized. Call InitializeAsync first.");

    /// <summary>
    /// Initializes the browser with Chromium (default).
    /// </summary>
    public async Task InitializeAsync(BrowserTypeLaunchOptions? options = null)
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(options ?? new BrowserTypeLaunchOptions { Headless = true });
        _context = await _browser.NewContextAsync();
        _page = await _context.NewPageAsync();
    }
   
    /// <summary>
    /// Cleans up browser resources.
    /// </summary>
    public async Task CloseAsync()
    {
        if (_page != null)
            await _page.CloseAsync();

        if (_context != null)
            await _context.CloseAsync();

        if (_browser != null)
            await _browser.CloseAsync();

        _playwright?.Dispose();
    }
}
