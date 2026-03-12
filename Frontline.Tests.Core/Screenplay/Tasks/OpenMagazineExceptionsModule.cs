using Frontline.Tests.Core.Screenplay.Core;
using Frontline.Tests.Core.Screenplay.Interactions;
using Frontline.Tests.Core.Screenplay.Targets;

namespace Frontline.Tests.Core.Screenplay.Tasks;

/// <summary>
/// Composite task: Open the Magazine Exceptions grid from the Frontline home page.
/// Full flow:
///   1. Click the Magazine Exceptions app tile
///   2. Wait for the app to load (burger menu becomes visible)
///   3. Click the burger menu to open navigation
///   4. Wait for the Magazine Exceptions menu item
///   5. Click the Magazine Exceptions menu item
///   6. Wait for the grid header and content tables to render
/// Precondition: Actor must already be on the Frontline home page.
/// </summary>
public class OpenMagazineExceptionsModule : ITask
{
    public string TaskDescription => "Open Magazine Exceptions module";

    public async Task PerformAsync(Actor actor)
    {
        await new Click(MagazineExceptionsPageTargets.MagazineExceptionsAppButton).PerformAsync(actor);
        await new WaitForElement(MagazineExceptionsPageTargets.BurgerMenuButton).PerformAsync(actor);

        await new Click(MagazineExceptionsPageTargets.BurgerMenuButton).PerformAsync(actor);
        await new WaitForElement(MagazineExceptionsPageTargets.MagazineExceptionsMenuItem).PerformAsync(actor);

        await new Click(MagazineExceptionsPageTargets.MagazineExceptionsMenuItem).PerformAsync(actor);
        await new WaitForElement(MagazineExceptionsPageTargets.ExceptionsTable).PerformAsync(actor);
        await new WaitForElement(MagazineExceptionsPageTargets.ExceptionsContentTable).PerformAsync(actor);
    }
}
