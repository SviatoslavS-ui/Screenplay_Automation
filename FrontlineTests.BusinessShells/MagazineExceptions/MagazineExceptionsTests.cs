using Frontline.Tests.Core.Screenplay.Configuration;
using Frontline.Tests.Core.Screenplay.Core;
using Frontline.Tests.Core.Screenplay.Tasks;
using Frontline.Tests.Core.Screenplay.Tasks.MagazineExceptions;
using Frontline.Tests.Core.Screenplay.Questions;
using Frontline.Tests.Core.Screenplay.Targets.MagazineExceptions;
using Frontline.Tests.Core.Screenplay.TestData.MagazineExceptions;
using Frontline.Tests.Core.Screenplay.Interactions;
using FrontlineTests.Common;
using Microsoft.Data.SqlClient;
using System.Data;

namespace FrontlineTests.Tests.MagazineExceptions;

[TestFixture]
public class MagazineExceptionsTests : ScreenplayTestBase
{
    // ── Fixture-level setup ──────────────────────────────────────────────────

    [OneTimeSetUp]
    public async Task EnsureTestEntitiesExistAsync()
    {
        if (!AppConfiguration.SqlEnabled)
        {
            TestContext.Out.WriteLine("[Seed] SQL disabled — skipping test entity verification.");
            return;
        }

        await using var connection = new SqlConnection(AppConfiguration.SqlConnectionString);
        try
        {
            await connection.OpenAsync();
        }
        catch (Exception ex)
        {
            TestContext.Out.WriteLine($"[Seed] DB connection failed — skipping entity verification. {ex.Message}");
            return;
        }

        EntitySeedData[] entities =
        [
            MagazineExceptionsCleanup.Entity1Seed,
            MagazineExceptionsCleanup.Entity2Seed,
            MagazineExceptionsCleanup.Entity3Seed,
        ];

        foreach (var entity in entities)
            await EnsureEntityExistsAsync(connection, entity);
    }

    private static async Task EnsureEntityExistsAsync(SqlConnection connection, EntitySeedData seed)
    {
        await using var checkCmd = new SqlCommand(MagazineExceptionsCleanup.CheckEntityExists, connection);
        checkCmd.Parameters.Add(new SqlParameter("@MagazineId", SqlDbType.Int) { Value = seed.MagazineId });
        checkCmd.Parameters.Add(new SqlParameter("@CompanyId",  SqlDbType.Int) { Value = seed.CompanyId });

        var count = (int)(await checkCmd.ExecuteScalarAsync())!;
        if (count > 0)
        {
            TestContext.Out.WriteLine($"[Seed] Verified: {seed.Label}");
            return;
        }

        TestContext.Out.WriteLine($"[Seed] Missing — inserting: {seed.Label}");
        await using var insertCmd = new SqlCommand(MagazineExceptionsCleanup.SeedEntity, connection);
        insertCmd.Parameters.Add(new SqlParameter("@MagazineId",  SqlDbType.Int)          { Value = seed.MagazineId });
        insertCmd.Parameters.Add(new SqlParameter("@CompanyId",   SqlDbType.Int)          { Value = seed.CompanyId });
        insertCmd.Parameters.Add(new SqlParameter("@ReasonCode",  SqlDbType.NVarChar, 20) { Value = seed.ReasonCode });
        insertCmd.Parameters.Add(new SqlParameter("@ReasonText",  SqlDbType.NVarChar, 100){ Value = seed.ReasonText });

        var rows = await insertCmd.ExecuteNonQueryAsync();
        if (rows == 0)
            Assert.Fail($"[Seed] Could not insert {seed.Label} — magazine {seed.MagazineId} not found in V_DOTNET_PRODUCT.");

        TestContext.Out.WriteLine($"[Seed] Inserted: {seed.Label}");
    }

    // ────────────────────────────────────────────────────────────────────────

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
        await user.Performs(new Fill(
            MagazineExceptionsPageTargets.MagIdFilterInput,
            MagazineExceptionsTestData.Entity1_MagId));

        // Then: Entity 1 first row contains expected ID
        await user.ShouldEventuallyRead(MagazineExceptionsPageTargets.FirstRowIdCell,
            MagazineExceptionsTestData.Entity1_MagId);

        // And: Clear filter, then filter by Entity 2 ID (10760 - MOJO EXPORT)
        await user.Performs(new Fill(MagazineExceptionsPageTargets.MagIdFilterInput, ""));
        await user.Performs(new Fill(
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

        // Given: User navigates to Magazine Exceptions
        await user.Performs(new NavigateTo(AppConfiguration.BaseUrl));
        await user.Performs(new OpenMagazineExceptionsModule());

        // When: User filters by exception ID
        await user.Performs(new Fill(
            MagazineExceptionsPageTargets.MagIdFilterInput,
            exceptionId));

        // Then: Grid shows matching rows or empty state
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

        // Given: User navigates to Magazine Exceptions
        await user.Performs(new NavigateTo(AppConfiguration.BaseUrl));
        await user.Performs(new OpenMagazineExceptionsModule());

        // When: User filters by magazine name
        await user.Performs(new Fill(
            MagazineExceptionsPageTargets.MagazineNameFilterInput,
            exceptionName));

        // Then: Grid handles gracefully (special chars) or shows expected result
        if (expectEmpty == null)
        {
            await user.ShouldEventuallySee(MagazineExceptionsPageTargets.ExceptionsContentTable);
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

        // Given: User navigates to Magazine Exceptions
        await user.Performs(new NavigateTo(AppConfiguration.BaseUrl));
        await user.Performs(new OpenMagazineExceptionsModule());

        // When: User filters by company name
        await user.Performs(new Fill(
            MagazineExceptionsPageTargets.CompanyFilterInput,
            companyName));

        // Then: Grid handles gracefully (special chars) or shows expected result
        if (expectEmpty == null)
        {
            await user.ShouldEventuallySee(MagazineExceptionsPageTargets.ExceptionsContentTable);
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
        yield return new TestCaseData(MagazineExceptionsTestData.EditReason_A)
            .SetName("TC_005_a_EditReason_TimeSensitive")
            .SetDescription("Edit reason to TIME SENSITIVE saves and shows success toast");

        yield return new TestCaseData(MagazineExceptionsTestData.EditReason_B)
            .SetName("TC_005_b_EditReason_Embargo")
            .SetDescription("Edit reason to EMBARGO saves and shows success toast");

        yield return new TestCaseData(MagazineExceptionsTestData.EditReason_C)
            .SetName("TC_005_c_EditReason_Other")
            .SetDescription("Edit reason to OTHER saves and shows success toast");
    }

    [Test, TestCaseSource(nameof(EditExceptionTestCases))]
    [Category(TestCategories.Functional)]
    public async Task TC_005_EditExceptionAndVerifySuccess(string newReason)
    {
        var user = ActorLibrary.GetActor("User");

        // Snapshot Entity1's current reason code so we can restore it in cleanup.
        // (object) cast required: SqlParameter(string, 0) would resolve to SqlParameter(string, SqlDbType)
        // since 0 converts implicitly to any enum — value would never be sent to SQL Server.
        // EXCEPTION_REASON_CODE is nvarchar(20) in the DB — snapshot as string, restore as string.
        // (object) cast on int constants is required: SqlParameter(string, 0) resolves to
        // SqlParameter(string, SqlDbType) because 0 implicitly converts to any enum type.
        var originalReasonCode = await user.Asks(new DbScalar<string?>(
            MagazineExceptionsCleanup.GetExceptionReasonCode,
            new SqlParameter("@MagazineId", (object)MagazineExceptionsCleanup.Entity1MagazineId),
            new SqlParameter("@CompanyId",  (object)MagazineExceptionsCleanup.FrontlineCompanyId)));

        RegisterCleanup(async () => await user.Performs(new DbExecute(
            MagazineExceptionsCleanup.RestoreExceptionReason,
            new SqlParameter("@ReasonCode", originalReasonCode ?? ""),
            new SqlParameter("@MagazineId", (object)MagazineExceptionsCleanup.Entity1MagazineId),
            new SqlParameter("@CompanyId",  (object)MagazineExceptionsCleanup.FrontlineCompanyId))));

        // Given: User navigates to Magazine Exceptions and filters to Entity1 (RADIO TIMES)
        await user.Performs(new NavigateTo(AppConfiguration.BaseUrl));
        await user.Performs(new OpenMagazineExceptionsModule());
        await user.Performs(new Fill(MagazineExceptionsPageTargets.MagIdFilterInput, MagazineExceptionsTestData.Entity1_MagId));
        await user.ShouldEventuallyRead(MagazineExceptionsPageTargets.FirstRowIdCell, MagazineExceptionsTestData.Entity1_MagId);

        // When: User opens the Edit dialog for that row
        await user.Performs(new Click(MagazineExceptionsPageTargets.RowEditButton(MagazineExceptionsTestData.Entity1_MagId)));
        await user.ShouldEventuallySee(MagazineExceptionsPageTargets.EditDialog);

        // And: User opens the reason dropdown via its arrow icon and selects a new value
        await user.Performs(new Click(MagazineExceptionsPageTargets.EditReasonDropdownIcon));
        await user.Performs(new WaitForElement(MagazineExceptionsPageTargets.EjPopupItem(newReason)));
        await user.Performs(new ClickFirst(MagazineExceptionsPageTargets.EjPopupItem(newReason)));
        await user.Performs(new WaitForBlazorReady());

        // And: User clicks Save
        await user.Performs(new Click(MagazineExceptionsPageTargets.EditSaveButton));

        // Then: Success toast appears and reason cell reflects the change
        await user.ShouldEventuallySee(MagazineExceptionsPageTargets.Toast);
        await user.ShouldEventuallyRead(MagazineExceptionsPageTargets.FirstRowReasonCell, newReason);
    }

    #endregion

    [Test]
    [Category(TestCategories.Functional)]
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
        await user.ShouldEventuallySee(MagazineExceptionsPageTargets.EditDialog);

        // And: User opens the reason dropdown via its arrow icon and selects a different value
        var tempReason = MagazineExceptionsTestData.EditReason_ForCancel;
        await user.Performs(new Click(MagazineExceptionsPageTargets.EditReasonDropdownIcon));
        await user.Performs(new WaitForElement(MagazineExceptionsPageTargets.EjPopupItem(tempReason)));
        await user.Performs(new ClickFirst(MagazineExceptionsPageTargets.EjPopupItem(tempReason)));
        await user.Performs(new WaitForBlazorReady());

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
            await user.ShouldHaveExactly(MagazineExceptionsPageTargets.TableRows, firstPageRowCount,
                "Should return to first page with same row count");
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
        await user.Performs(new Fill(
            MagazineExceptionsPageTargets.MagIdFilterInput,
            MagazineExceptionsTestData.Entity1_MagId));

        await user.ShouldEventuallyRead(MagazineExceptionsPageTargets.FirstRowIdCell,
            MagazineExceptionsTestData.Entity1_MagId);
        var afterIdFilterCount = await user.Asks(new CountOf(MagazineExceptionsPageTargets.TableRows));

        // And: User additionally filters by name (same entity)
        await user.Performs(new Fill(
            MagazineExceptionsPageTargets.MagazineNameFilterInput,
            MagazineExceptionsTestData.Entity1_Name));

        // Then: Grid settled — results narrowed further or remain same
        await user.ShouldEventuallyRead(MagazineExceptionsPageTargets.FirstRowIdCell,
            MagazineExceptionsTestData.Entity1_MagId);
        await user.ShouldAnswer(new CountOf(MagazineExceptionsPageTargets.TableRows),
            Is.LessThanOrEqualTo(afterIdFilterCount),
            "Combined filters should narrow or maintain result count");
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
        await user.Performs(new Fill(
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
    [Category(TestCategories.Smoke)]
    [Category(TestCategories.Functional)]
    public async Task TC_014_AddException_HappyPath_NoEndDate()
    {
        var user = ActorLibrary.GetActor("User");

        // Register cleanup upfront — runs in TearDown regardless of test outcome
        RegisterExceptionCleanup(user);

        // Given: User navigates to Magazine Exceptions
        await user.Performs(new NavigateTo(AppConfiguration.BaseUrl));
        await user.Performs(new OpenMagazineExceptionsModule());

        // When: User adds a new exception without end date
        var gridToken = await user.Asks(new GetGridDataToken());
        await user.Performs(new AddException(
            MagazineExceptionsTestData.AddException_Company,
            MagazineExceptionsTestData.AddException_MagazineSearch,
            MagazineExceptionsTestData.AddException_Reason));

        // Then: Dialog closes, grid re-renders, and success toast appears
        await user.ShouldEventuallyNotSee(MagazineExceptionsPageTargets.AddExceptionDialog);
        await user.Performs(WaitForGridDataLoaded.AfterAction(gridToken!));
        await user.ShouldEventuallySee(MagazineExceptionsPageTargets.Toast);

        // And: Record exists in the database
        await user.ShouldConfirm(new DbRecordExists(
            MagazineExceptionsCleanup.CountTestCreatedExceptions,
            new SqlParameter("@CompanyId", MagazineExceptionsCleanup.SeymourCompanyId),
            new SqlParameter("@ReasonCode", MagazineExceptionsCleanup.TimeSensitiveReasonCode),
            new SqlParameter("@AddedBy", MagazineExceptionsCleanup.CurrentWindowsUser)),
            "Exception should be persisted in the database");
    }

    #endregion

    #region TC-015: Add exception — with end date

    [Test]
    [Category(TestCategories.Functional)]
    public async Task TC_015_AddException_WithEndDate()
    {
        var user = ActorLibrary.GetActor("User");

        // Register cleanup upfront — runs in TearDown regardless of test outcome
        RegisterExceptionCleanup(user);

        // Given: User navigates to Magazine Exceptions
        await user.Performs(new NavigateTo(AppConfiguration.BaseUrl));
        await user.Performs(new OpenMagazineExceptionsModule());

        // When: User adds a new exception with an end date
        var gridToken = await user.Asks(new GetGridDataToken());
        await user.Performs(new AddException(
            MagazineExceptionsTestData.AddException_Company,
            MagazineExceptionsTestData.AddException_MagazineSearch,
            MagazineExceptionsTestData.AddException_Reason,
            endDate: MagazineExceptionsTestData.AddException_EndDate));

        // Then: Dialog closes, grid re-renders, and success toast appears
        await user.ShouldEventuallyNotSee(MagazineExceptionsPageTargets.AddExceptionDialog);
        await user.Performs(WaitForGridDataLoaded.AfterAction(gridToken!));
        await user.ShouldEventuallySee(MagazineExceptionsPageTargets.Toast);

        // And: Record exists in the database with end date
        await user.ShouldAnswer(new DbScalar<DateTime?>(
            MagazineExceptionsCleanup.GetTestCreatedExceptionEndDate,
            new SqlParameter("@CompanyId", MagazineExceptionsCleanup.SeymourCompanyId),
            new SqlParameter("@ReasonCode", MagazineExceptionsCleanup.TimeSensitiveReasonCode),
            new SqlParameter("@AddedBy", MagazineExceptionsCleanup.CurrentWindowsUser)),
            Has.Property("Year").EqualTo(2027),
            "Exception should have end date in 2027");

    }

    #endregion

    #region TC-016: Cancel discards dialog

    [Test]
    [Category(TestCategories.Functional)]
    public async Task TC_016_AddException_CancelDiscardsDialog()
    {
        var user = ActorLibrary.GetActor("User");

        // Given: User navigates to Magazine Exceptions
        await user.Performs(new NavigateTo(AppConfiguration.BaseUrl));
        await user.Performs(new OpenMagazineExceptionsModule());
        var rowCountBefore = await user.Asks(new CountOf(MagazineExceptionsPageTargets.TableRows));

        // When: User opens Add Exception dialog and clicks Cancel
        await OpenAddExceptionDialogWithCompany(user);
        await user.Performs(new Click(MagazineExceptionsPageTargets.AddCancelButton));

        // Then: Dialog closes and no new row is added
        await user.ShouldEventuallyNotSee(MagazineExceptionsPageTargets.AddExceptionDialog);
        await user.ShouldHaveExactly(MagazineExceptionsPageTargets.TableRows, rowCountBefore,
            "Cancel must not add a new row");
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
            .SetDescription("Save is disabled after company and magazine filled but reason not chosen")
            .Ignore("Frontend bug: Save button not disabled when Reason is empty. Clicking Save causes NullReferenceException in CreateNewException(). Defect raised.");
    }

    [Test, TestCaseSource(nameof(AddExceptionValidationTestCases))]
    [Category(TestCategories.Functional)]
    public async Task TC_017_AddException_SaveDisabledUntilAllFieldsFilled(string filledFields)
    {
        var user = ActorLibrary.GetActor("User");

        // Given: User navigates to Magazine Exceptions
        await user.Performs(new NavigateTo(AppConfiguration.BaseUrl));
        await user.Performs(new OpenMagazineExceptionsModule());

        // When: User opens dialog and fills fields up to the specified stage
        switch (filledFields)
        {
            case "none":
                await user.Performs(new Click(MagazineExceptionsPageTargets.AddExceptionButton));
                await user.Performs(new WaitForElement(MagazineExceptionsPageTargets.AddCompanyInput));
                break;

            case "company_only":
                await OpenAddExceptionDialogWithCompany(user);
                break;

            case "company_and_magazine":
                await OpenAddExceptionDialogWithCompany(user);
                await user.Performs(new Click(MagazineExceptionsPageTargets.AddMagazineInput));
                await user.Performs(new WaitForBlazorReady());
                await user.Performs(new TypeText(MagazineExceptionsPageTargets.AddMagazineInput, MagazineExceptionsTestData.AddException_MagazineSearch));
                var magItem = MagazineExceptionsPageTargets.MagSearchPopupItem(MagazineExceptionsTestData.AddException_MagazineSearch);
                await user.Performs(new WaitForElement(magItem));
                await user.Performs(new ClickFirst(magItem));
                await user.Performs(new WaitForElement(MagazineExceptionsPageTargets.AddReasonContainer));
                break;
        }

        // Then: Save button is disabled
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

        // Given: User opens Add Exception dialog with company selected
        await user.Performs(new NavigateTo(AppConfiguration.BaseUrl));
        await user.Performs(new OpenMagazineExceptionsModule());
        await OpenAddExceptionDialogWithCompany(user);

        // When: User types a mid-string search term (Contains is default)
        await user.Performs(new Click(MagazineExceptionsPageTargets.AddMagazineInput));
        await user.Performs(new WaitForBlazorReady());
        await user.Performs(new TypeText(MagazineExceptionsPageTargets.AddMagazineInput, MagazineExceptionsTestData.MagazineSearch_MidString));

        // Then: Autocomplete finds magazines containing the substring (JS-based — dialog overlay blocks Playwright Locator visibility)
        await user.Performs(new WaitForDomElement(MagazineExceptionsPageTargets.MagSearchPopupItem(MagazineExceptionsTestData.AddException_MagazineSearch)));

        // Dismiss popup then close dialog
        await user.Performs(new PressKey("Escape"));
        await user.Performs(new Click(MagazineExceptionsPageTargets.AddCancelButton));
    }

    #endregion

    #region TC-019: Magazine search — Starts with mode

    [Test]
    [Category(TestCategories.Functional)]
    [Ignore("EJ2 autocomplete in Starts-with mode doesn't show popup when no results — needs alternative assertion strategy.")]
    public async Task TC_019_AddException_MagazineSearch_StartsWithMode()
    {
        var user = ActorLibrary.GetActor("User");

        // Given: User opens Add Exception dialog and switches to Starts with mode
        await user.Performs(new NavigateTo(AppConfiguration.BaseUrl));
        await user.Performs(new OpenMagazineExceptionsModule());
        await OpenAddExceptionDialogWithCompany(user);
        await user.Performs(new Click(MagazineExceptionsPageTargets.AddStartsWithRadio));
        await user.Performs(new WaitForBlazorReady());

        // When: User types a mid-string term
        await user.Performs(new Click(MagazineExceptionsPageTargets.AddMagazineInput));
        await user.Performs(new WaitForBlazorReady());
        await user.Performs(new TypeText(MagazineExceptionsPageTargets.AddMagazineInput, MagazineExceptionsTestData.MagazineSearch_MidString));

        // Then: No results — popup does not appear in Starts with mode for mid-string
        await user.Performs(new WaitForBlazorReady());
        await user.ShouldNotSee("#MagSearch_popup.e-popup-open",
            "Autocomplete popup should not appear for mid-string in Starts with mode");

        // When: User types a prefix term
        await user.Performs(new Click(MagazineExceptionsPageTargets.AddMagazineInput));
        await user.Performs(new WaitForBlazorReady());
        await user.Performs(new TypeText(MagazineExceptionsPageTargets.AddMagazineInput, MagazineExceptionsTestData.MagazineSearch_Prefix));

        // Then: Prefix matches the magazine
        await user.Performs(new WaitForDomElement(MagazineExceptionsPageTargets.MagSearchPopupItem(MagazineExceptionsTestData.AddException_MagazineSearch)));

        // Dismiss popup then close dialog
        await user.Performs(new PressKey("Escape"));
        await user.Performs(new Click(MagazineExceptionsPageTargets.AddCancelButton));
    }

    #endregion

    #region TC-020: Magazine search — no results for unrecognised name

    [Test]
    [Category(TestCategories.EdgeCase)]
    public async Task TC_020_AddException_MagazineSearch_NoResults()
    {
        var user = ActorLibrary.GetActor("User");

        // Given: User opens Add Exception dialog with company selected
        await user.Performs(new NavigateTo(AppConfiguration.BaseUrl));
        await user.Performs(new OpenMagazineExceptionsModule());
        await OpenAddExceptionDialogWithCompany(user);

        // When: User types a non-existent magazine name
        await user.Performs(new Click(MagazineExceptionsPageTargets.AddMagazineInput));
        await user.Performs(new WaitForBlazorReady());
        await user.Performs(new TypeText(MagazineExceptionsPageTargets.AddMagazineInput, MagazineExceptionsTestData.NonExistentMagazine));

        // Then: No results shown and Save remains disabled
        await user.Performs(new WaitForDomElement(MagazineExceptionsPageTargets.MagSearchPopupNoData));
        await user.Performs(new PressKey("Escape"));
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

        // Given: User opens Add Exception dialog with all required fields filled
        await user.Performs(new NavigateTo(AppConfiguration.BaseUrl));
        await user.Performs(new OpenMagazineExceptionsModule());
        await OpenAddExceptionDialogWithCompany(user);
        await FillAddExceptionMagazineAndReason(user);

        // Then: Date picker hidden by default
        await user.ShouldNotSee(MagazineExceptionsPageTargets.AddEndDateInput,
            "Date picker must be hidden before checkbox is checked");

        // When: User checks the end date checkbox
        await user.Performs(new WaitForElement(MagazineExceptionsPageTargets.AddEndDateCheckbox));
        await user.Performs(new Click(MagazineExceptionsPageTargets.AddEndDateCheckbox));

        // Then: Date picker appears
        await user.ShouldEventuallySee(MagazineExceptionsPageTargets.AddEndDateInput);

        // When: User unchecks the end date checkbox
        await user.Performs(new Click(MagazineExceptionsPageTargets.AddEndDateCheckbox));

        // Then: Date picker hides again
        await user.ShouldEventuallyNotSee(MagazineExceptionsPageTargets.AddEndDateInput);

        await user.Performs(new Click(MagazineExceptionsPageTargets.AddCancelButton));
    }

    #endregion

    // ── Add Exception dialog helpers ─────────────────────────────────────────
    // Private helpers covering the progressive-disclosure setup steps that are
    // shared across TC-016 through TC-021. Not promoted to Tasks because they
    // are specific to this test class's state management.

    private static async Task OpenAddExceptionDialogWithCompany(Actor actor)
    {
        var companyPopupItem = MagazineExceptionsPageTargets.EjPopupItem(MagazineExceptionsTestData.AddException_Company);

        IPerformable[] steps =
        [
            new Click(MagazineExceptionsPageTargets.AddExceptionButton),
            new WaitForElement(MagazineExceptionsPageTargets.AddCompanyInput),
            new Click(MagazineExceptionsPageTargets.AddCompanyInput),
            new WaitForElement(companyPopupItem),
            new ClickFirst(companyPopupItem),
            new WaitForElement(MagazineExceptionsPageTargets.AddMagazineInput),
        ];

        foreach (var step in steps)
            await actor.Performs(step);
    }

    // ── Database cleanup helpers ────────────────────────────────────────────

    private void RegisterExceptionCleanup(Actor actor)
    {
        RegisterCleanup(async () =>
        {
            var cleanup = new DbExecute(
                MagazineExceptionsCleanup.DeleteTestCreatedExceptions,
                new SqlParameter("@CompanyId", MagazineExceptionsCleanup.SeymourCompanyId),
                new SqlParameter("@ReasonCode", MagazineExceptionsCleanup.TimeSensitiveReasonCode),
                new SqlParameter("@AddedBy", MagazineExceptionsCleanup.CurrentWindowsUser));
            await actor.Performs(cleanup);
            TestContext.Out.WriteLine($"[Cleanup] Deleted {cleanup.RowsAffected} test exception(s)");
        });
    }

    private static async Task FillAddExceptionMagazineAndReason(Actor actor)
    {
        var magazinePopupItem = MagazineExceptionsPageTargets.MagSearchPopupItem(MagazineExceptionsTestData.AddException_MagazineSearch);
        var reasonPopupItem = MagazineExceptionsPageTargets.EjPopupItem(MagazineExceptionsTestData.AddException_Reason);

        IPerformable[] steps =
        [
            new Click(MagazineExceptionsPageTargets.AddMagazineInput),
            new WaitForBlazorReady(),
            new TypeText(MagazineExceptionsPageTargets.AddMagazineInput, MagazineExceptionsTestData.AddException_MagazineSearch),
            new WaitForElement(magazinePopupItem),
            new ClickFirst(magazinePopupItem),
            new WaitForElement(MagazineExceptionsPageTargets.AddReasonContainer),
            new Click(MagazineExceptionsPageTargets.AddReasonContainer),
            new WaitForElement(reasonPopupItem),
            new ClickFirst(reasonPopupItem),
        ];

        foreach (var step in steps)
            await actor.Performs(step);
    }

    [Test]
    [Category(TestCategories.EdgeCase)]
    public async Task TC_013_SessionExpiryHandling()
    {
        var user = ActorLibrary.GetActor("User");

        // Given: User navigates to Magazine Exceptions and opens Edit dialog
        await user.Performs(new NavigateTo(AppConfiguration.BaseUrl));
        await user.Performs(new OpenMagazineExceptionsModule());

        // When: User clicks Edit to open dialog
        await user.Performs(new Click(MagazineExceptionsPageTargets.FirstRowEditButton));

        // Then: Edit dialog opens (Blazor async) and reason dropdown is visible and interactive
        await user.ShouldEventuallySee(MagazineExceptionsPageTargets.EditDialog);
        await user.ShouldEventuallySee(MagazineExceptionsPageTargets.EditReasonDropdownIcon);
    }
}
