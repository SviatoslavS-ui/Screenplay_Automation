using Frontline.Tests.Core.Screenplay.Core;

namespace Frontline.Tests.Core.Screenplay.Infrastructure;

/// <summary>Registry of named actors for a test scenario; creates actors on first access.</summary>
public class ActorLibrary
{
    private readonly Dictionary<string, Actor> _actors = [];

    /// <summary>Returns the named actor, creating it if it doesn't exist.</summary>
    public Actor GetActor(string name)
    {
        if (!_actors.TryGetValue(name, out var actor))
        {
            actor = new Actor(name);
            _actors[name] = actor;
        }

        return actor;
    }

    public IEnumerable<Actor> GetAllActors() => _actors.Values;

    public bool RemoveActor(string name) => _actors.Remove(name);

    public void Clear() => _actors.Clear();

    public int Count => _actors.Count;
}
