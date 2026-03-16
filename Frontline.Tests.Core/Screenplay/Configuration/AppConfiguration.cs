namespace Frontline.Tests.Core.Screenplay.Configuration;

/// <summary>Application URLs and browser flags. Override via environment variables for CI/environment targeting.</summary>
public static class AppConfiguration
{
    /// <summary>Frontline home portal. Override with FRONTLINE_BASE_URL env var.</summary>
    public static string BaseUrl =>
        Environment.GetEnvironmentVariable("FRONTLINE_BASE_URL")
        ?? "https://dotnettest.flgroup.co.uk/";

    /// <summary>Magazine Exceptions app root (port 10143). Override with FRONTLINE_MAG_EXCEPTIONS_URL env var.</summary>
    public static string MagazineExceptionsAppUrl =>
        Environment.GetEnvironmentVariable("FRONTLINE_MAG_EXCEPTIONS_URL")
        ?? "https://dotnettest.flgroup.co.uk:10143/";

    /// <summary>Direct link to the Magazine Exceptions grid, bypasses home portal.</summary>
    public static string MagazineExceptionsUrl => MagazineExceptionsAppUrl + "MagazineExceptions";

    /// <summary>Set PLAYWRIGHT_HEADLESS=true for CI. Defaults to false (headed browser).</summary>
    public static bool RunHeadless =>
        bool.TryParse(Environment.GetEnvironmentVariable("PLAYWRIGHT_HEADLESS"), out var v) && v;

    /// <summary>Maximize browser window. Automatically false in headless mode.</summary>
    public static bool StartMaximized => !RunHeadless;

    /// <summary>Dev team's render-complete signal — set in OnAfterRenderAsync.</summary>
    public const string BlazorPageLoadedSelector = ".control-container[data-pageloaded]";

    /// <summary>Base container selector used to detect SPA navigation token changes.</summary>
    public const string BlazorPageContainerSelector = ".control-container";
}
