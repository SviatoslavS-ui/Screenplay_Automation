namespace Frontline.Tests.Core.Screenplay.Core;

/// <summary>
/// Represents an atomic interaction that an Actor performs on the application.
/// Interactions are the lowest-level operations (clicks, fills, waits, etc.).
/// Multiple interactions can be composed into Tasks.
/// </summary>
public interface IInteraction
{
    /// <summary>
    /// A descriptive name for this interaction (used in reporting and logging).
    /// </summary>
    string InteractionDescription { get; }

    /// <summary>
    /// Performs this interaction using the actor's abilities.
    /// </summary>
    /// <param name="actor">The actor performing the interaction.</param>
    Task PerformAsync(Actor actor);
}
