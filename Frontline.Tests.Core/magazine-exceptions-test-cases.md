# Magazine Exceptions — Test Suite

## Overview

| Field         | Value                                    |
|---------------|------------------------------------------|
| Module        | Data Governance / Magazine Exceptions    |
| Application   | Frontline Applications                   |
| Execution     | Automated (NUnit 4 + Playwright)         |
| Last Updated  | 2026-03-14                               |

---

## Preconditions (shared across all test cases)

> These steps must be completed before executing any test case in this suite.

- Test Exception entities exist in the system with known values (see `MagazineExceptionsTestData.cs`):
  - Entity 1: `id=12`, `name=RADIO TIMES`, `company=FRONTLINE`
  - Entity 2: `id=10760`, `name=MOJO EXPORT`
  - Entity 3: `company=SEYMOUR`
- User is authenticated and has sufficient permissions to view, edit, and delete exceptions
- Browser is open and pointed at the Frontline Applications root URL (`AppConfiguration.BaseUrl`)
- `OpenMagazineExceptionsModule` task handles full navigation + grid-ready wait automatically

---

## TC-001 — Navigate to Magazine Exceptions page

**Goal:** Verify the user can navigate to the Magazine Exceptions section and the page loads correctly.

**Priority:** High | **Type:** Smoke | **Status:** Automated ✅

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | `NavigateTo(BaseUrl)` | Home page title contains `'Frontline'` |
| 2 | `OpenMagazineExceptionsModule` — clicks app tile, burger menu, nav link; waits for Blazor circuit + grid ready | Exceptions table header `#MagGrid_header_table` is visible |

**Pass Criteria:** `ExceptionsTable` selector is visible after module opens.

---

## TC-001b — Verify known entities are present

**Goal:** Confirm that Entity 1 (RADIO TIMES) and Entity 2 (MOJO EXPORT) appear in the grid after filtering by ID.

**Priority:** High | **Type:** Smoke | **Status:** Automated ✅

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open Magazine Exceptions | Grid loaded |
| 2 | Filter `MagIdFilterInput` by Entity 1 ID (`12`) | `ShouldEventuallyRead(FirstRowIdCell, "12")` passes |
| 3 | Clear filter, then filter by Entity 2 ID (`10760`) | `ShouldEventuallyRead(FirstRowIdCell, "10760")` passes |

**Pass Criteria:** Each entity's ID appears in the first row after filtering. Playwright retrying assertion used — no hardcoded waits.

---

## TC-002 — Find exception by ID

**Goal:** Verify the exceptions table can be filtered by ID.

**Priority:** High | **Type:** Functional | **Status:** Automated ✅ (parametrised)

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open Magazine Exceptions | Grid loaded |
| 2 | Fill `MagIdFilterInput` with `exceptionId` | Grid re-renders asynchronously |
| 3 | `ShouldEventuallySee(expectedSelector)` | Rows visible **or** empty-state message visible |

**Parametrised variants:**

| Test name | Input | `expectEmpty` | Expected outcome |
|---|---|---|---|
| `TC_002_a` | `12` (Entity 1) | `false` | `TableRows` visible |
| `TC_002_a2` | `10760` (Entity 2) | `false` | `TableRows` visible |
| `TC_002_b` | `99999` (non-existent) | `true` | `EmptyRecordsMessage` visible |
| `TC_002_c` | `107` (partial) | `false` | `TableRows` visible (multiple matches) |

---

## TC-003 — Find exception by Exception Name

**Goal:** Verify the exceptions table can be filtered by Magazine Name.

**Priority:** High | **Type:** Functional | **Status:** Automated ✅ (parametrised)

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open Magazine Exceptions | Grid loaded |
| 2 | Fill `MagazineNameFilterInput` with `exceptionName` | Grid re-renders |
| 3 | `ShouldEventuallySee(expectedSelector)` **or** graceful-handling check (for special chars) | See variants below |

**Parametrised variants:**

| Test name | Input | `expectEmpty` | Expected outcome |
|---|---|---|---|
| `TC_003_a` | `RADIO TIMES` (exact) | `false` | `TableRows` visible |
| `TC_003_a2` | `MOJO EXPORT` (exact) | `false` | `TableRows` visible |
| `TC_003_b` | `RADIO` (partial) | `false` | `TableRows` visible |
| `TC_003_c` | Non-existent name | `true` | `EmptyRecordsMessage` visible |
| `TC_003_d` | Special characters | `null` | `WaitForBlazorReady` + `ExceptionsContentTable` visible (outcome unpredictable — graceful handling only) |

---

## TC-004 — Find exception by Company

**Goal:** Verify the exceptions table can be filtered by Company.

**Priority:** High | **Type:** Functional | **Status:** Automated ✅ (parametrised)

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open Magazine Exceptions | Grid loaded |
| 2 | Fill `CompanyFilterInput` with `companyName` | Grid re-renders |
| 3 | `ShouldEventuallySee(expectedSelector)` **or** graceful-handling check | See variants below |

**Parametrised variants:**

| Test name | Input | `expectEmpty` | Expected outcome |
|---|---|---|---|
| `TC_004_a` | `FRONTLINE` (exact) | `false` | `TableRows` visible |
| `TC_004_a2` | `SEYMOUR` (exact) | `false` | `TableRows` visible |
| `TC_004_b` | `FRONT` (partial) | `false` | `TableRows` visible |
| `TC_004_c` | Non-existent company | `true` | `EmptyRecordsMessage` visible |
| `TC_004_d` | Special characters | `null` | Graceful handling only (no assertion on result count) |

---

## TC-005 — Edit exception and verify success toast

**Goal:** Verify the Edit dialog opens, reason can be changed, and a success toast is shown.

**Priority:** High | **Type:** Functional | **Status:** 🚫 Ignored — server-side bug

> **Blocked:** Edit action throws a server-side exception. All variants are `[Ignore]`d pending fix.
> Defect raised separately.

**Parametrised variants (defined, not running):**

| Test name | `newReason` | Expected outcome |
|---|---|---|
| `TC_005_a` | Valid short string | Success toast visible |
| `TC_005_b` | Max-length string | Success toast visible |
| `TC_005_c` | Special characters | Saves correctly |

**Steps (when unblocked):**

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open Magazine Exceptions | Grid loaded |
| 2 | `Click(FirstRowEditButton)` | Edit dialog visible |
| 3 | `ClearAndFill(EditReasonField, newReason)` | Field updated |
| 4 | `Click(EditSaveButton)` | Dialog closes |
| 5 | `ShouldEventuallySee(Toast)` | Toast notification appears |
| 6 | `ShouldHaveText(ToastMessage, "Success")` | Toast contains success message |

---

## TC-006 — Cancel edit does not persist changes

**Goal:** Verify Cancel/close discards unsaved edits.

**Priority:** Medium | **Type:** Functional | **Status:** 🚫 Ignored — server-side bug

> **Blocked:** Edit dialog throws before Cancel can be reached. `[Ignore]`d pending fix.

**Steps (when unblocked):**

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open Magazine Exceptions | Grid loaded |
| 2 | `TextOf(FirstRowReasonCell)` → capture `originalReason` | Snapshot of current value |
| 3 | `Click(FirstRowEditButton)` | Edit dialog visible |
| 4 | `ClearAndFill(EditReasonField, "Temporary Change")` | Field updated |
| 5 | `Click(EditCancelButton)` | Dialog closes without saving |
| 6 | `ShouldEventuallyRead(FirstRowReasonCell, originalReason)` | Original reason still displayed |

---

## TC-007 — Delete exception

**Goal:** Verify an exception can be deleted via confirmation dialog.

**Priority:** High | **Type:** Functional | **Status:** 🚫 Ignored — missing confirmation dialog

> **Blocked:** No delete confirmation dialog implemented. Delete executes immediately on button click without prompting.
> Defect raised separately.

**Steps (when unblocked):**

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open Magazine Exceptions | Grid loaded |
| 2 | `TextOf(FirstRowIdCell)` → capture `rowIdBeforeDelete` | Snapshot of first row ID |
| 3 | `Click(FirstRowDeleteButton)` | `DeleteConfirmDialog` visible |
| 4 | `Click(DeleteConfirmButton)` | Dialog closes |
| 5 | `ShouldEventuallyNotRead(FirstRowIdCell, rowIdBeforeDelete)` | Deleted ID no longer first row |

> ⚠️ Re-create test data via API before running to ensure idempotency.

---

## TC-008 — Delete confirmation can be cancelled

**Goal:** Verify cancelling the delete confirmation leaves the entity intact.

**Priority:** Medium | **Type:** Functional | **Status:** 🚫 Ignored — missing confirmation dialog

> **Blocked:** Same defect as TC-007. Cancel flow untestable until dialog exists.

**Steps (when unblocked):**

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open Magazine Exceptions | Grid loaded |
| 2 | `TextOf(FirstRowIdCell)` → capture `rowIdBefore` | Snapshot |
| 3 | `Click(FirstRowDeleteButton)` | `DeleteConfirmDialog` visible |
| 4 | `Click(DeleteCancelButton)` | Dialog dismissed |
| 5 | `ShouldEventuallyRead(FirstRowIdCell, rowIdBefore)` | Row unchanged |

---

## TC-009 — Table pagination

**Goal:** Verify Next/Previous page navigation works correctly.

**Priority:** Medium | **Type:** Functional | **Status:** Automated ✅

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open Magazine Exceptions | Grid loaded |
| 2 | Capture `firstRowOnPage1 = TextOf(FirstRowIdCell)` and `firstPageRowCount = CountOf(TableRows)` | Baseline values captured |
| 3 | Check `IsVisible(PaginationNextButton)` | Skip gracefully with `Assert.Pass` if single page |
| 4 | `Click(PaginationNextButton)` | `ShouldEventuallyNotRead(FirstRowIdCell, firstRowOnPage1)` — page 2 loaded |
| 5 | `ShouldHaveAtLeast(TableRows, 1)` | Second page has rows |
| 6 | `Click(PaginationPrevButton)` | `ShouldEventuallyRead(FirstRowIdCell, firstRowOnPage1)` — back to page 1 |
| 7 | `CountOf(TableRows)` == `firstPageRowCount` | Row count matches original page 1 |

**Pass Criteria:** First row ID changes after Next and reverts after Previous. Single-page datasets pass with a skip.

---

## TC-010 — Combined filter: ID + Name

**Goal:** Verify multiple simultaneous filters narrow results correctly.

**Priority:** Medium | **Type:** Functional | **Status:** Automated ✅

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open Magazine Exceptions | Grid loaded |
| 2 | Filter `MagIdFilterInput` by `Entity1_MagId` (`12`) | `ShouldEventuallyRead(FirstRowIdCell, "12")` |
| 3 | Capture `afterIdFilterCount = CountOf(TableRows)` | Baseline after ID filter |
| 4 | Filter `MagazineNameFilterInput` by `Entity1_Name` (`RADIO TIMES`) | `ShouldEventuallyRead(FirstRowIdCell, "12")` — grid settled |
| 5 | `afterNameFilterCount <= afterIdFilterCount` | Combined filters narrow or maintain count |

---

## TC-011 — Empty state when no exceptions match

**Goal:** Verify the grid shows an empty-state message when filtering yields no rows.

**Priority:** Low | **Type:** Edge Case | **Status:** Automated ✅

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open Magazine Exceptions | Grid loaded |
| 2 | Filter `MagIdFilterInput` by `NonExistentExceptionId` (`99999`) | Grid re-renders |
| 3 | `ShouldEventuallySee(EmptyRecordsMessage)` | `.e-grid .e-emptyrow` becomes visible |
| 4 | `ShouldSee(ExceptionsContentTable)` | Grid container still rendered (no crash) |

---

## TC-012 — Page access control (negative)

**Goal:** Verify the home page loads for an authenticated user (access smoke check).

**Priority:** High | **Type:** Security | **Status:** Automated ✅ (partial)

> **Note:** Full negative access control (unauthorised user) is not yet automated — requires a second actor without permissions. Current test verifies page title for an authorised user.

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | `NavigateTo(BaseUrl)` | Home page loaded |
| 2 | `ShouldHaveTitle(ExpectedHomePageTitle)` | Title contains expected substring |

---

## TC-013 — Session expiry handling

**Goal:** Verify the app handles session expiry gracefully during an edit operation.

**Priority:** Medium | **Type:** Edge Case | **Status:** 🚫 Ignored — server-side bug

> **Blocked:** Edit dialog cannot be opened due to the same server-side exception as TC-005/006. `[Ignore]`d pending fix.

**Steps (when unblocked):**

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open Magazine Exceptions | Grid loaded |
| 2 | `Click(FirstRowEditButton)` | Edit dialog visible |
| 3 | Invalidate session (manual step / test fixture) | — |
| 4 | `Click(EditSaveButton)` | Redirect to login **or** session-expired error shown |

---

---

## Add Exception dialog — field reference

> Dialog opened via **Add Exception** button on the Magazine Exceptions page.

| Field | Type | Notes |
|---|---|---|
| **Pick a Company** | Dropdown | Required; lists all companies |
| **Pick a Magazine** | Text search + radio (`Contains` / `Starts with`) | Required; searches magazines within selected company |
| **Pick a Reason** | Dropdown | Required; predefined reason codes |
| **Specific End Date needed?** | Checkbox | Optional; when checked reveals a date picker |
| **Save** | Button | Submits; validates all required fields |
| **Cancel** | Button | Closes dialog without saving |

---

## TC-014 — Add exception — happy path (no end date)

**Goal:** Verify a new exception can be added with all required fields and no end date.

**Priority:** High | **Type:** Functional | **Status:** Planned

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open Magazine Exceptions | Grid loaded; capture `rowCountBefore = CountOf(TableRows)` |
| 2 | Click **Add Exception** button | Add Exception dialog opens |
| 3 | Select a company from **Pick a Company** dropdown | Company field shows selection |
| 4 | Type a known magazine name into **Pick a Magazine** | Matching suggestions appear; select the target magazine |
| 5 | Select a reason from **Pick a Reason** dropdown | Reason field shows selection |
| 6 | Leave **Specific End Date needed?** unchecked | Date picker not visible |
| 7 | Click **Save** | Dialog closes; grid re-renders |
| 8 | `ShouldEventuallySee(TableRows)` | Grid has rows |
| 9 | Filter grid by the new magazine name; `ShouldEventuallyRead(ExceptionNameCell, magazineName)` | Newly added exception row is present |

**Pass Criteria:** Row count increases by 1 (or new row found by name filter).

> ⚠️ **Teardown:** Delete the added exception via API after the test to keep data clean.

---

## TC-015 — Add exception — with specific end date

**Goal:** Verify that checking **Specific End Date needed?** reveals a date picker and the exception is saved with the end date.

**Priority:** High | **Type:** Functional | **Status:** Planned

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open Magazine Exceptions | Grid loaded |
| 2 | Click **Add Exception** button | Dialog opens |
| 3 | Select company, magazine, and reason (valid values) | Fields populated |
| 4 | Check **Specific End Date needed?** | Date picker field becomes visible |
| 5 | Enter a valid future date | Date field accepts input |
| 6 | Click **Save** | Dialog closes |
| 7 | Filter grid to find the new row; verify end date column | End date matches entered value |

**Pass Criteria:** Exception saved with end date visible in the grid row.

> ⚠️ **Teardown:** Delete added exception via API.

---

## TC-016 — Cancel Add Exception discards dialog

**Goal:** Verify Cancel closes the dialog without adding a new record.

**Priority:** Medium | **Type:** Functional | **Status:** Planned

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open Magazine Exceptions | Grid loaded; capture `rowCountBefore = CountOf(TableRows)` |
| 2 | Click **Add Exception** button | Dialog opens |
| 3 | Fill all fields with valid values | Fields populated |
| 4 | Click **Cancel** | Dialog closes immediately |
| 5 | `CountOf(TableRows)` == `rowCountBefore` | No new row added |

**Pass Criteria:** Row count unchanged after Cancel.

---

## TC-017 — Validation: required fields cannot be empty (parametrised)

**Goal:** Verify Save is blocked when any required field is missing.

**Priority:** High | **Type:** Functional | **Status:** Planned (parametrised)

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open Magazine Exceptions; click **Add Exception** | Dialog opens |
| 2 | Fill all fields **except** the one under test | Partial form |
| 3 | Click **Save** | Dialog stays open; validation error shown for the missing field |
| 4 | `ShouldSee(AddExceptionDialog)` | Dialog remains visible |

**Parametrised variants:**

| Test name | Field omitted | Expected validation message |
|---|---|---|
| `TC_017_a` | Company not selected | Company required error |
| `TC_017_b` | Magazine not entered | Magazine required error |
| `TC_017_c` | Reason not selected | Reason required error |

---

## TC-018 — Magazine search: Contains mode

**Goal:** Verify the magazine search with **Contains** radio finds magazines where the name includes the search string anywhere.

**Priority:** Medium | **Type:** Functional | **Status:** Planned

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open **Add Exception** dialog | Dialog open |
| 2 | Select a company | Company set |
| 3 | Ensure **Contains** radio is selected | Default mode |
| 4 | Type a substring known to appear mid-name (e.g. `AMER` for `GAMER`) | Suggestions include magazines containing `AMER` |
| 5 | Type a prefix that also appears mid-name | Results include non-prefix matches |

**Pass Criteria:** Results include magazines where the search string appears at any position.

---

## TC-019 — Magazine search: Starts with mode

**Goal:** Verify the **Starts with** radio restricts results to magazines whose names begin with the search string.

**Priority:** Medium | **Type:** Functional | **Status:** Planned

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open **Add Exception** dialog | Dialog open |
| 2 | Select a company | Company set |
| 3 | Select **Starts with** radio | Mode changed |
| 4 | Type the same mid-name substring used in TC-018 (e.g. `AMER`) | Results do **not** include `GAMER`; only magazines starting with `AMER` |
| 5 | Type a known prefix (e.g. `GAM`) | Results include `GAMER` and other `GAM…` magazines |

**Pass Criteria:** `Starts with` results are a strict subset of `Contains` results for the same query.

---

## TC-020 — Magazine search: no results for unrecognised name

**Goal:** Verify the dialog handles gracefully a magazine name that does not exist.

**Priority:** Low | **Type:** Edge Case | **Status:** Planned

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open **Add Exception** dialog | Dialog open |
| 2 | Select any company | Company set |
| 3 | Type a non-existent magazine name (e.g. `ZZZNOMATCH`) | No suggestions / empty suggestion list shown |
| 4 | Click **Save** | Save blocked (magazine not resolved to a valid entity) or validation error shown |

**Pass Criteria:** No exception is added; dialog remains open with an informative state.

---

## TC-021 — Specific End Date: checkbox toggles date picker

**Goal:** Verify the end date field is hidden by default and shown only when the checkbox is checked; unchecking hides it again.

**Priority:** Medium | **Type:** Functional | **Status:** Planned

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open **Add Exception** dialog | Dialog open |
| 2 | Verify end date picker is not visible | `ShouldNotSee(AddEndDateInput)` |
| 3 | Check **Specific End Date needed?** | Date picker becomes visible (`ShouldEventuallySee(AddEndDateInput)`) |
| 4 | Uncheck **Specific End Date needed?** | Date picker hidden again (`ShouldEventuallyNotSee(AddEndDateInput)`) |

**Pass Criteria:** Date picker visibility toggles with the checkbox state.

---

## Automation Notes

| Consideration | Implementation |
|---|---|
| **Framework** | Screenplay pattern — Actor → Tasks → Interactions → Questions; no raw Playwright in test bodies |
| **Wait strategy** | `ShouldEventuallyRead` / `ShouldEventuallySee` / `ShouldEventuallyNotRead` — Playwright retrying assertions; no `Task.Delay` |
| **Blazor readiness** | `NavigateTo` calls `WaitForPageLoaded` automatically; `OpenMagazineExceptionsModule` ends with `WaitForBlazorReady` + `WaitForGridReady` |
| **Filter settle** | `FilterExceptionsBy` fills only; caller uses `ShouldEventuallyRead` as the combined wait+assert |
| **Parametrised tests** | TC-002/003/004/005/017 use `[TestCaseSource]`; `bool? expectEmpty` distinguishes rows / empty / graceful-handling outcomes |
| **Blocked tests** | TC-005/006/013 blocked by Edit server bug; TC-007/008 blocked by missing confirmation dialog — all `[Ignore]`d with defect notes |
| **Test data** | Static known entities in `MagazineExceptionsTestData.cs`; TC-014/015 need API teardown to stay idempotent |
| **New targets needed** | `AddExceptionButton`, `AddExceptionDialog`, `AddCompanyDropdown`, `AddMagazineInput`, `AddContainsRadio`, `AddStartsWithRadio`, `AddReasonDropdown`, `AddEndDateCheckbox`, `AddEndDateInput`, `AddSaveButton`, `AddCancelButton` — add to `MagazineExceptionsPageTargets.cs` |
