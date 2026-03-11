using Frontline.Tests.Core.Screenplay.Core;

namespace Frontline.Tests.Core.Screenplay.Questions;

/// <summary>
/// Question: Is an element visible on the page?
/// </summary>
public class IsVisible(string selector) : IQuestion<bool>
{
    public async Task<bool> AnswerAsync(Actor actor)
    {
        ArgumentNullException.ThrowIfNull(selector);

        var browserAbility = actor.UsesAbility<Abilities.BrowserAbility>("BrowserAbility");

        try
        {
            return await browserAbility.Page.IsVisibleAsync(selector);
        }
        catch
        {
            return false;
        }
    }
}
