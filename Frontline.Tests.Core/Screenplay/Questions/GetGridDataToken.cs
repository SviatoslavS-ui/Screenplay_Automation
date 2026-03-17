using Frontline.Tests.Core.Screenplay.Core;
using Frontline.Tests.Core.Screenplay.Targets.MagazineExceptions;

namespace Frontline.Tests.Core.Screenplay.Questions;

/// <summary>Returns current grid-dataloaded token. Capture before a grid-mutating action,
/// pass to WaitForGridDataLoaded.AfterAction to wait for the grid to re-render.</summary>
public class GetGridDataToken(string gridSelector = "#MagGrid") : IQuestion<string?>
{
    public async Task<string?> AnswerAsync(Actor actor)
    {
        var page = actor.UsesAbility<Abilities.BrowserAbility>().Page;
        return await page.Locator(gridSelector).GetAttributeAsync("grid-dataloaded");
    }
}
