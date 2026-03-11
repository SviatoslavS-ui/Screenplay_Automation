namespace Frontline.Tests.Core.Screenplay.Core;

/// <summary>
/// Represents a question that an Actor can ask to retrieve information from the application.
/// Questions are used to verify state, retrieve data, or make assertions in a more expressive way.
/// Example: "What is the text on the login button?" or "Is the success message visible?"
/// </summary>
public interface IQuestion<TAnswer>
{
    /// <summary>
    /// Asks the question and retrieves the answer using the actor's abilities.
    /// </summary>
    /// <param name="actor">The actor performing the question.</param>
    /// <returns>The answer to the question.</returns>
    Task<TAnswer> AnswerAsync(Actor actor);
}
