namespace Frontline.Tests.Core.Screenplay.Core;

/// <summary>Thrown when actor or ability constraints are violated within the Screenplay framework.</summary>
public class ScreenplayException : Exception
{
    public ScreenplayException(string message) : base(message) { }

    public ScreenplayException(string message, Exception innerException)
        : base(message, innerException) { }
}
