namespace Frontline.Tests.Core.Screenplay.Targets;

/// <summary>
/// UI locators for the Magazine Exceptions page.
/// Centralized location for all selectors to minimize maintenance when UI changes.
/// </summary>
public static class MagazineExceptionsPageTargets
{
    // ── Navigation ────────────────────────────────────────────────────────────

    /// <summary>Home page tile that launches the Magazine Exceptions app.</summary>
    public const string MagazineExceptionsAppButton = "button:has-text(\"Magazine Exceptions\")";

    /// <summary>Syncfusion dropdown burger menu. Identified by the e-menu icon it contains — id is dynamic.</summary>
    public const string BurgerMenuButton = "button.e-dropdown-btn:has(span.e-icons.e-menu)";

    /// <summary>Sidebar menu item that navigates to the exceptions grid.
    public const string MagazineExceptionsMenuItem = "a.e-menu-url[href='/MagazineExceptions']";

    // ── Page Elements ─────────────────────────────────────────────────────────

    public const string PageHeader = "span.pageheader";

    // ── Grid ──────────────────────────────────────────────────────────────────

    /// <summary>Syncfusion grid header table — use to verify the grid has rendered.</summary>
    public const string ExceptionsTable = "#MagGrid_header_table";

    /// <summary>Syncfusion grid content table — use to verify data rows have loaded.</summary>
    public const string ExceptionsContentTable = "#MagGrid_content_table";

    /// <summary>Data rows inside the content table (excludes header/filter rows).</summary>
    public const string TableRows = "#MagGrid_content_table tbody tr.e-row";

    // ── Column Headers (by data-colindex) ─────────────────────────────────────

    public const string MagIdColumnHeader          = "th[data-colindex='0'] .e-headertext";
    public const string MagazineNameColumnHeader   = "th[data-colindex='1'] .e-headertext";
    public const string CompanyColumnHeader        = "th[data-colindex='2'] .e-headertext";
    public const string ExceptionReasonColumnHeader    = "th[data-colindex='3'] .e-headertext";
    public const string ExceptionEndDateColumnHeader   = "th[data-colindex='4'] .e-headertext";
    public const string ExceptionAddedDateColumnHeader = "th[data-colindex='5'] .e-headertext";
    public const string ExceptionAddedByColumnHeader   = "th[data-colindex='6'] .e-headertext";

    // ── Filter Bar Inputs (by input id) ──────────────────────────────────────

    public const string MagIdFilterInput              = "#MagazineId_filterBarcell";
    public const string MagazineNameFilterInput       = "#MagazineIdAsString_filterBarcell";
    public const string CompanyFilterInput            = "#company_filterBarcell";
    public const string ExceptionReasonFilterInput    = "#ExceptionReasonCode_filterBarcell";
    public const string ExceptionEndDateFilterInput   = "#ExceptionEndDateNicer_filterBarcell";
    public const string ExceptionAddedDateFilterInput = "#ExceptionAddedDate_filterBarcell";
    public const string ExceptionAddedByFilterInput   = "#ExceptionAddedBy_filterBarcell";

    // ── Cell Selectors ────────────────────────────────────────────────────────

    /// <summary>Magazine Name cell in the first data row.</summary>
    public const string ExceptionNameCell = "#MagGrid_content_table tbody tr.e-row:first-child td[aria-colindex='2']";
}
