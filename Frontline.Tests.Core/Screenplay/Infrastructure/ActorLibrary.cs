using Frontline.Tests.Core.Screenplay.Core;

namespace Frontline.Tests.Core.Screenplay.Infrastructure;

/// <summary>
/// Library for managing multiple actors in a test scenario.
/// Provides singleton access to actors and cleanup management.
/// </summary>
public class ActorLibrary
{
    private readonly Dictionary<string, Actor> _actors = [];

    /// <summary>
    /// Gets or creates an actor by name.
    /// </summary>
    public Actor GetActor(string name)
    {
        if (!_actors.TryGetValue(name, out var actor))
        {
            actor = new Actor(name);
            _actors[name] = actor;
        }

        return actor;
    }

    /// <summary>
    /// Gets all actors in the library.
    /// </summary>
    public IEnumerable<Actor> GetAllActors() => _actors.Values;

    /// <summary>
    /// Removes an actor from the library.
    /// </summary>
    public bool RemoveActor(string name) => _actors.Remove(name);

    /// <summary>
    /// Clears all actors from the library.
    /// </summary>
    public void Clear() => _actors.Clear();

    /// <summary>
    /// Gets the number of actors in the library.
    /// </summary>
    public int Count => _actors.Count;
}
