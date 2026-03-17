namespace Frontline.Tests.Core.Screenplay.Core;

/// <summary>Question an actor asks to read application state or verify data.</summary>
public interface IQuestion<TAnswer>
{
    /// <summary>Resolves the question using the actor's abilities and returns the answer.</summary>
    Task<TAnswer> AnswerAsync(Actor actor);
}
