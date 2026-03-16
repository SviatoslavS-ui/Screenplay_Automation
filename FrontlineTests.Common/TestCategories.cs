namespace FrontlineTests.Common;

/// <summary>NUnit category constants. Use with [Category(TestCategories.Smoke)] instead of magic strings.</summary>
public static class TestCategories
{
    public const string Smoke      = "Smoke";
    public const string Functional = "Functional";
    public const string EdgeCase   = "EdgeCase";
    public const string Security   = "Security";
    public const string Regression = "Regression";
}
