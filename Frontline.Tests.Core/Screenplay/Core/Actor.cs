namespace Frontline.Tests.Core.Screenplay.Core;

/// <summary>
/// Represents a test actor who performs tasks and asks questions.
/// An actor is the central character in the Screenplay pattern - they have abilities,
/// perform tasks, and ask questions about the application state.
/// Example: "Alice the customer" can log in, place orders, and check her balance.
/// </summary>
public class Actor
{
    private readonly Dictionary<string, IAbility> _abilities = [];
    private readonly string _name;

    public Actor(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        _name = name;
    }

    /// <summary>
    /// Gets the actor's name.
    /// </summary>
    public string Name => _name;

    /// <summary>
    /// Grants the actor a new ability.
    /// </summary>
    /// <param name="ability">The ability to grant.</param>
    /// <returns>This actor instance for chaining.</returns>
    /// <exception cref="InvalidOperationException">If the actor already has an ability with the same name.</exception>
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

    /// <summary>
    /// Retrieves one of the actor's abilities by name.
    /// </summary>
    /// <typeparam name="TAbility">The type of ability to retrieve.</typeparam>
    /// <param name="abilityName">The name of the ability.</param>
    /// <returns>The ability instance.</returns>
    /// <exception cref="InvalidOperationException">If the actor doesn't have the requested ability.</exception>
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

    /// <summary>
    /// Performs a task.
    /// </summary>
    /// <param name="task">The task to perform.</param>
    public async Task Performs(ITask task)
    {
        ArgumentNullException.ThrowIfNull(task);

        await task.PerformAsync(this);
    }

    /// <summary>
    /// Performs an interaction.
    /// </summary>
    /// <param name="interaction">The interaction to perform.</param>
    public async Task Performs(IInteraction interaction)
    {
        ArgumentNullException.ThrowIfNull(interaction);

        await interaction.PerformAsync(this);
    }

    /// <summary>
    /// Asks a question and retrieves the answer.
    /// </summary>
    /// <typeparam name="TAnswer">The type of answer expected.</typeparam>
    /// <param name="question">The question to ask.</param>
    /// <returns>The answer to the question.</returns>
    public async Task<TAnswer> Asks<TAnswer>(IQuestion<TAnswer> question)
    {
        ArgumentNullException.ThrowIfNull(question);

        return await question.AnswerAsync(this);
    }

    /// <summary>
    /// Reports a message from the actor, typically for logging or debugging.
    /// </summary>
    /// <param name="message">The message to report.</param>
    public void Report(string message)
    {
        Console.WriteLine($"[{Name}] {message}");
    }

    public override string ToString() => $"Actor '{_name}'";
}
