namespace Frontline.Tests.Core.Screenplay.Core;

/// <summary>Screenplay actor: holds abilities, performs tasks/interactions, and answers questions.</summary>
public class Actor
{
    private readonly Dictionary<string, IAbility> _abilities = [];
    private readonly string _name;

    public Actor(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        _name = name;
    }

    public string Name => _name;

    /// <summary>Grants an ability; returns this actor for chaining. The ability's runtime type name is used as the key.</summary>
    /// <exception cref="InvalidOperationException">Thrown when an ability of the same type already exists.</exception>
    public Actor Can(IAbility ability)
    {
        ArgumentNullException.ThrowIfNull(ability);
        var key = ability.GetType().Name;
        if (_abilities.ContainsKey(key))
            throw new InvalidOperationException($"Actor '{_name}' already has ability '{key}'.");
        _abilities[key] = ability;
        return this;
    }

    /// <summary>Returns the ability by type; throws if absent.</summary>
    /// <exception cref="ScreenplayException">Thrown when the ability is missing.</exception>
    public TAbility UsesAbility<TAbility>() where TAbility : IAbility
    {
        var key = typeof(TAbility).Name;
        if (!_abilities.TryGetValue(key, out var ability))
            throw new ScreenplayException($"Actor '{_name}' doesn't have ability '{key}'.");
        return (TAbility)ability;
    }

    /// <summary>Returns false instead of throwing when the ability is absent or type-mismatched.</summary>
    public bool TryGetAbility<TAbility>(out TAbility? ability) where TAbility : IAbility
    {
        ability = default;
        var key = typeof(TAbility).Name;
        if (_abilities.TryGetValue(key, out var foundAbility) && foundAbility is TAbility typedAbility)
        {
            ability = typedAbility;
            return true;
        }
        return false;
    }

    /// <summary>Executes a task or interaction, logging its description to NUnit test output.</summary>
    public async Task Performs(IPerformable performable)
    {
        ArgumentNullException.ThrowIfNull(performable);
        TestContext.Out.WriteLine($"[{_name}] {performable.Description}");
        await performable.PerformAsync(this);
    }

    /// <summary>Answers a question and returns the result.</summary>
    public async Task<TAnswer> Asks<TAnswer>(IQuestion<TAnswer> question)
    {
        ArgumentNullException.ThrowIfNull(question);
        return await question.AnswerAsync(this);
    }

    public void Report(string message) => TestContext.Out.WriteLine($"[{Name}] {message}");

    public override string ToString() => $"Actor '{_name}'";
}
