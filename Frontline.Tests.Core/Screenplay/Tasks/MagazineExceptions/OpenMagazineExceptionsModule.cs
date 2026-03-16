using Frontline.Tests.Core.Screenplay.Core;
using Frontline.Tests.Core.Screenplay.Interactions;
using Frontline.Tests.Core.Screenplay.Questions;
using Frontline.Tests.Core.Screenplay.Targets.MagazineExceptions;

namespace Frontline.Tests.Core.Screenplay.Tasks.MagazineExceptions;

/// <summary>Opens the Magazine Exceptions grid via app tile, burger menu, and sidebar navigation.</summary>
public class OpenMagazineExceptionsModule : ITask
{
    public string Description => "Open Magazine Exceptions module";

    public async Task PerformAsync(Actor actor)
    {
        await actor.Performs(new Click(MagazineExceptionsPageTargets.MagazineExceptionsAppButton));
        await actor.Performs(WaitForPageLoaded.ForFullLoad());

        var tokenBeforeSpa = await actor.Asks(new GetPageLoadedToken());

        await actor.Performs(new WaitForElement(MagazineExceptionsPageTargets.BurgerMenuButton));
        await actor.Performs(ClickAriaToggle.ToExpand("dropdownbutton"));
        await actor.Performs(new WaitForElement(MagazineExceptionsPageTargets.MagazineExceptionsMenuItem));

        await actor.Performs(new Click(MagazineExceptionsPageTargets.MagazineExceptionsMenuItem));
        await actor.Performs(new WaitForElement(MagazineExceptionsPageTargets.ExceptionsTable));
        await actor.Performs(new WaitForElement(MagazineExceptionsPageTargets.ExceptionsContentTable));
        await actor.Performs(new WaitForBlazorReady());
        await actor.Performs(new WaitForGridReady());

        await actor.Performs(WaitForPageLoaded.ForSpaNavigation(tokenBeforeSpa!));
    }
}
