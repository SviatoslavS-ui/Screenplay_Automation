using Frontline.Tests.Core.Screenplay.Core;

namespace Frontline.Tests.Core.Screenplay.Questions;

/// <summary>Returns the count of elements matching the selector.</summary>
public class CountOf(string selector) : IQuestion<int>
{
    public async Task<int> AnswerAsync(Actor actor)
    {
        ArgumentNullException.ThrowIfNull(selector);

        var browserAbility = actor.UsesAbility<Abilities.BrowserAbility>();
        return await browserAbility.Page.Locator(selector).CountAsync();
    }
}
