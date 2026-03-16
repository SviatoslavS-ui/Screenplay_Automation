namespace Frontline.Tests.Core.Screenplay.TestData.MagazineExceptions;

/// <summary>Expected values for Magazine Exceptions tests.</summary>
public static class MagazineExceptionsTestData
{
    // ── Page Headers ──────────────────────────────────────────────────────────

    public const string ExpectedHomePageTitle = "FLGroup Apps";
    public const string ExpectedPageHeader = "Magazine Exceptions";

    // ── Test Entities (Verified to exist in database) ─────────────────────────

    // Entity 1
    public const string Entity1_MagId = "12";
    public const string Entity1_Name = "RADIO TIMES";
    public const string Entity1_Company = "FRONTLINE";
    public const string Entity1_Reason = "OTHER";

    // Entity 2
    public const string Entity2_MagId = "10760";
    public const string Entity2_Name = "MOJO EXPORT";
    public const string Entity2_Company = "FRONTLINE";
    public const string Entity2_Reason = "EMBARGO";

    // Entity 3
    public const string Entity3_MagId = "10216";
    public const string Entity3_Name = "X 360 EXPORT";
    public const string Entity3_Company = "SEYMOUR";
    public const string Entity3_Reason = "NOT TO UK WHOLESALE";

    // ── Filter Test Data ──────────────────────────────────────────────────────

    // TC-002: Find by ID
    public const string ValidExceptionId = Entity1_MagId;
    public const string ValidExceptionId2 = Entity2_MagId;
    public const string NonExistentExceptionId = "99999";
    public const string PartialExceptionId = "107";

    // TC-003: Find by Exception Name
    public const string ValidExceptionName = Entity1_Name;
    public const string ValidExceptionName2 = Entity2_Name;
    public const string PartialExceptionName = "RADIO";
    public const string NonExistentExceptionName = "NonExistentException";
    public const string SpecialCharExceptionName = "Test!@#$%";

    // TC-004: Find by Company
    public const string ValidCompanyName = Entity1_Company;
    public const string ValidCompanyName2 = Entity3_Company;
    public const string PartialCompanyName = "FRONT";
    public const string NonExistentCompanyName = "NonExistentCorp";
    public const string SpecialCharCompanyName = "Test & Co.";

    // TC-005: Edit exception reason
    public const string ValidNewReason = "UPDATED - Test Automation";
    public const string EmptyReason = "";
    public const string MaxLengthReason = "This is a very long exception reason that tests the maximum allowed length for the exception reason field in the system";
    public const string SpecialCharReason = "Reason with special chars: !@#$%^&*()";

    // TC-006: Original reason for cancel test (use real entity reason)
    public const string OriginalReasonForCancelTest = Entity1_Reason;

    // ── Add Exception (TC-014+) ───────────────────────────────────────────────

    public const string AddException_Company        = "SEYMOUR";
    public const string AddException_MagazineSearch = "GAMER";
    public const string AddException_Reason         = "TIME SENSITIVE";
    public const string AddException_EndDate        = "15/06/2027";   // DD/MM/YYYY — unambiguous future date

    // TC-018/019: magazine search mode — "AMER" is mid-string in "GAMER", "GAM" is a prefix
    public const string MagazineSearch_MidString = "AMER";
    public const string MagazineSearch_Prefix    = "GAM";

    // TC-020: non-existent magazine
    public const string NonExistentMagazine = "ZZZNOMATCH";

    // ── Toast/UI Messages ─────────────────────────────────────────────────────

    public const string SuccessToastMessage = "Success";
    public const string NoResultsEmptyStateMessage = "No records found";
}


