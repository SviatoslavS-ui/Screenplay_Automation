namespace Frontline.Tests.Core.Screenplay.Core;

/// <summary>Atomic UI operation (click, fill, wait, etc.); lowest-level building block composed into Tasks.</summary>
public interface IInteraction
{
    /// <summary>Human-readable name used in reporting and logging.</summary>
    string InteractionDescription { get; }

    /// <summary>Executes the interaction using the actor's abilities.</summary>
    Task PerformAsync(Actor actor);
}
