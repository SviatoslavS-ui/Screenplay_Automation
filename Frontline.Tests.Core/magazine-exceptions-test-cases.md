# Magazine Exceptions — Test Suite

## Overview

| Field         | Value                                    |
|---------------|------------------------------------------|
| Module        | Data Governance / Magazine Exceptions    |
| Application   | Frontline Applications                   |
| Execution     | Manual (automation-ready)                |
| Last Updated  | 2026-03-10                               |

---

## Preconditions (shared across all test cases)

> These steps must be completed before executing any test case in this suite.

- A test Exception entity exists in the system with known values:
  - `id`: `{EXCEPTION_ID}`
  - `name`: `{EXCEPTION_NAME}`
  - `company`: `{COMPANY_NAME}`
  - `reason`: `{EXCEPTION_REASON}`
- User is authenticated and has sufficient permissions to view, edit, and delete exceptions
- Browser is open and pointed at the Frontline Applications root URL

---

## TC-001 — Navigate to Magazine Exceptions page

**Goal:** Verify the user can navigate to the Magazine Exceptions section and the page loads correctly.

**Priority:** High | **Type:** Smoke

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open Frontline Applications | Application loads without errors |
| 2 | Navigate to **Data Governance → Magazine Exceptions** | `span.pageheader` text equals `'Welcome'` |
| 3 | Click the **burger icon** in the top menu | Navigation menu expands |
| 4 | Select **Magazine Exceptions** from the menu | A paginated list/table of exceptions is displayed |

**Pass Criteria:** Table is visible and contains at least one row.

---

## TC-002 — Find exception by ID

**Goal:** Verify the exceptions table can be filtered or sorted by ID.

**Priority:** High | **Type:** Functional  
**Parameters:** `{EXCEPTION_ID}`

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Magazine Exceptions (see TC-001) | Table is displayed |
| 2 | Enter `{EXCEPTION_ID}` into the **ID** search/filter field **or** click the **ID** column header to sort | Table updates to show matching row(s) |
| 3 | Verify the row with `{EXCEPTION_ID}` is visible | Row with exact ID is present in results |

**Pass Criteria:** The row matching `{EXCEPTION_ID}` is visible after filtering/sorting.

> **Parametrised variants:**
>
> | # | EXCEPTION_ID   | Expected outcome      |
> |---|----------------|-----------------------|
> | a | Valid existing | Row found             |
> | b | Non-existent   | No rows / empty state |
> | c | Partial string | Matching rows shown   |

---

## TC-003 — Find exception by Exception Name

**Goal:** Verify the exceptions table can be filtered or sorted by Exception Name.

**Priority:** High | **Type:** Functional  
**Parameters:** `{EXCEPTION_NAME}`

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Magazine Exceptions | Table is displayed |
| 2 | Enter `{EXCEPTION_NAME}` into the **Exception Name** search/filter field **or** sort by that column | Table updates |
| 3 | Verify the target row is visible | Row with matching name is present |

> **Parametrised variants:**
>
> | # | EXCEPTION_NAME        | Expected outcome         |
> |---|-----------------------|--------------------------|
> | a | Exact match           | Correct row found        |
> | b | Partial name          | All partial matches shown|
> | c | Case variation        | Case-insensitive match   |
> | d | Non-existent name     | Empty results / no rows  |
> | e | Special characters    | Handled gracefully       |

---

## TC-004 — Find exception by Magazine Name (Company)

**Goal:** Verify the exceptions table can be filtered or sorted by Magazine/Company Name.

**Priority:** High | **Type:** Functional  
**Parameters:** `{COMPANY_NAME}`

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Magazine Exceptions | Table is displayed |
| 2 | Enter `{COMPANY_NAME}` into the **Magazine Name** search/filter field **or** sort by that column | Table updates |
| 3 | Verify the target row is visible | Row with matching company is present |

> **Parametrised variants:** Same pattern as TC-003 — exact, partial, case, non-existent, special chars.

---

## TC-005 — Edit exception and verify success toast

**Goal:** Verify the Edit dialog opens, changes to Exception Reason can be saved, and a success notification is shown.

**Priority:** High | **Type:** Functional  
**Parameters:** `{EXCEPTION_ID}`, `{NEW_REASON}`

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Magazine Exceptions | Table is displayed |
| 2 | Locate row with `{EXCEPTION_ID}` | Row is visible |
| 3 | Click the **Edit** button in that row | Edit dialog/modal opens |
| 4 | Verify dialog is open | Dialog element is visible; fields are pre-populated with current values |
| 5 | Clear the **Exception Reason** field and enter `{NEW_REASON}` | Field updates to new value |
| 6 | Click **Save** | Dialog closes |
| 7 | Verify `div#toast` element appears | Toast notification is visible |
| 8 | Verify toast message contains `'Success'` | Toast text includes `'Success'` |
| 9 | Locate the row again and verify the Reason column shows `{NEW_REASON}` | Persisted change visible in table |

**Pass Criteria:** Toast with `'Success'` is shown and the updated reason persists in the table.

> **Parametrised variants:**
>
> | # | NEW_REASON                    | Expected outcome              |
> |---|-------------------------------|-------------------------------|
> | a | Valid short string            | Saved, success toast          |
> | b | Max-length string             | Saved or validation error     |
> | c | Empty / blank                 | Validation error shown        |
> | d | Special/unicode characters    | Saved correctly or validation |

---

## TC-006 — Cancel edit does not persist changes

**Goal:** Verify that closing/cancelling the Edit dialog discards unsaved changes.

**Priority:** Medium | **Type:** Functional

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Magazine Exceptions | Table is displayed |
| 2 | Click **Edit** on target row | Edit dialog opens |
| 3 | Modify the **Exception Reason** field | Field shows new value |
| 4 | Click **Cancel** or close the dialog (×) | Dialog closes without saving |
| 5 | Verify original reason still shows in the table | No change persisted |

---

## TC-007 — Delete exception

**Goal:** Verify an exception can be deleted and is removed from the table.

**Priority:** High | **Type:** Functional  
**Parameters:** `{EXCEPTION_ID}`

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Magazine Exceptions | Table is displayed |
| 2 | Locate row with `{EXCEPTION_ID}` | Row is visible |
| 3 | Click the **Delete** button in that row | Confirmation prompt/dialog appears (if applicable) |
| 4 | Confirm deletion | Confirmation accepted |
| 5 | Verify the row with `{EXCEPTION_ID}` is **no longer** present in the table | Row absent from table |
| 6 | (Optional) Refresh the page | Row still absent after refresh |

**Pass Criteria:** Deleted row is not found in table after deletion.

> ⚠️ **Note for automation:** This test case should re-create the precondition entity via API before running, to ensure idempotency.

---

## TC-008 — Delete confirmation can be cancelled

**Goal:** Verify that cancelling a delete confirmation leaves the entity intact.

**Priority:** Medium | **Type:** Functional

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Magazine Exceptions | Table displayed |
| 2 | Click **Delete** on target row | Confirmation prompt appears |
| 3 | Click **Cancel** / dismiss prompt | Prompt closes |
| 4 | Verify the row is still present | Entity not deleted |

---

## TC-009 — Table pagination

**Goal:** Verify the table paginates correctly when there are more rows than the page size.

**Priority:** Medium | **Type:** Functional

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Magazine Exceptions with > 1 page of data | First page is shown |
| 2 | Click **Next page** | Second page loads with next set of rows |
| 3 | Click **Previous page** | Returns to first page |
| 4 | Navigate to last page | No "next" navigation available |

---

## TC-010 — Combined filter/sort: ID + Name

**Goal:** Verify that applying multiple filters simultaneously narrows results correctly.

**Priority:** Medium | **Type:** Functional  
**Parameters:** `{EXCEPTION_ID}`, `{EXCEPTION_NAME}`

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Magazine Exceptions | Table displayed |
| 2 | Filter by `{EXCEPTION_ID}` | Results narrowed |
| 3 | Additionally filter by `{EXCEPTION_NAME}` | Results narrowed further |
| 4 | Verify only rows matching both criteria are shown | No unrelated rows visible |

---

## TC-011 — Empty state when no exceptions exist

**Goal:** Verify the table displays an appropriate message when no exceptions are present.

**Priority:** Low | **Type:** Edge Case

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Ensure no exceptions exist (or apply a filter that yields no results) | — |
| 2 | Navigate to / refresh Magazine Exceptions | Table shows an empty state message (e.g., "No results found") |
| 3 | Verify no error or blank white space | UI handles empty state gracefully |

---

## TC-012 — Page access control (negative)

**Goal:** Verify that a user without permissions cannot access Magazine Exceptions.

**Priority:** High | **Type:** Security / Access Control

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Log in as a user **without** Data Governance permissions | — |
| 2 | Attempt to navigate to **Data Governance → Magazine Exceptions** | Access denied message shown **or** menu item not visible |
| 3 | Attempt direct URL navigation to the page | Redirected to login or unauthorised page |

---

## TC-013 — Session expiry handling

**Goal:** Verify the application handles session expiry gracefully during an edit operation.

**Priority:** Medium | **Type:** Edge Case

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open Edit dialog for an exception | Dialog open |
| 2 | Allow session to expire (or manually invalidate token) | — |
| 3 | Click **Save** | User is redirected to login page **or** shown a session-expired error |
| 4 | Verify no partial data corruption | Entity state unchanged after re-login |

---

## Automation Notes

| Consideration | Recommendation |
|---------------|----------------|
| **Test data isolation** | Create entities via API in `@BeforeEach` / `beforeEach`; delete via API in `@AfterEach` |
| **Parametrised tests** | TC-002, TC-003, TC-004, TC-005 are prime candidates for `@ParameterizedTest` (JUnit 5) or `test.each` (Playwright/Jest) |
| **Toast assertion** | Poll `div#toast` with a reasonable timeout (e.g. 3 s) rather than hard-coded sleep |
| **Selector strategy** | Prefer `data-testid` attributes; avoid fragile CSS class selectors where possible |
| **Page Object Model** | Introduce `MagazineExceptionsPage`, `EditExceptionDialog` POMs before automating |
| **Smoke gate** | TC-001 should run as a prerequisite; skip suite on failure |
