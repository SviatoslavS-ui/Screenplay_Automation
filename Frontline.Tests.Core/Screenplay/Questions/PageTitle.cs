using Frontline.Tests.Core.Screenplay.Core;

namespace Frontline.Tests.Core.Screenplay.Questions;

/// <summary>
/// Question: What is the current page title?
/// </summary>
public class PageTitle : IQuestion<string>
{
    public async Task<string> AnswerAsync(Actor actor)
    {
        var browserAbility = actor.UsesAbility<Abilities.BrowserAbility>("BrowserAbility");
        return await browserAbility.Page.TitleAsync();
    }
}