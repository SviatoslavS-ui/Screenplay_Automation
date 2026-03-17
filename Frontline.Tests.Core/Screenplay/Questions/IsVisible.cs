using Frontline.Tests.Core.Screenplay.Core;

namespace Frontline.Tests.Core.Screenplay.Questions;

/// <summary>Returns true if the matched element is currently visible.</summary>
public class IsVisible(string selector) : IQuestion<bool>
{
    public async Task<bool> AnswerAsync(Actor actor)
    {
        ArgumentNullException.ThrowIfNull(selector);

        var browserAbility = actor.UsesAbility<Abilities.BrowserAbility>();
        return await browserAbility.Page.Locator(selector).IsVisibleAsync();
    }
}
