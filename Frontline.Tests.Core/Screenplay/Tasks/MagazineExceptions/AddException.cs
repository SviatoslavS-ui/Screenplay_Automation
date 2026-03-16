using Frontline.Tests.Core.Screenplay.Core;
using Frontline.Tests.Core.Screenplay.Interactions;
using Frontline.Tests.Core.Screenplay.Targets.MagazineExceptions;

namespace Frontline.Tests.Core.Screenplay.Tasks.MagazineExceptions;

/// <summary>
/// Fills and submits the Add Exception dialog.
/// The dialog uses progressive disclosure: company → magazine → reason → optional end date.
/// Ends when the dialog closes and the grid is ready.
/// </summary>
public class AddException(
    string company,
    string magazineSearch,
    string reason,
    string? endDate = null,
    string searchMode = "contains") : ITask
{
    public string Description =>
        $"Add exception: company='{company}', magazine='{magazineSearch}', reason='{reason}'";

    public async Task PerformAsync(Actor actor)
    {
        await actor.Performs(new Click(MagazineExceptionsPageTargets.AddExceptionButton));
        await actor.Performs(new WaitForElement(MagazineExceptionsPageTargets.AddCompanyInput));

        // Select company — reveals magazine section
        await actor.Performs(new Click(MagazineExceptionsPageTargets.AddCompanyInput));
        await actor.Performs(new WaitForElement(MagazineExceptionsPageTargets.EjPopupItem(company)));
        await actor.Performs(new ClickFirst(MagazineExceptionsPageTargets.EjPopupItem(company)));

        await actor.Performs(new WaitForElement(MagazineExceptionsPageTargets.AddMagazineInput));

        // Set search mode (Contains is default — only switch if Starts with requested)
        if (searchMode == "startswith")
            await actor.Performs(new Click(MagazineExceptionsPageTargets.AddStartsWithRadio));

        // Type magazine name and select from autocomplete suggestions — reveals reason section
        await actor.Performs(new Fill(MagazineExceptionsPageTargets.AddMagazineInput, magazineSearch));
        await actor.Performs(new WaitForElement(MagazineExceptionsPageTargets.EjPopupItem(magazineSearch)));
        await actor.Performs(new ClickFirst(MagazineExceptionsPageTargets.EjPopupItem(magazineSearch)));

        await actor.Performs(new WaitForElement(MagazineExceptionsPageTargets.AddReasonContainer));

        // Select reason — enables Save button
        await actor.Performs(new Click(MagazineExceptionsPageTargets.AddReasonContainer));
        await actor.Performs(new WaitForElement(MagazineExceptionsPageTargets.EjPopupItem(reason)));
        await actor.Performs(new ClickFirst(MagazineExceptionsPageTargets.EjPopupItem(reason)));

        // Optional end date
        if (endDate != null)
        {
            await actor.Performs(new Click(MagazineExceptionsPageTargets.AddEndDateCheckbox));
            await actor.Performs(new WaitForElement(MagazineExceptionsPageTargets.AddEndDateInput));
            await actor.Performs(new Fill(MagazineExceptionsPageTargets.AddEndDateInput, endDate));
        }

        await actor.Performs(new WaitForElement(MagazineExceptionsPageTargets.AddSaveButton));
        await actor.Performs(new Click(MagazineExceptionsPageTargets.AddSaveButton));
        await actor.Performs(new WaitForBlazorReady());
        await actor.Performs(new WaitForGridReady());
    }
}
