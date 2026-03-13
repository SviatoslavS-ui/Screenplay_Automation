# FLGroup Frontline Test Automation — Copilot Instructions

## Project context
- **Solution:** FrontlineTests (3 projects)
- **Stack:** .NET 9 · C# 14 · NUnit 4 · Microsoft Playwright
- **Pattern:** Screenplay Pattern (BDD, actor-centric)
- **Azure DevOps:** https://dev.azure.com/flgroup/FLGroupTest/_git/FLGroupTest
- **Full architecture reference:** `Frontline.Tests.Core/SCREENPLAY_ARCHITECTURE.md`

## Project structure
```
Frontline.Tests.Core/          ← framework library (never put tests here)
FrontlineTests.Common/         ← ScreenplayTestBase, shared test infrastructure
FrontlineTests.BusinessShells/ ← test fixtures (one per feature area)
```

## Screenplay layer rules — always follow these
| Layer | Where | Rule |
|-------|-------|------|
| Test body | `BusinessShells/` | Only Given/When/Then using Tasks, Questions, and constants. No selectors, no Playwright calls, no magic strings. |
| Composite Task | `Tasks/` | Orchestrates interactions/sub-tasks for one user goal. Implements `ITask`. |
| Interaction | `Interactions/` | One Playwright call. Implements `IInteraction`. |
| Question | `Questions/` | Reads state from page. Implements `IQuestion<T>`. |
| Ability | `Abilities/` | Wraps external system (browser, API). Implements `IAbility`. |
| Targets | `Targets/` | All CSS/Playwright selectors as `const string`. Never inline selectors in tests or tasks. |
| TestData | `TestData/` | All expected values as `const string`. Never inline expected values in tests. |
| Config | `Configuration/AppConfiguration.cs` | URLs and browser flags only. |

## Naming conventions
- Tasks: verb + noun describing user goal — `OpenMagazineExceptionsModule`, `LoginAs`, `SubmitForm`
- Interactions: verb only — `Click`, `Fill`, `WaitForElement`
- Questions: readable as a question — `IsVisible`, `TextOf`, `PageTitle`, `HasText`
- Targets class: `{Feature}PageTargets`
- TestData class: `{Feature}TestData`
- Test fixture: `{Feature}Tests : ScreenplayTestBase`

## Key implementation facts
- `BrowserAbility.InitializeAsync` takes two params: `BrowserTypeLaunchOptions` + `BrowserNewContextOptions`
- Maximized browser = `--start-maximized` arg + `ViewportSize.NoViewport` together (Playwright requires both)
- `ScreenplayTestBase` creates `"User"` actor automatically — override `InitializeActorsAsync()` for custom setup
- `TearDown` uses `TryGetAbility` (safe) not `UsesAbility` (throws) — must not mask test failure exceptions
- Syncfusion EJ2 Grid renders two tables: `#MagGrid_header_table` (header) and `#MagGrid_content_table` (data rows)
- `WaitForElement` must be placed **after** the action that triggers the element, not before

## When adding a new feature area
1. `{Feature}PageTargets.cs` in `Targets/`
2. `{Feature}TestData.cs` in `TestData/`
3. Composite task(s) in `Tasks/`
4. `{Feature}Tests.cs` in `FrontlineTests.BusinessShells/` inheriting `ScreenplayTestBase`

## Code comment style
- XML doc comments (`<summary>`) must be **single-line** and state a fact, not explain what the reader can see
- No filler phrases: never use "Represents", "This class", "Used to", "Provides"
- Omit comments entirely when the name is self-explanatory
- Acceptable: `/// <summary>Frontline home portal. Title: "FLGroup Apps".</summary>`
- Not acceptable: `/// <summary>\n/// Base URL for the Frontline Applications test environment (internal system).\n/// </summary>`

---

# Blazor + Playwright + Screenplay Pattern — Coding Agent Reference

## Critical Context: Blazor Server Architecture

The application under test is **Blazor Server** (SignalR-based), **not** traditional HTTP request/response web app. This fundamentally changes how Playwright interacts with the UI:

- DOM updates arrive as **binary diff patches** over persistent **WebSocket**, not page navigations
- `WaitForNavigationAsync()` is **useless** — client-side routing never fires navigation events
- `WaitForLoadStateAsync(LoadState.NetworkIdle)` is **unreliable** — SignalR is persistent, network never truly goes idle
- Page URLs may **not** change between "page" transitions
- Full page reloads can happen silently if SignalR circuit drops and reconnects
- Component rendering is **asynchronous** — DOM may be visible but contain stale data

---

## Anti-Patterns — Never Use These

```csharp
// NEVER — does not map to Blazor's rendering model
await Page.WaitForNavigationAsync();
await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

// NEVER — masks timing issues, causes flakiness in CI
await Page.WaitForTimeoutAsync(2000);

// NEVER — stale element after Blazor re-render
var element = await Page.QuerySelectorAsync("td");
await element.ClickAsync(); // may be detached before click reaches browser

// AVOID — tied to CSS library internals, breaks on framework updates
Page.Locator(".mud-table-cell:nth-child(3)");
Page.Locator("//table/tbody/tr[1]/td[2]");
```

---

## Preferred Locator Strategies (In Priority Order)

### 1. Semantic / ARIA locators — most stable

```csharp
// By role — survives CSS class and DOM structure changes
Page.GetByRole(AriaRole.Button, new() { Name = "Save" })
Page.GetByRole(AriaRole.Columnheader, new() { Name = "Amount" })
Page.GetByRole(AriaRole.Row, new() { Name = "Invoice #123" })
Page.GetByRole(AriaRole.Link, new() { Name = "Dashboard" })

// By label — tied to accessible form label
Page.GetByLabel("Start Date")
Page.GetByLabel("Filter by category")

// By user-visible text
Page.GetByText("No records found")
Page.GetByText("Saved successfully")
```

### 2. data-testid attributes — if devs implement them (request these)

```csharp
Page.GetByTestId("data-grid-amount-column")
Page.GetByTestId("sort-header-date")
Page.GetByTestId("pagination-next")
```

### 3. Stable structural selectors — last resort only

```csharp
// Blazor infrastructure
Page.Locator("#blazor-error-ui")                  // Blazor error display
Page.Locator("#components-reconnect-modal")       // Reconnect notification

// Aria attributes on headers (reliable sort state signal)
Page.Locator("th[aria-sort='ascending']")
Page.Locator("[aria-sort]")
```

### 4. Locator chaining for complex grids

```csharp
// Find row by text, then get specific cell
var row = Page.GetByRole(AriaRole.Row).Filter(new() { HasText = "Invoice #123" });
var statusCell = row.GetByRole(AriaRole.Cell).Nth(3);

// Filter by child element presence
Page.Locator("tr", new() { Has = Page.Locator("td", new() { HasText = "Pending" }) })
```

---

## Blazor-Specific Waiting Strategies

### Always guard with Blazor health checks

```csharp
// After navigation and major actions — verify circuit is healthy
await Expect(Page.Locator("#blazor-error-ui"))
    .ToBeHiddenAsync(new() { Timeout = 5_000 });

await Expect(Page.Locator("#components-reconnect-modal"))
    .ToBeHiddenAsync(new() { Timeout = 15_000 });
```

### Wait for outcome, never for time

```csharp
// WRONG
await Page.ClickAsync("button:has-text('Save')");
await Page.WaitForTimeoutAsync(3000);

// CORRECT — assert on expected result
await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();
await Expect(Page.GetByText("Record saved successfully"))
    .ToBeVisibleAsync(new() { Timeout = 10_000 });
```

### Wait for element state before interacting

```csharp
// Ensure element is visible and attached before action
await Page.Locator("button#submit")
    .WaitForAsync(new() { State = WaitForSelectorState.Visible });

// Wait for button to be enabled, not just visible
await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Export" }))
    .ToBeEnabledAsync(new() { Timeout = 10_000 });
```

### Loading indicator synchronisation

```csharp
// Trigger action, then wait for loading overlay to disappear
await Page.GetByRole(AriaRole.Columnheader, new() { Name = "Date" }).ClickAsync();
await Expect(Page.Locator(".loading-overlay, .mud-overlay, [data-loading='true']"))
    .ToBeHiddenAsync(new() { Timeout = 10_000 });
```

---

## Table / Data Grid Patterns

### Sorting — the correct approach

```csharp
// Step 1: Capture pre-sort state BEFORE clicking
var firstCell = Page.Locator("tbody tr:first-child td:first-child");
var valueBefore = await firstCell.TextContentAsync();

// Step 2: Click sort header
await Page.GetByRole(AriaRole.Columnheader, new() { Name = "Amount" }).ClickAsync();

// Step 3: Wait for aria-sort attribute — confirms server-side sort applied
await Expect(Page.GetByRole(AriaRole.Columnheader, new() { Name = "Amount" }))
    .ToHaveAttributeAsync("aria-sort", "ascending", new() { Timeout = 10_000 });

// Step 4: Belt-and-braces — confirm data actually changed
await Expect(firstCell).Not.ToHaveTextAsync(valueBefore!);
```

### Wait for table data to be present

```csharp
// Wait for minimum row count (guards against skeleton/empty states)
await Expect(Page.Locator("tbody tr:not(.skeleton):not(.loading-row)"))
    .ToHaveCountAsync(expectedCount, new() { Timeout = 10_000 });

// Or wait for first data row to render
await Page.Locator("tbody tr").First
    .WaitForAsync(new() { State = WaitForSelectorState.Visible });
```

### Reading table data safely

```csharp
// Read all cells in a column
var cells = Page.Locator("tbody tr td:nth-child(2)");
var values = await cells.AllTextContentsAsync();

// Read specific row by content
var targetRow = Page.GetByRole(AriaRole.Row).Filter(new() { HasText = "INV-001" });
var amount = await targetRow.GetByRole(AriaRole.Cell).Nth(2).TextContentAsync();
```

### Virtual scrolling / large datasets

```csharp
// Virtual grids recycle DOM nodes — never assume all rows are in DOM
// Scroll to target, then read visible rows only
await Page.Locator(".grid-viewport").EvaluateAsync("el => el.scrollTop = 0");
await Page.Locator("tbody tr").First
    .WaitForAsync(new() { State = WaitForSelectorState.Visible });
```

---

## Navigation in Blazor (Client-Side Routing)

```csharp
// WRONG — WaitForNavigationAsync never fires for Blazor SPA routing
await Page.GetByRole(AriaRole.Link, new() { Name = "Reports" }).ClickAsync();
await Page.WaitForNavigationAsync(); // USELESS

// CORRECT — wait for content unique to destination page
await Page.GetByRole(AriaRole.Link, new() { Name = "Reports" }).ClickAsync();
await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Reports" }))
    .ToBeVisibleAsync(new() { Timeout = 10_000 });

// OR wait for URL pattern change (if route updates the URL)
await Page.WaitForURLAsync("**/reports**", new() { Timeout = 10_000 });
```

---

## Blazor Readiness Gate Pattern (If Implemented by Dev Team)

**Only use if the app implements this:** A `<div class="control-container" pageloaded="{guid}">` element in the page layout with a fresh GUID set in `OnAfterRenderAsync`.

```csharp
// Step 1: Snapshot current pageloaded value BEFORE triggering navigation
var pageBefore = await Page.GetAttributeAsync(".control-container[pageloaded]", "pageloaded");

// Step 2: Trigger SPA navigation IMMEDIATELY after snapshot — no awaits between
await Page.GetByRole(AriaRole.Link, new() { Name = "MagazineExceptions" }).ClickAsync();

// Step 3: Wait for pageloaded GUID to change — confirms OnAfterRenderAsync fired
await Page.WaitForFunctionAsync(
    @$"document.querySelector('.control-container')?.getAttribute('pageloaded') !== '{pageBefore}'",
    new PageWaitForFunctionOptions { Timeout = 10_000 }
);

// Step 4: Safe to interact — all component lifecycle hooks completed
```

**⚠️ Critical discipline:** Snapshot and triggering action must be consecutive with no awaits between them, or Blazor may update the attribute before you've captured the old value.

---

## Blazor Circuit Health Guards

```csharp
// Reusable health check — call after navigation and major actions
public static async Task AssertBlazorHealthy(IPage page)
{
    await Expect(page.Locator("#blazor-error-ui"))
        .ToBeHiddenAsync(new() { Timeout = 5_000 });
    await Expect(page.Locator("#components-reconnect-modal"))
        .ToBeHiddenAsync(new() { Timeout = 15_000 });
}

// Subscribe to browser console to catch SignalR errors early
Page.Console += (_, msg) =>
{
    if (msg.Type == "error")
        Console.WriteLine($"[BROWSER ERROR] {msg.Text}");
};
```

---

## Debugging Flaky Tests

```csharp
// Pause test execution and open Playwright Inspector (headed mode only)
await Page.PauseAsync();

// Capture screenshot on failure in NUnit teardown
[TearDown]
public async Task TearDown()
{
    if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
    {
        await Page.ScreenshotAsync(new()
        {
            Path = $"screenshots/{TestContext.CurrentContext.Test.Name}.png",
            FullPage = true
        });
    }
}

// Record Playwright trace for post-mortem analysis
await Context.Tracing.StartAsync(new() { Screenshots = true, Snapshots = true });
// ... test runs ...
await Context.Tracing.StopAsync(new() { Path = "trace.zip" });
// Open with: playwright show-trace trace.zip
```

---

## NUnit Parallelisation Warning

```csharp
// Blazor Server SignalR connections are expensive server-side resources
// DO NOT run too many parallel workers against the same instance

[assembly: Parallelizable(ParallelScope.Fixtures)] // NOT ParallelScope.All
[assembly: LevelOfParallelism(2)] // Keep low — max 2–3 for Blazor Server
```

---

## Quick Reference — Do / Don't

| Scenario | DON'T | DO |
|---|---|---|
| Wait after navigation | `WaitForNavigationAsync()` | `Expect(heading).ToBeVisibleAsync()` + `WaitForURLAsync()` |
| Wait for data load | `WaitForTimeoutAsync(3000)` | `Expect(rows).ToHaveCountAsync(n)` |
| Wait after sort | `WaitForTimeoutAsync(1000)` | `Expect(header).ToHaveAttributeAsync("aria-sort")` |
| Select elements | CSS class + child selectors | `GetByRole`, `GetByLabel`, `GetByText` |
| Detect page change | `WaitForNavigationAsync()` | `WaitForURLAsync("**/path**")` or wait for unique heading |
| Detect Blazor errors | Nothing | Always check `#blazor-error-ui` is hidden |
| Detect reconnect | Nothing | Always wait for `#components-reconnect-modal` hidden |
| Read table cells | Loop with `QuerySelectorAllAsync` | `AllTextContentsAsync()` on Locator |
| Verify sort | Check row count | Check `aria-sort` + first cell value changed |
| Parallel tests | Many workers | Max 2–3 workers |
