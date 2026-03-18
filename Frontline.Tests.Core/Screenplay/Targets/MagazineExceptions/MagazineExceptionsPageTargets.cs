namespace Frontline.Tests.Core.Screenplay.Targets.MagazineExceptions;

/// <summary>CSS selectors for the Magazine Exceptions page and navigation.</summary>
public static class MagazineExceptionsPageTargets
{
    // ── Navigation ────────────────────────────────────────────────────────────

    /// <summary>Home page tile that launches the Magazine Exceptions app.</summary>
    public const string MagazineExceptionsAppButton = "button:has-text(\"Magazine Exceptions\")";

    /// <summary>Syncfusion dropdown burger menu — interact via ClickAriaToggle.ToExpand("dropdownbutton").</summary>
    public const string BurgerMenuButton = "[aria-label=\"dropdownbutton\"]";

    /// <summary>Sidebar nav link to the exceptions grid.</summary>
    public const string MagazineExceptionsMenuItem = "a.e-menu-url[href='/MagazineExceptions']";

    // ── Grid ──────────────────────────────────────────────────────────────────

    /// <summary>Root ID of the Syncfusion grid — used by WaitForGridReady to detect aria-busy state.</summary>
    public const string GridId = "#MagGrid";

    /// <summary>Syncfusion grid header table — use to verify the grid has rendered.</summary>
    public const string ExceptionsTable = "#MagGrid_header_table";

    /// <summary>Syncfusion grid content table — use to verify data rows have loaded.</summary>
    public const string ExceptionsContentTable = "#MagGrid_content_table";

    /// <summary>Data rows inside the content table (excludes header/filter rows).</summary>
    public const string TableRows = "#MagGrid_content_table tbody tr.e-row";

    // ── Filter Bar Inputs (by input id) ──────────────────────────────────────

    public const string MagIdFilterInput = "#MagazineId_filterBarcell";
    public const string MagazineNameFilterInput = "#MagazineIdAsString_filterBarcell";
    public const string CompanyFilterInput = "#company_filterBarcell";

    // ── Cell Selectors ────────────────────────────────────────────────────────

    /// <summary>First row ID cell for verification.</summary>
    public const string FirstRowIdCell = "#MagGrid_content_table tbody tr.e-row:first-child td[aria-colindex='1']";

    /// <summary>First row reason cell for verification.</summary>
    public const string FirstRowReasonCell = "#MagGrid_content_table tbody tr.e-row:first-child td[aria-colindex='4']";

    // ── Dialog/Modal Elements ─────────────────────────────────────────────────

    /// <summary>Edit dialog modal container.</summary>
    public const string EditDialog = ".e-dialog[role='dialog']";

    /// <summary>Reason dropdown arrow icon in the edit dialog — click this to open the EJ2 dropdown list.</summary>
    public const string EditReasonDropdownIcon = ".e-dialog .e-input-group-icon.e-ddl-icon";

    /// <summary>Edit dialog Save button.</summary>
    public const string EditSaveButton = ".e-dialog button:has-text('Save')";

    /// <summary>Edit dialog Cancel button.</summary>
    public const string EditCancelButton = ".e-dialog button:has-text('Cancel')";

    /// <summary>Delete confirmation dialog.</summary>
    public const string DeleteConfirmDialog = ".e-dialog[role='dialog']:has-text('Delete')";

    /// <summary>Delete confirmation OK/Yes button.</summary>
    public const string DeleteConfirmButton = ".e-dialog button:has-text('Delete'), .e-dialog button:has-text('Yes')";

    /// <summary>Delete confirmation Cancel button.</summary>
    public const string DeleteCancelButton = ".e-dialog button:has-text('Cancel'), .e-dialog button:has-text('No')";

    // ── Row Action Buttons ────────────────────────────────────────────────────

    /// <summary>Edit button in the first data row (action column).</summary>
    public const string FirstRowEditButton = "#MagGrid_content_table tbody tr.e-row:first-child button:has-text('Edit')";

    /// <summary>Edit button in the row whose Magazine ID column exactly matches <paramref name="magId"/>.</summary>
    public static string RowEditButton(string magId) =>
        $"#MagGrid_content_table tbody tr.e-row:has(td[aria-colindex='1']:text-is('{magId}')) button:has-text('Edit')";

    /// <summary>Delete button in the first data row (action column).</summary>
    public const string FirstRowDeleteButton = "#MagGrid_content_table tbody tr.e-row:first-child button:has-text('Delete')";

    // ── Toast Notification ────────────────────────────────────────────────────

    /// <summary>Toast notification container.</summary>
    public const string Toast = "div#toast, .e-toast-container";

    // ── Pagination ────────────────────────────────────────────────────────────

    /// <summary>Next page button in Syncfusion grid pager.</summary>
    public const string PaginationNextButton = ".e-pagercontainer .e-numericitem:has-text('Next')";

    /// <summary>Previous page button in Syncfusion grid pager.</summary>
    public const string PaginationPrevButton = ".e-pagercontainer .e-numericitem:has-text('Previous')";

    // ── Empty State ───────────────────────────────────────────────────────────

    /// <summary>Empty records message when no data is present.</summary>
    public const string EmptyRecordsMessage = ".e-grid .e-emptyrow";

    // ── Add Exception Dialog ──────────────────────────────────────────────────

    /// <summary>Button on the Magazine Exceptions page that opens the Add Exception dialog.</summary>
    public const string AddExceptionButton = "button:has-text('Add Exception')";

    /// <summary>Content area of the Add Exception dialog.</summary>
    public const string AddExceptionDialog = ".e-dlg-content";

    /// <summary>Company dropdown wrapper (click the wrapper span, not the readonly input, to open EJ2 dropdown).</summary>
    public const string AddCompanyInput = "span.e-ddl:has(#companyPicker)";

    /// <summary>Magazine autocomplete search input — appears after company is selected.</summary>
    public const string AddMagazineInput = "#MagSearch";

    /// <summary>Contains search mode radio button label.</summary>
    public const string AddContainsRadio = ".e-radio-wrapper:has(input[value='contains']) label";

    /// <summary>Starts with search mode radio button label.</summary>
    public const string AddStartsWithRadio = ".e-radio-wrapper:has(input[value='startswith']) label";

    /// <summary>Reason dropdown container — click to open (input has no stable id; identified by placeholder).</summary>
    public const string AddReasonContainer = ".e-dlg-content span.e-ddl:has(input[placeholder='e.g. SRP'])";

    /// <summary>End date checkbox — toggles date picker visibility.</summary>
    public const string AddEndDateCheckbox = ".e-dlg-content input[type='checkbox']";

    /// <summary>EJ2 DatePicker input — visible only when end date checkbox is checked.</summary>
    public const string AddEndDateInput = ".e-dlg-content .e-date-wrapper input";

    /// <summary>Save button — disabled until all required fields are filled.</summary>
    public const string AddSaveButton = ".e-dlg-content button:has-text('Save')";

    /// <summary>Save button in its disabled state — present when required fields are incomplete.</summary>
    public const string AddSaveButtonDisabled = ".e-dlg-content button:has-text('Save'):disabled";

    /// <summary>Cancel button in the Add Exception dialog.</summary>
    public const string AddCancelButton = ".e-dlg-content button:has-text('Cancel')";

    // ── EJ2 Popup (shared — only one popup open at a time) ────────────────────

    /// <summary>EJ2 dropdown/autocomplete list item matched by exact data-value. Works for company and reason popups.</summary>
    public static string EjPopupItem(string text) => $".e-popup-open .e-list-item[data-value='{text}']";

    /// <summary>Magazine autocomplete popup item — scoped to #MagSearch_popup (required for Playwright visibility
    /// when the dialog overlay blocks generic .e-popup-open matching).</summary>
    public static string MagSearchPopupItem(string text) => $"#MagSearch_popup .e-list-item[data-value='{text}']";

    /// <summary>Magazine autocomplete no-data message.</summary>
    public const string MagSearchPopupNoData = "#MagSearch_popup .e-nodata";
}
