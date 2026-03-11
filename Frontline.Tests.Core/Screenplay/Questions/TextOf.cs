using Frontline.Tests.Core.Screenplay.Core;

namespace Frontline.Tests.Core.Screenplay.Questions;

/// <summary>
/// Question: What is the text content of an element?
/// </summary>
public class TextOf(string selector) : IQuestion<string?>
{
    public async Task<string?> AnswerAsync(Actor actor)
    {
        ArgumentNullException.ThrowIfNull(selector);
        
        var browserAbility = actor.UsesAbility<Abilities.BrowserAbility>("BrowserAbility");
        return await browserAbility.Page.TextContentAsync(selector);
    }
}
