namespace Frontline.Tests.Core.Screenplay.Core;

/// <summary>User-level action composed of one or more Interactions (e.g., Login, SubmitForm).</summary>
public interface ITask
{
    /// <summary>Human-readable name used in reporting and logging.</summary>
    string TaskDescription { get; }

    /// <summary>Executes the task using the actor's abilities.</summary>
    Task PerformAsync(Actor actor);
}
