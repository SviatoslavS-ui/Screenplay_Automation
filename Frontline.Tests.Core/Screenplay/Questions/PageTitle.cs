using Frontline.Tests.Core.Screenplay.Core;

namespace Frontline.Tests.Core.Screenplay.Questions;

/// <summary>Returns the current browser page title.</summary>
public class PageTitle : IQuestion<string>
{
    public async Task<string> AnswerAsync(Actor actor)
    {
        var browserAbility = actor.UsesAbility<Abilities.BrowserAbility>();
        return await browserAbility.Page.TitleAsync();
    }
}