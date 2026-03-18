namespace Frontline.Tests.Core.Screenplay.TestData.MagazineExceptions;

/// <summary>Seed data descriptor for a required test entity in dbo.EXCEPTIONS_MAGAZINES.</summary>
public record EntitySeedData(int MagazineId, int CompanyId, string ReasonCode, string ReasonText, string Label);

/// <summary>SQL queries and constants for Magazine Exceptions test data cleanup.</summary>
public static class MagazineExceptionsCleanup
{
    /// <summary>
    /// Deletes test-created exceptions by company, reason, and the Windows user who created them.
    /// Parameters: @CompanyId (int), @ReasonCode (int), @AddedBy (string)
    /// </summary>
    public const string DeleteTestCreatedExceptions = @"
        DELETE FROM dbo.EXCEPTIONS_MAGAZINES
        WHERE COMPANY_ID = @CompanyId
          AND EXCEPTION_REASON_CODE = @ReasonCode
          AND EXCEPTION_ADDED_BY = @AddedBy";

    /// <summary>Returns the current Windows identity (domain\user) for cleanup scoping.</summary>
    public static string CurrentWindowsUser =>
        System.Security.Principal.WindowsIdentity.GetCurrent().Name;

    /// <summary>Counts test-created exceptions matching the cleanup criteria.</summary>
    public const string CountTestCreatedExceptions = @"
        SELECT COUNT(*) FROM dbo.EXCEPTIONS_MAGAZINES
        WHERE COMPANY_ID = @CompanyId
          AND EXCEPTION_REASON_CODE = @ReasonCode
          AND EXCEPTION_ADDED_BY = @AddedBy";

    /// <summary>Returns the end date of the most recent test-created exception.</summary>
    public const string GetTestCreatedExceptionEndDate = @"
        SELECT TOP 1 EXCEPTION_END_DATE FROM dbo.EXCEPTIONS_MAGAZINES
        WHERE COMPANY_ID = @CompanyId
          AND EXCEPTION_REASON_CODE = @ReasonCode
          AND EXCEPTION_ADDED_BY = @AddedBy
        ORDER BY EX_MAG_ID DESC";

    // Company IDs
    public const int SeymourCompanyId = 1;
    public const int FrontlineCompanyId = 0;

    // Reason codes
    public const int TimeSensitiveReasonCode = 15;

    // Entity1 (RADIO TIMES, Magazine ID = 12) — used by TC_005 edit tests
    public const int Entity1MagazineId = 12;

    // ── Fixture seed data — entities required to exist before any test runs ──

    /// <summary>Entity 1: RADIO TIMES / FRONTLINE / OTHER</summary>
    public static readonly EntitySeedData Entity1Seed =
        new(12, FrontlineCompanyId, "10", "OTHER", "RADIO TIMES / FRONTLINE");

    /// <summary>Entity 2: MOJO EXPORT / FRONTLINE / EMBARGO</summary>
    public static readonly EntitySeedData Entity2Seed =
        new(10760, FrontlineCompanyId, "3", "EMBARGO", "MOJO EXPORT / FRONTLINE");

    /// <summary>Entity 3: X 360 EXPORT / SEYMOUR — reason is nominal; only company filter tests rely on this row.</summary>
    public static readonly EntitySeedData Entity3Seed =
        new(10216, SeymourCompanyId, "3", "EMBARGO", "X 360 EXPORT / SEYMOUR");

    /// <summary>
    /// Returns 1 if an active record for the given magazine+company exists, 0 otherwise.
    /// Parameters: @MagazineId (int), @CompanyId (int)
    /// </summary>
    public const string CheckEntityExists = @"
        SELECT COUNT(*) FROM dbo.EXCEPTIONS_MAGAZINES
        WHERE MAGAZINE_ID = @MagazineId
          AND COMPANY_ID  = @CompanyId
          AND DELETED_FLAG = 'N'";

    /// <summary>
    /// Inserts a seed record using PRODUCT_ID resolved from V_DOTNET_PRODUCT.
    /// Returns 0 rows affected if the magazine is not found in the product view.
    /// Parameters: @MagazineId (int), @CompanyId (int), @ReasonCode (nvarchar), @ReasonText (nvarchar)
    /// </summary>
    public const string SeedEntity = @"
        INSERT INTO dbo.EXCEPTIONS_MAGAZINES
               (PRODUCT_ID, MAGAZINE_ID, COMPANY_ID, EXCEPTION_REASON_CODE, EXCEPTION_REASON, EXCEPTION_ADDED_BY, DELETED_FLAG)
        SELECT TOP 1
               [Product ID], @MagazineId, @CompanyId, @ReasonCode, @ReasonText, 'TEST_SEED', 'N'
        FROM   V_DOTNET_PRODUCT
        WHERE  [Product ID] = @MagazineId";

    /// <summary>
    /// Returns the current EXCEPTION_REASON_CODE for a specific magazine+company record.
    /// Parameters: @MagazineId (int), @CompanyId (int)
    /// </summary>
    public const string GetExceptionReasonCode = @"
        SELECT TOP 1 EXCEPTION_REASON_CODE FROM dbo.EXCEPTIONS_MAGAZINES
        WHERE MAGAZINE_ID = @MagazineId
          AND COMPANY_ID  = @CompanyId";

    /// <summary>
    /// Restores EXCEPTION_REASON_CODE to its original value for a specific magazine+company record.
    /// Parameters: @ReasonCode (int), @MagazineId (int), @CompanyId (int)
    /// </summary>
    public const string RestoreExceptionReason = @"
        UPDATE dbo.EXCEPTIONS_MAGAZINES
        SET    EXCEPTION_REASON_CODE = @ReasonCode
        WHERE  MAGAZINE_ID = @MagazineId
          AND  COMPANY_ID  = @CompanyId";
}
