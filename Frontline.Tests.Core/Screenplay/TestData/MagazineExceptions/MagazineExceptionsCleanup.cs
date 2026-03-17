namespace Frontline.Tests.Core.Screenplay.TestData.MagazineExceptions;

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

    // DB IDs corresponding to the UI values used in TC_014/TC_015
    public const int SeymourCompanyId = 1;
    public const int TimeSensitiveReasonCode = 15;
}
