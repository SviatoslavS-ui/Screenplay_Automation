# Screenplay Pattern Framework — Architecture Guide

> **Project:** FLGroup Frontline Test Automation
> **Framework:** .NET 9 · NUnit 4 · Microsoft Playwright
> **Pattern:** Screenplay (BDD actor-centric)
> **Repository:** https://dev.azure.com/flgroup/FLGroupTest/_git/FLGroupTest

---

## Table of Contents

1. [What is the Screenplay Pattern?](#1-what-is-the-screenplay-pattern)
2. [Solution Structure](#2-solution-structure)
3. [Core Abstractions](#3-core-abstractions)
4. [Layer Hierarchy](#4-layer-hierarchy)
5. [Composite Task Pattern](#5-composite-task-pattern---key-concept)
6. [Supporting Infrastructure](#6-supporting-infrastructure)
7. [Real-World Example: TC_001](#7-real-world-example-tc_001)
8. [Directory Structure](#8-directory-structure)
9. [Design Decisions Log](#9-design-decisions-log)
10. [Adding New Tests — Checklist](#10-adding-new-tests--checklist)

---

## 1. What is the Screenplay Pattern?

The Screenplay Pattern is a BDD approach to test automation built around **actors** — real people performing tasks on the system under test. The central principle:

> *Tests should read like a user story, not like a Selenium/Playwright script.*

Three pillars:

| Pillar | Question answered | Example |
|--------|-------------------|---------|
| **Tasks** | What does the user *do*? | `OpenMagazineExceptionsModule` |
| **Questions** | What does the user *see/know*? | `IsVisible`, `PageTitle` |
| **Abilities** | What can the user *use*? | `BrowserAbility` |

---

## 2. Solution Structure

```
FrontlineTests/
├── Frontline.Tests.Core/          ← Framework library (abstractions + implementations)
│   └── Screenplay/
│       ├── Core/                  ← Interfaces and Actor class
│       ├── Abilities/             ← BrowserAbility (Playwright wrapper)
│       ├── Tasks/                 ← Composite and atomic tasks
│       ├── Interactions/          ← Atomic browser operations
│       ├── Questions/             ← Information retrieval / assertions
│       ├── Targets/               ← UI locators (CSS/Playwright selectors)
│       ├── TestData/              ← Expected values and test constants
│       ├── Configuration/         ← AppConfiguration (URLs, browser flags)
│       └── Infrastructure/        ← ActorLibrary
│
├── FrontlineTests.Common/         ← Shared test infrastructure
│   └── ScreenplayTestBase.cs      ← Base NUnit fixture (setup / teardown)
│
└── FrontlineTests.BusinessShells/ ← Actual test fixtures (one per feature)
    └── MagazineExceptionsTests.cs
```

**Dependency direction:**
```
BusinessShells  →  Common  →  Tests.Core
```

---

## 3. Core Abstractions

### `IAbility` — What an actor can use
```csharp
public interface IAbility
{
    string AbilityName { get; }
}
```
Abilities are capabilities granted to an actor. Currently implemented:

| Class | Description |
|-------|-------------|
| `BrowserAbility` | Wraps Playwright `IPage`, `IBrowserContext`, `IBrowser`. Initialized with `BrowserTypeLaunchOptions` + `BrowserNewContextOptions`. Supports `--start-maximized` via Chromium args + `ViewportSize.NoViewport`. |

---

### `ITask` — What an actor does
```csharp
public interface ITask
{
    string TaskDescription { get; }
    Task PerformAsync(Actor actor);
}
```
Tasks represent **user-level actions**. They are either **atomic** (wrap one interaction) or **composite** (orchestrate multiple interactions/tasks). See [Section 5](#5-composite-task-pattern---key-concept) for full details.

Currently implemented:

| Class | Type | Description |
|-------|------|-------------|
| `NavigateTo` | Atomic | Navigate browser to a URL |
| `OpenMagazineExceptionsModule` | **Composite** | Full 7-step flow from home page to grid |

---

### `IInteraction` — Atomic browser operations
```csharp
public interface IInteraction
{
    string InteractionDescription { get; }
    Task PerformAsync(Actor actor);
}
```
Interactions are the **lowest-level** building blocks. They call Playwright directly.

| Class | Playwright call |
|-------|----------------|
| `Click(selector)` | `Page.ClickAsync(selector)` |
| `Fill(selector, text)` | `Page.FillAsync(selector, text)` |
| `WaitForElement(selector, timeoutMs)` | `Page.WaitForSelectorAsync(selector)` |

---

### `IQuestion<TAnswer>` — What an actor can observe
```csharp
public interface IQuestion<TAnswer>
{
    Task<TAnswer> AnswerAsync(Actor actor);
}
```
Questions retrieve state from the application for use in assertions.

| Class | Returns | Description |
|-------|---------|-------------|
| `IsVisible(selector)` | `bool` | Is element currently visible? |
| `TextOf(selector)` | `string?` | Full text content of an element |
| `HasText(selector, expected)` | `bool` | Does element text match exactly? |
| `PageTitle()` | `string` | Current browser tab title |

---

### `Actor` — The central character
```csharp
var user = new Actor("User");
user.Can(new BrowserAbility());

await user.Performs(new NavigateTo(AppConfiguration.BaseUrl));
await user.Performs(new OpenMagazineExceptionsModule());
var title = await user.Asks(new PageTitle());
```

Key methods:

| Method | Purpose |
|--------|---------|
| `Can(IAbility)` | Grant an ability (throws if already has it) |
| `UsesAbility<T>(name)` | Retrieve a typed ability (throws if missing) |
| `TryGetAbility<T>(name, out T)` | Safe ability retrieval (no throw) |
| `Performs(ITask)` | Execute a task |
| `Performs(IInteraction)` | Execute an atomic interaction |
| `Asks<T>(IQuestion<T>)` | Ask a question and get a typed answer |

---

## 4. Layer Hierarchy

```
┌─────────────────────────────────────────────────────┐
│  TEST BODY  (BusinessShells)                        │
│  Given / When / Then — business language only       │
│  user.Performs(new OpenMagazineExceptionsModule())  │
└───────────────────┬─────────────────────────────────┘
                    │  calls
┌───────────────────▼─────────────────────────────────┐
│  COMPOSITE TASKS  (Tasks/)                          │
│  Orchestrate multiple interactions / sub-tasks      │
│  OpenMagazineExceptionsModule                       │
└───────────────────┬─────────────────────────────────┘
                    │  calls
┌───────────────────▼─────────────────────────────────┐
│  ATOMIC INTERACTIONS  (Interactions/)               │
│  One Playwright call each                           │
│  Click · Fill · WaitForElement                      │
└───────────────────┬─────────────────────────────────┘
                    │  uses
┌───────────────────▼─────────────────────────────────┐
│  ABILITIES  (Abilities/)                            │
│  BrowserAbility → IPage, IBrowser, IBrowserContext  │
└─────────────────────────────────────────────────────┘

QUESTIONS flow independently (also use Abilities):
user.Asks(new IsVisible(target))  →  BrowserAbility.Page.IsVisibleAsync()
```

---

## 5. Composite Task Pattern — Key Concept

### What it is

A **composite task** is a class implementing `ITask` whose `PerformAsync` body calls multiple interactions (or other tasks) in sequence to represent one **complete user goal**.

### Why it is NOT just method extraction

This is the most important distinction in the Screenplay pattern.

| | `private async Task OpenExceptions()` in test class | `OpenMagazineExceptionsModule : ITask` |
|---|---|---|
| **Scope** | Locked to one test class | Available to every actor in every fixture |
| **Actor model** | Bypasses it — Playwright called directly | Actor carries it through — abilities flow via `actor` parameter |
| **Reusable** | No | Yes — any test says `user.Performs(new OpenMagazineExceptionsModule())` |
| **Composable** | No | Tasks can call other tasks |
| **Discoverable** | Buried inside a test file | Named domain object in `/Tasks` |
| **Describable** | No | `TaskDescription` property for reporting |

### Real implementation: `OpenMagazineExceptionsModule`

This task encapsulates the **full 7-step navigation** from the Frontline home page to the Magazine Exceptions grid. The test body knows none of these steps:

```csharp
public async Task PerformAsync(Actor actor)
{
    // Step 1 — click the app tile on the home page
    await new Click(MagazineExceptionsPageTargets.MagazineExceptionsAppButton).PerformAsync(actor);

    // Step 2 — wait for the app shell to load (burger menu is the readiness signal)
    await new WaitForElement(MagazineExceptionsPageTargets.BurgerMenuButton).PerformAsync(actor);

    // Step 3 — open the navigation drawer
    await new Click(MagazineExceptionsPageTargets.BurgerMenuButton).PerformAsync(actor);

    // Step 4 — wait for the nav item to be rendered
    await new WaitForElement(MagazineExceptionsPageTargets.MagazineExceptionsMenuItem).PerformAsync(actor);

    // Step 5 — click the nav item → triggers grid page navigation
    await new Click(MagazineExceptionsPageTargets.MagazineExceptionsMenuItem).PerformAsync(actor);

    // Steps 6–7 — wait for both Syncfusion EJ2 grid tables (header + data)
    await new WaitForElement(MagazineExceptionsPageTargets.ExceptionsTable).PerformAsync(actor);
    await new WaitForElement(MagazineExceptionsPageTargets.ExceptionsContentTable).PerformAsync(actor);
}
```

> **Key rule:** `WaitForElement` calls are placed *after the action that triggers the element to appear*, not at the start of the task. In this case the grid tables only appear after Step 5, not after Step 1.

### Composing tasks from tasks

Because every task receives the `actor`, tasks are fully composable:

```csharp
// Future example: a task that composes two existing tasks
public class LoginAndOpenMagazineExceptions : ITask
{
    public string TaskDescription => "Login and open Magazine Exceptions";

    public async Task PerformAsync(Actor actor)
    {
        await new Login(Credentials.Default).PerformAsync(actor);
        await new OpenMagazineExceptionsModule().PerformAsync(actor); // reused, unchanged
    }
}
```

---

## 6. Supporting Infrastructure

### `AppConfiguration` — Centralized settings
```csharp
public static class AppConfiguration
{
    public const string BaseUrl              = "https://dotnettest.flgroup.co.uk:10143/";
    public const string MagazineExceptionsUrl = BaseUrl + "magazine-exceptions";
    public const bool   RunHeadless          = false;
    public const bool   StartMaximized       = true;
}
```
All URLs and browser flags live here. Never hardcode URLs or flags in tests.

---

### `MagazineExceptionsPageTargets` — UI Locators
All CSS/Playwright selectors for the Magazine Exceptions feature are centralized here. If the UI changes, only this file needs updating.

```
Navigation:
  MagazineExceptionsAppButton  →  button:has-text("Magazine Exceptions")
  BurgerMenuButton             →  button.navbar-toggle
  MagazineExceptionsMenuItem   →  a.e-menu-url[href='/MagazineExceptions']
                                  (href-based: stable against text changes)

Grid (Syncfusion EJ2, id="MagGrid"):
  ExceptionsTable              →  #MagGrid_header_table   (header + filter bar)
  ExceptionsContentTable       →  #MagGrid_content_table  (data rows)
  TableRows                    →  #MagGrid_content_table tbody tr.e-row

Column headers (by data-colindex — survives reorder):
  MagIdColumnHeader            →  th[data-colindex='0'] .e-headertext
  MagazineNameColumnHeader     →  th[data-colindex='1'] .e-headertext
  CompanyColumnHeader          →  th[data-colindex='2'] .e-headertext
  ExceptionReasonColumnHeader  →  th[data-colindex='3'] .e-headertext
  ExceptionEndDateColumnHeader →  th[data-colindex='4'] .e-headertext
  ...

Filter inputs (by field-name id):
  MagIdFilterInput             →  #MagazineId_filterBarcell
  MagazineNameFilterInput      →  #MagazineIdAsString_filterBarcell
  CompanyFilterInput           →  #company_filterBarcell
  ...
```

---

### `MagazineExceptionsTestData` — Expected values
```csharp
public static class MagazineExceptionsTestData
{
    public const string ExpectedHomePageTitle = "FLGroup Apps";
    public const string ExpectedPageHeader    = "Magazine Exceptions";
}
```
No magic strings inside test methods. All expected values are named constants here.

---

### `ScreenplayTestBase` — Base NUnit fixture
Lives in `FrontlineTests.Common`. Every test fixture inherits from this.

**Responsibilities:**
- Creates and owns the `ActorLibrary` per test
- Creates `BrowserAbility`, initializes it with `AppConfiguration` flags, grants it to the `"User"` actor
- Tears down all actor browser instances safely after each test via `TryGetAbility` (no throw on missing ability)

**Browser launch flags applied:**
- `Headless = AppConfiguration.RunHeadless` (currently `false`)
- `Args = ["--start-maximized"]` when `AppConfiguration.StartMaximized` is `true`
- `ViewportSize = ViewportSize.NoViewport` — disables Playwright's default fixed 1280×720 viewport so the maximized window size takes effect

**Customization:**
Override `InitializeActorsAsync()` in a fixture to set up multiple actors, different browsers, or custom context options:
```csharp
protected override async Task InitializeActorsAsync()
{
    // custom setup for fixtures that need it
}
```

---

### `ActorLibrary` — Actor registry
```csharp
var library = new ActorLibrary();
var user = library.GetActor("User");    // get-or-create
var admin = library.GetActor("Admin");  // second actor for multi-user scenarios
```
`GetAllActors()` is used in teardown to clean up every actor's browser regardless of how many were created.

---

## 7. Real-World Example: TC_001

```csharp
[Test]
[Category("Smoke")]
public async Task TC_001_NavigateToMagazineExceptionsPage()
{
    var user = ActorLibrary.GetActor("User");

    // Given: User navigates to the Frontline home page
    await user.Performs(new NavigateTo(AppConfiguration.BaseUrl));

    // Then: The home page is loaded correctly
    var pageTitle = await user.Asks(new PageTitle());
    Assert.That(pageTitle, Does.Contain(MagazineExceptionsTestData.ExpectedHomePageTitle),
        $"Page title should contain '{MagazineExceptionsTestData.ExpectedHomePageTitle}'");

    // When: User opens the Magazine Exceptions module
    await user.Performs(new OpenMagazineExceptionsModule());

    // Then: The exceptions table is visible
    var tableIsVisible = await user.Asks(new IsVisible(MagazineExceptionsPageTargets.ExceptionsTable));
    Assert.That(tableIsVisible, Is.True,
        "Exceptions table should be visible after opening the module");
}
```

**What the test body does NOT contain:**
- ❌ Any CSS selector
- ❌ Any Playwright API call
- ❌ Any `await Page.ClickAsync(...)` or `await Page.WaitForSelectorAsync(...)`
- ❌ Any setup or teardown logic
- ❌ Any hardcoded string expected values

Everything is delegated to the appropriate layer.

---

## 8. Directory Structure

```
Frontline.Tests.Core/
├── SCREENPLAY_ARCHITECTURE.md         ← this file
└── Screenplay/
    ├── Core/
    │   ├── Actor.cs                   ← central actor class
    │   ├── IAbility.cs                ← ability interface
    │   ├── ITask.cs                   ← task interface
    │   ├── IInteraction.cs            ← interaction interface
    │   ├── IQuestion.cs               ← question interface
    │   └── ScreenplayException.cs     ← custom exception
    ├── Abilities/
    │   └── BrowserAbility.cs          ← Playwright browser/context/page lifecycle
    ├── Tasks/
    │   ├── NavigateTo.cs              ← atomic: navigate to URL
    │   └── OpenMagazineExceptionsModule.cs  ← composite: full grid navigation flow
    ├── Interactions/
    │   ├── Click.cs                   ← Page.ClickAsync
    │   ├── Fill.cs                    ← Page.FillAsync
    │   └── WaitForElement.cs          ← Page.WaitForSelectorAsync
    ├── Questions/
    │   ├── IsVisible.cs               ← Page.IsVisibleAsync → bool
    │   ├── TextOf.cs                  ← Page.TextContentAsync → string?
    │   ├── HasText.cs                 ← text equality check → bool
    │   └── PageTitle.cs               ← Page.TitleAsync → string
    ├── Targets/
    │   └── MagazineExceptionsPageTargets.cs  ← all CSS selectors for the feature
    ├── TestData/
    │   └── MagazineExceptionsTestData.cs     ← expected values / constants
    ├── Configuration/
    │   └── AppConfiguration.cs        ← URLs, headless flag, maximized flag
    └── Infrastructure/
        └── ActorLibrary.cs            ← actor get-or-create registry

FrontlineTests.Common/
└── ScreenplayTestBase.cs              ← abstract NUnit base fixture

FrontlineTests.BusinessShells/
└── MagazineExceptionsTests.cs         ← TC_001 and future Magazine Exceptions tests
```

---

## 9. Design Decisions Log

| Decision | Rationale |
|----------|-----------|
| `IAbility` as interface, not abstract class | Allows abilities to inherit from other base classes if needed |
| `BrowserAbility.InitializeAsync` takes two separate option objects | `BrowserTypeLaunchOptions` (process) and `BrowserNewContextOptions` (viewport/locale) have different scopes and lifetimes. Merging them would hide intent. |
| `--start-maximized` + `ViewportSize.NoViewport` together | `--start-maximized` sets the OS window size; without `NoViewport`, Playwright overrides it with a fixed 1280×720 CSS viewport. Both are needed. |
| `MagazineExceptionsMenuItem` uses `href` not text | `a.e-menu-url[href='/MagazineExceptions']` survives display text changes; `:has-text()` would break on any wording update. |
| `ExceptionsTable` uses `#MagGrid_header_table` not `table` | Generic `table` matches every table element on the page. ID-based selector is unambiguous and reflects the actual Syncfusion EJ2 grid structure. |
| Syncfusion grid split into two targets | EJ2 Grid renders a header table (frozen headers + filter bar) and a separate content table (data rows). Waiting only on the header proves grid chrome loaded, not that data loaded. |
| `WaitForElement` placed after the action that triggers it | Playwright waits are assertions about post-action state. Placing them before the triggering action would wait for an element that may already be stale or for the wrong reason. |
| `ScreenplayTestBase` uses `TryGetAbility` in teardown | `UsesAbility` throws if the ability is missing. If a test fails during setup before the ability is granted, teardown must not throw a second exception that masks the original failure. |
| Test data extracted to `MagazineExceptionsTestData` | Magic strings in test methods make grep-based analysis impossible and break silently when expected values change. |

---

## 10. Adding New Tests — Checklist

**For a new test in `MagazineExceptionsTests`:**
- [ ] Add any missing selectors to `MagazineExceptionsPageTargets`
- [ ] Add any expected values to `MagazineExceptionsTestData`
- [ ] If the navigation path is new, create a composite Task in `/Tasks`
- [ ] Write test method using only Tasks, Questions, and constants — no raw selectors

**For a new feature area (e.g. `SubscriptionsTests`):**
- [ ] Create `SubscriptionsPageTargets.cs` in `/Targets`
- [ ] Create `SubscriptionsTestData.cs` in `/TestData`
- [ ] Create composite task(s) in `/Tasks`
- [ ] Create `SubscriptionsTests.cs` in `FrontlineTests.BusinessShells`, inheriting `ScreenplayTestBase`
- [ ] If setup needs multiple actors or a different browser, override `InitializeActorsAsync()`

   - `ScreenplayTestBase` - Common setup/teardown
   - Browser configuration management
   - Actor library initialization

5. **Reporting & Logging**
   - Action logging (what the actor did)
   - Screenshot capture on failure
   - Performance metrics

## Architecture Benefits

✅ **Readable**: Non-technical stakeholders can follow test logic
✅ **Maintainable**: UI changes are isolated to specific classes
✅ **Scalable**: Easy to add new abilities, tasks, and questions
✅ **Reusable**: Tasks and questions used across multiple tests
✅ **Flexible**: Supports multi-actor scenarios and complex workflows
✅ **Testable**: Each component can be unit tested independently
