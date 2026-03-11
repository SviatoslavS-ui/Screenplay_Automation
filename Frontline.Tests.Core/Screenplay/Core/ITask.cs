namespace Frontline.Tests.Core.Screenplay.Core;

/// <summary>
/// Represents a task that an Actor can perform.
/// Tasks are composed of one or more Interactions and represent user-level actions.
/// Example: "Login" (composed of "Fill username", "Fill password", "Click login button")
/// </summary>
public interface ITask
{
    /// <summary>
    /// A descriptive name for this task (used in reporting and logging).
    /// </summary>
    string TaskDescription { get; }

    /// <summary>
    /// Executes this task using the actor's abilities.
    /// </summary>
    /// <param name="actor">The actor performing the task.</param>
    Task PerformAsync(Actor actor);
}
