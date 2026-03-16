namespace Frontline.Tests.Core.Screenplay.Core;

/// <summary>Shared contract for all executable screenplay elements (tasks and interactions).</summary>
public interface IPerformable
{
    /// <summary>Human-readable description used in reporting and logging.</summary>
    string Description { get; }

    /// <summary>Executes this performable using the actor's abilities.</summary>
    Task PerformAsync(Actor actor);
}
