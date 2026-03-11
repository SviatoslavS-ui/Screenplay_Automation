namespace Frontline.Tests.Core.Screenplay.Core;

/// <summary>
/// Represents a capability that an Actor can possess and use to perform tasks.
/// Abilities are reusable, modular units of test functionality (e.g., browser navigation, API calls).
/// </summary>
public interface IAbility
{
    /// <summary>
    /// The unique identifier for this ability within an actor's ability set.
    /// </summary>
    string AbilityName { get; }
}
