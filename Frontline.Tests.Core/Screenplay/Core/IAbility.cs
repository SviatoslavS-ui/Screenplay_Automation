namespace Frontline.Tests.Core.Screenplay.Core;

/// <summary>Capability an actor can possess to perform tasks (e.g., browser navigation, API calls).</summary>
public interface IAbility
{
    /// <summary>Unique key identifying this ability within an actor's ability set.</summary>
    string AbilityName { get; }
}
