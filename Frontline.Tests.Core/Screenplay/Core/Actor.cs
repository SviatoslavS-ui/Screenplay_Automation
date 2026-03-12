namespace Frontline.Tests.Core.Screenplay.Core;

/// <summary>Screenplay actor: holds abilities, performs tasks, and answers questions.</summary>
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

    /// <summary>Grants an ability; returns this actor for chaining.</summary>
    /// <exception cref="InvalidOperationException">Thrown when an ability with the same name already exists.</exception>
    public Actor Can(IAbility ability)
    {
        ArgumentNullException.ThrowIfNull(ability);

        if (_abilities.ContainsKey(ability.AbilityName))
        {
            throw new InvalidOperationException(
                $"Actor '{_name}' already has ability '{ability.AbilityName}'.");
        }

        _abilities[ability.AbilityName] = ability;
        return this;
    }

    /// <summary>Returns the named ability; throws if absent or type-mismatched.</summary>
    /// <exception cref="ScreenplayException">Thrown when the ability is missing or the wrong type.</exception>
    public TAbility UsesAbility<TAbility>(string abilityName) where TAbility : IAbility
    {
        if (!_abilities.TryGetValue(abilityName, out var ability))
            throw new ScreenplayException($"Actor '{_name}' doesn't have ability '{abilityName}'.");

        if (ability is not TAbility typedAbility)
            throw new ScreenplayException($"Ability '{abilityName}' is not of type '{typeof(TAbility).Name}'.");

        return typedAbility;
    }

    /// <summary>
    /// Attempts to retrieve an ability without throwing if it doesn't exist.
    /// </summary>
    public bool TryGetAbility<TAbility>(string abilityName, out TAbility? ability) where TAbility : IAbility
    {
        ability = default;

        if (_abilities.TryGetValue(abilityName, out var foundAbility) && foundAbility is TAbility typedAbility)
        {
            ability = typedAbility;
            return true;
        }

        return false;
    }

    /// <summary>Executes a task.</summary>
    public async Task Performs(ITask task)
    {
        ArgumentNullException.ThrowIfNull(task);

        await task.PerformAsync(this);
    }

    /// <summary>Executes an interaction.</summary>
    public async Task Performs(IInteraction interaction)
    {
        ArgumentNullException.ThrowIfNull(interaction);

        await interaction.PerformAsync(this);
    }

    /// <summary>Answers a question and returns the result.</summary>
    public async Task<TAnswer> Asks<TAnswer>(IQuestion<TAnswer> question)
    {
        ArgumentNullException.ThrowIfNull(question);

        return await question.AnswerAsync(this);
    }

    public void Report(string message)
    {
        Console.WriteLine($"[{Name}] {message}");
    }

    public override string ToString() => $"Actor '{_name}'";
}
