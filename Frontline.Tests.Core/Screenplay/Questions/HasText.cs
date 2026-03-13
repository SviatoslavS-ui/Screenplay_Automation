using Frontline.Tests.Core.Screenplay.Core;

namespace Frontline.Tests.Core.Screenplay.Questions;

/// <summary>Returns true if the matched element's trimmed text equals the expected value.</summary>
public class HasText(string selector, string expectedText) : IQuestion<bool>
{
    public async Task<bool> AnswerAsync(Actor actor)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(expectedText);
        
        var browserAbility = actor.UsesAbility<Abilities.BrowserAbility>("BrowserAbility");
        var actualText = await browserAbility.Page.TextContentAsync(selector);
        
        return actualText?.Trim() == expectedText.Trim();
    }
}