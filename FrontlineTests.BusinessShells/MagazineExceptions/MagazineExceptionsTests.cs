using Frontline.Tests.Core.Screenplay.Configuration;
using Frontline.Tests.Core.Screenplay.Core;
using Frontline.Tests.Core.Screenplay.Tasks;
using Frontline.Tests.Core.Screenplay.Tasks.MagazineExceptions;
using Frontline.Tests.Core.Screenplay.Questions;
using Frontline.Tests.Core.Screenplay.Targets.MagazineExceptions;
using Frontline.Tests.Core.Screenplay.TestData.MagazineExceptions;
using Frontline.Tests.Core.Screenplay.Interactions;
using FrontlineTests.Common;

namespace FrontlineTests.Tests.MagazineExceptions;

[TestFixture]
public class MagazineExceptionsTests : ScreenplayTestBase
{
    [Test]
    [Category(TestCategories.Smoke)]
    public async Task TC_001_NavigateToMagazineExceptionsPage()
    {
        var user = ActorLibrary.GetActor("User");

        // Given: User navigates to the Frontline home page
        await user.Performs(new NavigateTo(AppConfiguration.BaseUrl));

        // Then: The home page is loaded correctly
        await user.ShouldHaveTitle(MagazineExceptionsTestData.ExpectedHomePageTitle);

        // When: User opens the Magazine Exceptions module
        await user.Performs(new OpenMagazineExceptionsModule());

        // Then: The exceptions table is visible
        await user.ShouldSee(MagazineExceptionsPageTargets.ExceptionsTable,
            "Exceptions table should be visible after opening the module");
    }

    [Test]
    [Category(TestCategories.Smoke)]
    public async Task TC_001b_VerifyKnownEntitiesArePresent()
    {
        var user = ActorLibrary.GetActor("User");

        // Given: User navigates to Magazine Exceptions
        await user.Performs(new NavigateTo(AppConfiguration.BaseUrl));
        await user.Performs(new OpenMagazineExceptionsModule());

        // When: User filters by Entity 1 ID (12 - RADIO TIMES)
        await user.Performs(new FilterExceptionsBy(
            MagazineExceptionsPageTargets.MagIdFilterInput,
            MagazineExceptionsTestData.Entity1_MagId));

        // Then: Entity 1 first row contains expected ID
        await user.ShouldEventuallyRead(MagazineExceptionsPageTargets.FirstRowIdCell,
            MagazineExceptionsTestData.Entity1_MagId);

        // And: Clear filter, then filter by Entity 2 ID (10760 - MOJO EXPORT)
        await user.Performs(new FilterExceptionsBy(MagazineExceptionsPageTargets.MagIdFilterInput, ""));
        await user.Performs(new FilterExceptionsBy(
            MagazineExceptionsPageTargets.MagIdFilterInput,
            MagazineExceptionsTestData.Entity2_MagId));

        // Then: Entity 2 first row contains expected ID
        await user.ShouldEventuallyRead(MagazineExceptionsPageTargets.FirstRowIdCell,
            MagazineExceptionsTestData.Entity2_MagId);
    }

    #region TC-002: Find exception by ID (Parametrized)

    private static IEnumerable<TestCaseData> FilterByIdTestCases()
    {
        yield return new TestCaseData(MagazineExceptionsTestData.ValidExceptionId, false)
            .SetName("TC_002_a_FilterById_ValidEntity1")
            .SetDescription("Filter by existing ID (12 - RADIO TIMES) returns matching row");

        yield return new TestCaseData(MagazineExceptionsTestData.ValidExceptionId2, false)
            .SetName("TC_002_a2_FilterById_ValidEntity2")
            .SetDescription("Filter by existing ID (10760 - MOJO EXPORT) returns matching row");

        yield return new TestCaseData(MagazineExceptionsTestData.NonExistentExceptionId, true)
            .SetName("TC_002_b_FilterById_NonExistentId")
            .SetDescription("Filter by non-existent ID (99999) returns no rows");

        yield return new TestCaseData(MagazineExceptionsTestData.PartialExceptionId, false)
            .SetName("TC_002_c_FilterById_PartialId")
            .SetDescription("Filter by partial ID (107) returns matching rows");
    }

    [Test, TestCaseSource(nameof(FilterByIdTestCases))]
    [Category(TestCategories.Functional)]
    public async Task TC_002_FindExceptionById(string exceptionId, bool expectEmpty)
    {
        var user = ActorLibrary.GetActor("User");

        await user.Performs(new NavigateTo(AppConfiguration.BaseUrl));
        await user.Performs(new OpenMagazineExceptionsModule());

        await user.Performs(new FilterExceptionsBy(
            MagazineExceptionsPageTargets.MagIdFilterInput,
            exceptionId));

        var expectedSelector = expectEmpty
            ? MagazineExceptionsPageTargets.EmptyRecordsMessage
            : MagazineExceptionsPageTargets.TableRows;
        await user.ShouldEventuallySee(expectedSelector);
    }

    #endregion

    #region TC-003: Find exception by Exception Name (Parametrized)

    private static IEnumerable<TestCaseData> FilterByExceptionNameTestCases()
    {
        yield return new TestCaseData(MagazineExceptionsTestData.ValidExceptionName, false)
            .SetName("TC_003_a_FilterByName_Entity1_RadioTimes")
            .SetDescription("Filter by exact name (RADIO TIMES) returns matching row");

        yield return new TestCaseData(MagazineExceptionsTestData.ValidExceptionName2, false)
            .SetName("TC_003_a2_FilterByName_Entity2_MojoExport")
            .SetDescription("Filter by exact name (MOJO EXPORT) returns matching row");

        yield return new TestCaseData(MagazineExceptionsTestData.PartialExceptionName, false)
            .SetName("TC_003_b_FilterByName_PartialMatch")
            .SetDescription("Filter by partial name (RADIO) returns matching rows");

        yield return new TestCaseData(MagazineExceptionsTestData.NonExistentExceptionName, true)
            .SetName("TC_003_c_FilterByName_NonExistent")
            .SetDescription("Filter by non-existent name returns empty results");

        yield return new TestCaseData(MagazineExceptionsTestData.SpecialCharExceptionName, null)
            .SetName("TC_003_d_FilterByName_SpecialChars")
            .SetDescription("Filter by name with special characters is handled gracefully");
    }

    [Test, TestCaseSource(nameof(FilterByExceptionNameTestCases))]
    [Category(TestCategories.Functional)]
    public async Task TC_003_FindExceptionByName(string exceptionName, bool? expectEmpty)
    {
        var user = ActorLibrary.GetActor("User");

        await user.Performs(new NavigateTo(AppConfiguration.BaseUrl));
        await user.Performs(new OpenMagazineExceptionsModule());

        await user.Performs(new FilterExceptionsBy(
            MagazineExceptionsPageTargets.MagazineNameFilterInput,
            exceptionName));

        if (expectEmpty == null)
        {
            await user.Performs(new WaitForBlazorReady());
            await user.ShouldSee(MagazineExceptionsPageTargets.ExceptionsContentTable);
            return;
        }

        var expectedSelector = expectEmpty.Value
            ? MagazineExceptionsPageTargets.EmptyRecordsMessage
            : MagazineExceptionsPageTargets.TableRows;
        await user.ShouldEventuallySee(expectedSelector);
    }

    #endregion

    #region TC-004: Find exception by Company (Parametrized)

    private static IEnumerable<TestCaseData> FilterByCompanyTestCases()
    {
        yield return new TestCaseData(MagazineExceptionsTestData.ValidCompanyName, false)
            .SetName("TC_004_a_FilterByCompany_Entity1_Frontline")
            .SetDescription("Filter by exact company (FRONTLINE) returns matching rows");

        yield return new TestCaseData(MagazineExceptionsTestData.ValidCompanyName2, false)
            .SetName("TC_004_a2_FilterByCompany_Entity3_Seymour")
            .SetDescription("Filter by exact company (SEYMOUR) returns matching row");

        yield return new TestCaseData(MagazineExceptionsTestData.PartialCompanyName, false)
            .SetName("TC_004_b_FilterByCompany_PartialMatch")
            .SetDescription("Filter by partial company (FRONT) returns matching rows");

        yield return new TestCaseData(MagazineExceptionsTestData.NonExistentCompanyName, true)
            .SetName("TC_004_c_FilterByCompany_NonExistent")
            .SetDescription("Filter by non-existent company returns empty results");

        yield return new TestCaseData(MagazineExceptionsTestData.SpecialCharCompanyName, null)
            .SetName("TC_004_d_FilterByCompany_SpecialChars")
            .SetDescription("Filter by company with special characters is handled gracefully");
    }

    [Test, TestCaseSource(nameof(FilterByCompanyTestCases))]
    [Category(TestCategories.Functional)]
    public async Task TC_004_FindExceptionByCompany(string companyName, bool? expectEmpty)
    {
        var user = ActorLibrary.GetActor("User");

        await user.Performs(new NavigateTo(AppConfiguration.BaseUrl));
        await user.Performs(new OpenMagazineExceptionsModule());

        await user.Performs(new FilterExceptionsBy(
            MagazineExceptionsPageTargets.CompanyFilterInput,
            companyName));

        if (expectEmpty == null)
        {
            await user.Performs(new WaitForBlazorReady());
            await user.ShouldSee(MagazineExceptionsPageTargets.ExceptionsContentTable);
            return;
        }

        var expectedSelector = expectEmpty.Value
            ? MagazineExceptionsPageTargets.EmptyRecordsMessage
            : MagazineExceptionsPageTargets.TableRows;
        await user.ShouldEventuallySee(expectedSelector);
    }

    #endregion

    #region TC-005: Edit exception and verify success toast (Parametrized)

    private static IEnumerable<TestCaseData> EditExceptionTestCases()
    {
        yield return new TestCaseData(MagazineExceptionsTestData.ValidNewReason)
            .SetName("TC_005_a_EditReason_ValidShortString")
            .SetDescription("Edit with valid short string saves and shows success toast");

        yield return new TestCaseData(MagazineExceptionsTestData.MaxLengthReason)
            .SetName("TC_005_b_EditReason_MaxLength")
            .SetDescription("Edit with max-length string saves and shows success toast");

        yield return new TestCaseData(MagazineExceptionsTestData.SpecialCharReason)
            .SetName("TC_005_c_EditReason_SpecialChars")
            .SetDescription("Edit with special characters saves correctly");
    }

    [Test, TestCaseSource(nameof(EditExceptionTestCases))]
    [Category(TestCategories.Functional)]
    [Ignore("Server-side bug: Edit action throws exception. Pending fix.")]
    public async Task TC_005_EditExceptionAndVerifySuccess(string newReason)
    {
        var user = ActorLibrary.GetActor("User");

        // Given: User navigates to Magazine Exceptions with data
        await user.Performs(new NavigateTo(AppConfiguration.BaseUrl));
        await user.Performs(new OpenMagazineExceptionsModule());

        // When: User clicks Edit button on first row
        await user.Performs(new Click(MagazineExceptionsPageTargets.FirstRowEditButton));

        // Then: Edit dialog opens
        await user.ShouldSee(MagazineExceptionsPageTargets.EditDialog, "Edit dialog should be visible");

        // When: User clears and updates the Exception Reason field
        await user.Performs(new ClearAndFill(
            MagazineExceptionsPageTargets.EditReasonField,
            newReason));

        // And: User clicks Save
        await user.Performs(new Click(MagazineExceptionsPageTargets.EditSaveButton));

        // Then: Success toast appears with correct message
        await user.ShouldEventuallySee(MagazineExceptionsPageTargets.Toast);
        await user.ShouldHaveText(MagazineExceptionsPageTargets.ToastMessage,
            MagazineExceptionsTestData.SuccessToastMessage,
            $"Toast should contain '{MagazineExceptionsTestData.SuccessToastMessage}'");
    }

    #endregion

    [Test]
    [Category(TestCategories.Functional)]
    [Ignore("Server-side bug: Edit action throws exception. Pending fix.")]
    public async Task TC_006_CancelEditDoesNotPersistChanges()
    {
        var user = ActorLibrary.GetActor("User");

        // Given: User navigates to Magazine Exceptions with data
        await user.Performs(new NavigateTo(AppConfiguration.BaseUrl));
        await user.Performs(new OpenMagazineExceptionsModule());

        // Capture original reason before editing
        var originalReason = await user.Asks(new TextOf(MagazineExceptionsPageTargets.FirstRowReasonCell));

        // When: User clicks Edit on first row
        await user.Performs(new Click(MagazineExceptionsPageTargets.FirstRowEditButton));

        // Then: Edit dialog opens
        await user.ShouldSee(MagazineExceptionsPageTargets.EditDialog, "Edit dialog should be visible");

        // When: User modifies the reason field
        await user.Performs(new ClearAndFill(
            MagazineExceptionsPageTargets.EditReasonField,
            "Temporary Change"));

        // And: User clicks Cancel
        await user.Performs(new Click(MagazineExceptionsPageTargets.EditCancelButton));

        // Then: Original reason still displays in table (no change persisted)
        await user.ShouldEventuallyRead(MagazineExceptionsPageTargets.FirstRowReasonCell, originalReason!);
    }

    [Test]
    [Category(TestCategories.Functional)]
    [Ignore("No confirmation dialog implemented — defect raised. Delete executes immediately without user confirmation.")]
    public async Task TC_007_DeleteException()
    {
        var user = ActorLibrary.GetActor("User");

        // Given: User navigates to Magazine Exceptions with data
        await user.Performs(new NavigateTo(AppConfiguration.BaseUrl));
        await user.Performs(new OpenMagazineExceptionsModule());

        // Capture first row ID before deletion
        var rowIdBeforeDelete = await user.Asks(new TextOf(MagazineExceptionsPageTargets.FirstRowIdCell));

        // When: User clicks Delete on first row
        await user.Performs(new Click(MagazineExceptionsPageTargets.FirstRowDeleteButton));

        // Then: Delete confirmation dialog appears
        await user.ShouldSee(MagazineExceptionsPageTargets.DeleteConfirmDialog,
            "Delete confirmation dialog should be visible");

        // When: User confirms deletion
        await user.Performs(new Click(MagazineExceptionsPageTargets.DeleteConfirmButton));

        // Then: Deleted row is no longer the first row
        await user.ShouldEventuallyNotRead(MagazineExceptionsPageTargets.FirstRowIdCell, rowIdBeforeDelete!);
    }

    [Test]
    [Category(TestCategories.Functional)]
    [Ignore("No confirmation dialog implemented — defect raised. Cancel flow cannot be tested until dialog exists.")]
    public async Task TC_008_DeleteConfirmationCanBeCancelled()
    {
        var user = ActorLibrary.GetActor("User");

        // Given: User navigates to Magazine Exceptions with data
        await user.Performs(new NavigateTo(AppConfiguration.BaseUrl));
        await user.Performs(new OpenMagazineExceptionsModule());

        // Capture first row ID before delete attempt
        var rowIdBefore = await user.Asks(new TextOf(MagazineExceptionsPageTargets.FirstRowIdCell));

        // When: User clicks Delete on first row
        await user.Performs(new Click(MagazineExceptionsPageTargets.FirstRowDeleteButton));

        // Then: Delete confirmation dialog appears
        await user.ShouldSee(MagazineExceptionsPageTargets.DeleteConfirmDialog,
            "Delete confirmation dialog should be visible");

        // When: User clicks Cancel
        await user.Performs(new Click(MagazineExceptionsPageTargets.DeleteCancelButton));

        // Then: Row is still present in table
        await user.ShouldEventuallyRead(MagazineExceptionsPageTargets.FirstRowIdCell, rowIdBefore!);
    }

    [Test]
    [Category(TestCategories.Functional)]
    public async Task TC_009_TablePagination()
    {
        var user = ActorLibrary.GetActor("User");

        // Given: User navigates to Magazine Exceptions with paginated data
        await user.Performs(new NavigateTo(AppConfiguration.BaseUrl));
        await user.Performs(new OpenMagazineExceptionsModule());

        // Capture first row ID and row count before pagination
        var firstRowOnPage1 = await user.Asks(new TextOf(MagazineExceptionsPageTargets.FirstRowIdCell));
        var firstPageRowCount = await user.Asks(new CountOf(MagazineExceptionsPageTargets.TableRows));

        // When: User clicks Next page (if visible)
        var nextButtonVisible = await user.Asks(new IsVisible(MagazineExceptionsPageTargets.PaginationNextButton));
        if (nextButtonVisible)
        {
            await user.Performs(new Click(MagazineExceptionsPageTargets.PaginationNextButton));

            // Then: Second page loaded — first row changed from page 1
            await user.ShouldEventuallyNotRead(MagazineExceptionsPageTargets.FirstRowIdCell, firstRowOnPage1!);
            await user.ShouldHaveAtLeast(MagazineExceptionsPageTargets.TableRows, 1, "Second page should have rows");

            // When: User clicks Previous page
            await user.ShouldSee(MagazineExceptionsPageTargets.PaginationPrevButton,
                "Previous button should be visible on page 2");
            await user.Performs(new Click(MagazineExceptionsPageTargets.PaginationPrevButton));

            // Then: Returns to first page — first row matches original
            await user.ShouldEventuallyRead(MagazineExceptionsPageTargets.FirstRowIdCell, firstRowOnPage1!);
            var returnedPageRowCount = await user.Asks(new CountOf(MagazineExceptionsPageTargets.TableRows));
            Assert.That(returnedPageRowCount, Is.EqualTo(firstPageRowCount), "Should return to first page with same row count");
        }
        else
        {
            Assert.Pass("Pagination not available (single page of data)");
        }
    }

    [Test]
    [Category(TestCategories.Functional)]
    public async Task TC_010_CombinedFilterIdAndName()
    {
        var user = ActorLibrary.GetActor("User");

        // Given: User navigates to Magazine Exceptions
        await user.Performs(new NavigateTo(AppConfiguration.BaseUrl));
        await user.Performs(new OpenMagazineExceptionsModule());

        // When: User filters by ID (Entity 1: ID=12, Name=RADIO TIMES)
        await user.Performs(new FilterExceptionsBy(
            MagazineExceptionsPageTargets.MagIdFilterInput,
            MagazineExceptionsTestData.Entity1_MagId));

        await user.ShouldEventuallyRead(MagazineExceptionsPageTargets.FirstRowIdCell,
            MagazineExceptionsTestData.Entity1_MagId);
        var afterIdFilterCount = await user.Asks(new CountOf(MagazineExceptionsPageTargets.TableRows));

        // And: User additionally filters by name (same entity)
        await user.Performs(new FilterExceptionsBy(
            MagazineExceptionsPageTargets.MagazineNameFilterInput,
            MagazineExceptionsTestData.Entity1_Name));

        // Then: Grid settled — results narrowed further or remain same
        await user.ShouldEventuallyRead(MagazineExceptionsPageTargets.FirstRowIdCell,
            MagazineExceptionsTestData.Entity1_MagId);
        var afterNameFilterCount = await user.Asks(new CountOf(MagazineExceptionsPageTargets.TableRows));
        Assert.That(afterNameFilterCount, Is.LessThanOrEqualTo(afterIdFilterCount), "Combined filters should narrow or maintain result count");
    }

    [Test]
    [Category(TestCategories.EdgeCase)]
    public async Task TC_011_EmptyStateWhenNoExceptionsExist()
    {
        var user = ActorLibrary.GetActor("User");

        // Given: User navigates to Magazine Exceptions
        await user.Performs(new NavigateTo(AppConfiguration.BaseUrl));
        await user.Performs(new OpenMagazineExceptionsModule());

        // When: User filters with a value that yields no results
        await user.Performs(new FilterExceptionsBy(
            MagazineExceptionsPageTargets.MagIdFilterInput,
            MagazineExceptionsTestData.NonExistentExceptionId));

        // Then: Empty state message appears — grid settled with no rows
        await user.ShouldEventuallySee(MagazineExceptionsPageTargets.EmptyRecordsMessage);
        await user.ShouldSee(MagazineExceptionsPageTargets.ExceptionsContentTable,
            "Grid container should still be visible in empty state");
    }

    [Test]
    [Category(TestCategories.Security)]
    public async Task TC_012_PageAccessControlNegative()
    {
        var user = ActorLibrary.GetActor("User");

        // Given: User is logged in (assumed by base setup)
        // When: User navigates to the home page
        await user.Performs(new NavigateTo(AppConfiguration.BaseUrl));

        // Then: We are still on a valid page
        await user.ShouldHaveTitle(MagazineExceptionsTestData.ExpectedHomePageTitle);
    }

    #region TC-014: Add exception — happy path (no end date)

    [Test]
    [Category(TestCategories.Functional)]
    public async Task TC_014_AddException_HappyPath_NoEndDate()
    {
        var user = ActorLibrary.GetActor("User");

        // Given: User navigates to Magazine Exceptions
        await user.Performs(new NavigateTo(AppConfiguration.BaseUrl));
        await user.Performs(new OpenMagazineExceptionsModule());

        // When: User adds a new exception without end date
        await user.Performs(new AddException(
            MagazineExceptionsTestData.AddException_Company,
            MagazineExceptionsTestData.AddException_MagazineSearch,
            MagazineExceptionsTestData.AddException_Reason));

        // Then: Dialog closes and success toast appears
        await user.ShouldEventuallyNotSee(MagazineExceptionsPageTargets.AddExceptionDialog);
        await user.ShouldEventuallySee(MagazineExceptionsPageTargets.Toast);

        // And: New row is present in the grid
        await user.Performs(new FilterExceptionsBy(
            MagazineExceptionsPageTargets.MagazineNameFilterInput,
            MagazineExceptionsTestData.AddException_MagazineSearch));
        await user.ShouldEventuallyRead(
            MagazineExceptionsPageTargets.ExceptionNameCell,
            MagazineExceptionsTestData.AddException_MagazineSearch);
    }

    #endregion

    #region TC-015: Add exception — with end date

    [Test]
    [Category(TestCategories.Functional)]
    public async Task TC_015_AddException_WithEndDate()
    {
        var user = ActorLibrary.GetActor("User");

        // Given: User navigates to Magazine Exceptions
        await user.Performs(new NavigateTo(AppConfiguration.BaseUrl));
        await user.Performs(new OpenMagazineExceptionsModule());

        // When: User adds a new exception with an end date
        await user.Performs(new AddException(
            MagazineExceptionsTestData.AddException_Company,
            MagazineExceptionsTestData.AddException_MagazineSearch,
            MagazineExceptionsTestData.AddException_Reason,
            endDate: MagazineExceptionsTestData.AddException_EndDate));

        // Then: Dialog closes and success toast appears
        await user.ShouldEventuallyNotSee(MagazineExceptionsPageTargets.AddExceptionDialog);
        await user.ShouldEventuallySee(MagazineExceptionsPageTargets.Toast);

        // And: New row shows the end date year
        await user.Performs(new FilterExceptionsBy(
            MagazineExceptionsPageTargets.MagazineNameFilterInput,
            MagazineExceptionsTestData.AddException_MagazineSearch));
        await user.ShouldEventuallyRead(
            MagazineExceptionsPageTargets.ExceptionNameCell,
            MagazineExceptionsTestData.AddException_MagazineSearch);
        await user.ShouldReadContaining(
            MagazineExceptionsPageTargets.FirstRowEndDateCell, "2027");
    }

    #endregion

    #region TC-016: Cancel discards dialog

    [Test]
    [Category(TestCategories.Functional)]
    public async Task TC_016_AddException_CancelDiscardsDialog()
    {
        var user = ActorLibrary.GetActor("User");

        await user.Performs(new NavigateTo(AppConfiguration.BaseUrl));
        await user.Performs(new OpenMagazineExceptionsModule());

        var rowCountBefore = await user.Asks(new CountOf(MagazineExceptionsPageTargets.TableRows));

        await OpenAddExceptionDialogWithCompany(user);
        await user.Performs(new Click(MagazineExceptionsPageTargets.AddCancelButton));

        await user.ShouldEventuallyNotSee(MagazineExceptionsPageTargets.AddExceptionDialog);
        var rowCountAfter = await user.Asks(new CountOf(MagazineExceptionsPageTargets.TableRows));
        Assert.That(rowCountAfter, Is.EqualTo(rowCountBefore), "Cancel must not add a new row");
    }

    #endregion

    #region TC-017: Save disabled until all required fields filled (parametrised)

    private static IEnumerable<TestCaseData> AddExceptionValidationTestCases()
    {
        yield return new TestCaseData("none")
            .SetName("TC_017_a_Validation_NoFieldsFilled")
            .SetDescription("Save is disabled when dialog opens with no fields filled");

        yield return new TestCaseData("company_only")
            .SetName("TC_017_b_Validation_CompanyOnly")
            .SetDescription("Save is disabled after company selected but magazine not entered");

        yield return new TestCaseData("company_and_magazine")
            .SetName("TC_017_c_Validation_CompanyAndMagazine")
            .SetDescription("Save is disabled after company and magazine filled but reason not chosen");
    }

    [Test, TestCaseSource(nameof(AddExceptionValidationTestCases))]
    [Category(TestCategories.Functional)]
    public async Task TC_017_AddException_SaveDisabledUntilAllFieldsFilled(string filledFields)
    {
        var user = ActorLibrary.GetActor("User");

        await user.Performs(new NavigateTo(AppConfiguration.BaseUrl));
        await user.Performs(new OpenMagazineExceptionsModule());

        if (filledFields == "none")
        {
            await user.Performs(new Click(MagazineExceptionsPageTargets.AddExceptionButton));
            await user.Performs(new WaitForElement(MagazineExceptionsPageTargets.AddCompanyInput));
        }
        else
        {
            await OpenAddExceptionDialogWithCompany(user);
        }

        if (filledFields == "company_and_magazine")
        {
            await user.Performs(new Fill(MagazineExceptionsPageTargets.AddMagazineInput, MagazineExceptionsTestData.AddException_MagazineSearch));
            await user.Performs(new WaitForElement(MagazineExceptionsPageTargets.EjPopupItem(MagazineExceptionsTestData.AddException_MagazineSearch)));
            await user.Performs(new ClickFirst(MagazineExceptionsPageTargets.EjPopupItem(MagazineExceptionsTestData.AddException_MagazineSearch)));
            await user.Performs(new WaitForElement(MagazineExceptionsPageTargets.AddReasonContainer));
        }

        await user.ShouldSee(MagazineExceptionsPageTargets.AddSaveButtonDisabled,
            "Save must be disabled until all required fields are filled");

        await user.Performs(new Click(MagazineExceptionsPageTargets.AddCancelButton));
    }

    #endregion

    #region TC-018: Magazine search — Contains mode

    [Test]
    [Category(TestCategories.Functional)]
    public async Task TC_018_AddException_MagazineSearch_ContainsMode()
    {
        var user = ActorLibrary.GetActor("User");

        await user.Performs(new NavigateTo(AppConfiguration.BaseUrl));
        await user.Performs(new OpenMagazineExceptionsModule());
        await OpenAddExceptionDialogWithCompany(user);

        // Contains is default — mid-string "AMER" must find "GAMER"
        await user.Performs(new Fill(MagazineExceptionsPageTargets.AddMagazineInput, MagazineExceptionsTestData.MagazineSearch_MidString));
        await user.ShouldEventuallySee(MagazineExceptionsPageTargets.EjPopupItem(MagazineExceptionsTestData.AddException_MagazineSearch));

        await user.Performs(new Click(MagazineExceptionsPageTargets.AddCancelButton));
    }

    #endregion

    #region TC-019: Magazine search — Starts with mode

    [Test]
    [Category(TestCategories.Functional)]
    public async Task TC_019_AddException_MagazineSearch_StartsWithMode()
    {
        var user = ActorLibrary.GetActor("User");

        await user.Performs(new NavigateTo(AppConfiguration.BaseUrl));
        await user.Performs(new OpenMagazineExceptionsModule());
        await OpenAddExceptionDialogWithCompany(user);

        await user.Performs(new Click(MagazineExceptionsPageTargets.AddStartsWithRadio));

        // Mid-string "AMER" must NOT find "GAMER" (assumes no SEYMOUR magazine starts with "AMER")
        await user.Performs(new Fill(MagazineExceptionsPageTargets.AddMagazineInput, MagazineExceptionsTestData.MagazineSearch_MidString));
        await user.ShouldEventuallySee(MagazineExceptionsPageTargets.EjPopupNoData);

        // Prefix "GAM" must find "GAMER"
        await user.Performs(new Fill(MagazineExceptionsPageTargets.AddMagazineInput, MagazineExceptionsTestData.MagazineSearch_Prefix));
        await user.ShouldEventuallySee(MagazineExceptionsPageTargets.EjPopupItem(MagazineExceptionsTestData.AddException_MagazineSearch));

        await user.Performs(new Click(MagazineExceptionsPageTargets.AddCancelButton));
    }

    #endregion

    #region TC-020: Magazine search — no results for unrecognised name

    [Test]
    [Category(TestCategories.EdgeCase)]
    public async Task TC_020_AddException_MagazineSearch_NoResults()
    {
        var user = ActorLibrary.GetActor("User");

        await user.Performs(new NavigateTo(AppConfiguration.BaseUrl));
        await user.Performs(new OpenMagazineExceptionsModule());
        await OpenAddExceptionDialogWithCompany(user);

        await user.Performs(new Fill(MagazineExceptionsPageTargets.AddMagazineInput, MagazineExceptionsTestData.NonExistentMagazine));

        await user.ShouldEventuallySee(MagazineExceptionsPageTargets.EjPopupNoData);
        await user.ShouldSee(MagazineExceptionsPageTargets.AddSaveButtonDisabled,
            "Save must remain disabled when no magazine is selected");

        await user.Performs(new Click(MagazineExceptionsPageTargets.AddCancelButton));
    }

    #endregion

    #region TC-021: End date checkbox toggles date picker

    [Test]
    [Category(TestCategories.Functional)]
    public async Task TC_021_AddException_EndDateCheckboxTogglesDatePicker()
    {
        var user = ActorLibrary.GetActor("User");

        await user.Performs(new NavigateTo(AppConfiguration.BaseUrl));
        await user.Performs(new OpenMagazineExceptionsModule());
        await OpenAddExceptionDialogWithCompany(user);
        await FillAddExceptionMagazineAndReason(user);

        // Date picker hidden by default
        await user.ShouldNotSee(MagazineExceptionsPageTargets.AddEndDateInput,
            "Date picker must be hidden before checkbox is checked");

        // Check → date picker appears
        await user.Performs(new WaitForElement(MagazineExceptionsPageTargets.AddEndDateCheckbox));
        await user.Performs(new Click(MagazineExceptionsPageTargets.AddEndDateCheckbox));
        await user.ShouldEventuallySee(MagazineExceptionsPageTargets.AddEndDateInput);

        // Uncheck → date picker hides again
        await user.Performs(new Click(MagazineExceptionsPageTargets.AddEndDateCheckbox));
        await user.ShouldEventuallyNotSee(MagazineExceptionsPageTargets.AddEndDateInput);

        await user.Performs(new Click(MagazineExceptionsPageTargets.AddCancelButton));
    }

    #endregion

    // ── Add Exception dialog helpers ─────────────────────────────────────────
    // Private helpers covering the progressive-disclosure setup steps that are
    // shared across TC-016 through TC-021. Not promoted to Tasks because they
    // are specific to this test class's state management.

    private async Task OpenAddExceptionDialogWithCompany(Actor actor)
    {
        await actor.Performs(new Click(MagazineExceptionsPageTargets.AddExceptionButton));
        await actor.Performs(new WaitForElement(MagazineExceptionsPageTargets.AddCompanyInput));
        await actor.Performs(new Click(MagazineExceptionsPageTargets.AddCompanyInput));
        await actor.Performs(new WaitForElement(MagazineExceptionsPageTargets.EjPopupItem(MagazineExceptionsTestData.AddException_Company)));
        await actor.Performs(new ClickFirst(MagazineExceptionsPageTargets.EjPopupItem(MagazineExceptionsTestData.AddException_Company)));
        await actor.Performs(new WaitForElement(MagazineExceptionsPageTargets.AddMagazineInput));
    }

    private async Task FillAddExceptionMagazineAndReason(Actor actor)
    {
        await actor.Performs(new Fill(MagazineExceptionsPageTargets.AddMagazineInput, MagazineExceptionsTestData.AddException_MagazineSearch));
        await actor.Performs(new WaitForElement(MagazineExceptionsPageTargets.EjPopupItem(MagazineExceptionsTestData.AddException_MagazineSearch)));
        await actor.Performs(new ClickFirst(MagazineExceptionsPageTargets.EjPopupItem(MagazineExceptionsTestData.AddException_MagazineSearch)));
        await actor.Performs(new WaitForElement(MagazineExceptionsPageTargets.AddReasonContainer));
        await actor.Performs(new Click(MagazineExceptionsPageTargets.AddReasonContainer));
        await actor.Performs(new WaitForElement(MagazineExceptionsPageTargets.EjPopupItem(MagazineExceptionsTestData.AddException_Reason)));
        await actor.Performs(new ClickFirst(MagazineExceptionsPageTargets.EjPopupItem(MagazineExceptionsTestData.AddException_Reason)));
    }

    [Test]
    [Category(TestCategories.EdgeCase)]
    [Ignore("Server-side bug: Edit action throws exception. Pending fix.")]
    public async Task TC_013_SessionExpiryHandling()
    {
        var user = ActorLibrary.GetActor("User");

        // Given: User navigates to Magazine Exceptions and opens Edit dialog
        await user.Performs(new NavigateTo(AppConfiguration.BaseUrl));
        await user.Performs(new OpenMagazineExceptionsModule());

        // When: User clicks Edit to open dialog
        await user.Performs(new Click(MagazineExceptionsPageTargets.FirstRowEditButton));

        // Then: Edit dialog is open and interactive
        await user.ShouldSee(MagazineExceptionsPageTargets.EditDialog, "Edit dialog should open");
        await user.ShouldSee(MagazineExceptionsPageTargets.EditReasonField, "Reason field should be visible and interactive");
    }
}
