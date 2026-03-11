namespace Frontline.Tests.Core.Screenplay.Core;

/// <summary>
/// Custom exception for Screenplay pattern framework errors.
/// </summary>
public class ScreenplayException : Exception
{
    public ScreenplayException(string message) : base(message) { }

    public ScreenplayException(string message, Exception innerException)
        : base(message, innerException) { }
}
