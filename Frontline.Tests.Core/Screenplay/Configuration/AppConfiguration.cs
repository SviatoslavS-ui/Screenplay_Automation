namespace Frontline.Tests.Core.Screenplay.Configuration;

/// <summary>Application URLs and browser flags.</summary>
public static class AppConfiguration
{
    /// <summary>Frontline home portal. Title: "FLGroup Apps".</summary>
    public const string BaseUrl = "https://dotnettest.flgroup.co.uk/";

    /// <summary>Magazine Exceptions app root (port 10143).</summary>
    public const string MagazineExceptionsAppUrl = "https://dotnettest.flgroup.co.uk:10143/";

    /// <summary>Direct link to the Magazine Exceptions grid, bypasses home portal.</summary>
    public const string MagazineExceptionsUrl = MagazineExceptionsAppUrl + "MagazineExceptions";

    /// <summary>false = headed (visible browser), true = headless.</summary>
    public const bool RunHeadless = false;

    /// <summary>Launch browser maximized. Requires RunHeadless = false.</summary>
    public const bool StartMaximized = true;
}