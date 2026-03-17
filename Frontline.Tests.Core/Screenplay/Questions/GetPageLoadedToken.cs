using Frontline.Tests.Core.Screenplay.Configuration;
using Frontline.Tests.Core.Screenplay.Core;

namespace Frontline.Tests.Core.Screenplay.Questions;

/// <summary>Returns current data-pageloaded token. Capture before SPA navigation, pass to WaitForPageLoaded.</summary>
public class GetPageLoadedToken : IQuestion<string?>
{
    public async Task<string?> AnswerAsync(Actor actor)
    {
        var page = actor.UsesAbility<Abilities.BrowserAbility>().Page;
        return await page.Locator(AppConfiguration.BlazorPageContainerSelector)
            .GetAttributeAsync("data-pageloaded");
    }
}
