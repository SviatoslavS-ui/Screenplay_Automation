using Frontline.Tests.Core.Screenplay.Core;

namespace Frontline.Tests.Core.Screenplay.Questions;

/// <summary>Returns the text content of the matched element.</summary>
public class TextOf(string selector) : IQuestion<string?>
{
    public async Task<string?> AnswerAsync(Actor actor)
    {
        ArgumentNullException.ThrowIfNull(selector);
        
        var browserAbility = actor.UsesAbility<Abilities.BrowserAbility>("BrowserAbility");
        return await browserAbility.Page.TextContentAsync(selector);
    }
}
