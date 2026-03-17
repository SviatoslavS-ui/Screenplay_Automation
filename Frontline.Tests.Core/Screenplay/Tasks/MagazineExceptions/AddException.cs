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
        var companyItem = MagazineExceptionsPageTargets.EjPopupItem(company);
        await actor.Performs(new Click(MagazineExceptionsPageTargets.AddCompanyInput));
        await actor.Performs(new WaitForElement(companyItem));
        await actor.Performs(new ClickFirst(companyItem));

        await actor.Performs(new WaitForElement(MagazineExceptionsPageTargets.AddMagazineInput));

        // Set search mode (Contains is default — only switch if Starts with requested)
        if (searchMode == "startswith")
            await actor.Performs(new Click(MagazineExceptionsPageTargets.AddStartsWithRadio));

        // Type magazine name and select from autocomplete suggestions — reveals reason section
        var magazineItem = MagazineExceptionsPageTargets.MagSearchPopupItem(magazineSearch);
        await actor.Performs(new Click(MagazineExceptionsPageTargets.AddMagazineInput));
        await actor.Performs(new TypeText(MagazineExceptionsPageTargets.AddMagazineInput, magazineSearch));
        await actor.Performs(new WaitForElement(magazineItem, timeoutMs: 10_000));
        await actor.Performs(new ClickFirst(magazineItem));

        await actor.Performs(new WaitForElement(MagazineExceptionsPageTargets.AddReasonContainer));

        // Select reason
        var reasonItem = MagazineExceptionsPageTargets.EjPopupItem(reason);
        await actor.Performs(new Click(MagazineExceptionsPageTargets.AddReasonContainer));
        await actor.Performs(new WaitForElement(reasonItem));
        await actor.Performs(new ClickFirst(reasonItem));

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
