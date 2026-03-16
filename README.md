# FrontlineTests — Automated Regression Suite

End-to-end regression suite for **FLGroup Frontline Applications** — a Blazor Server platform with Syncfusion EJ2 data grids.

---

## Technology stack

| Component | Technology |
|---|---|
| Language | C# 13 / .NET 9 |
| Test framework | NUnit 4 |
| Browser automation | Microsoft Playwright 1.58 |
| Pattern | Screenplay (actor-centric BDD) |
| Target app | Blazor Server + Syncfusion EJ2 |

---

## Screenplay pattern

Tests are written using the **Screenplay pattern** — an actor-centric approach to BDD that keeps test bodies readable and free of Playwright/selector details.

```
Test body   →   Tasks (composite user flows)
                  →   Interactions (atomic Playwright operations)
                        →   Abilities (browser lifecycle)
            →   Questions (read application state)
```

- **Actor** — represents a user; holds abilities and performs tasks
- **Task** — a named, reusable user-level flow (e.g. `OpenMagazineExceptionsModule`, `AddException`)
- **Interaction** — a single atomic UI operation (e.g. `Click`, `Fill`, `WaitForElement`)
- **Question** — reads application state and returns a typed answer (e.g. `IsVisible`, `TextOf`)
- **Ability** — owns the Playwright browser/page instance (`BrowserAbility`)

A typical test reads like a specification:

```csharp
await user.Performs(new NavigateTo(AppConfiguration.BaseUrl));
await user.Performs(new OpenMagazineExceptionsModule());
await user.Performs(new FilterExceptionsBy(MagIdFilterInput, "12"));
await user.ShouldEventuallyRead(FirstRowIdCell, "12");
```

---

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Access to the FLGroup test environment (VPN or internal network)
- A test user account with permissions to the target application

---

## Getting started

### 1. Clone the repository

```bash
git clone git@github.com:SviatoslavS-ui/Screenplay_Automation.git
cd FrontlineTests
```

### 2. Restore dependencies

```bash
dotnet restore
```

### 3. Install Playwright browsers

Playwright downloads its own browser binaries. Run this once after cloning (and after each Playwright version upgrade):

```bash
dotnet build
pwsh Frontline.Tests.Core/bin/Debug/net9.0/playwright.ps1 install chromium
```

> On Linux/macOS without PowerShell: `dotnet tool install --global Microsoft.Playwright.CLI && playwright install chromium`

### 4. Build

```bash
dotnet build
```

---

## Running tests

### Run the full suite

```bash
dotnet test
```

### Run by category

```bash
dotnet test --filter "Category=Smoke"
dotnet test --filter "Category=Functional"
```

### Run a specific test or module

```bash
dotnet test --filter "FullyQualifiedName~TC_001"
dotnet test --filter "FullyQualifiedName~MagazineExceptions"
```

### Run headless (CI / no display)

```bash
PLAYWRIGHT_HEADLESS=true dotnet test
```

---

## Configuration

All settings are driven by environment variables with sensible local defaults. No code changes needed to retarget environments.

| Variable | Default | Purpose |
|---|---|---|
| `FRONTLINE_BASE_URL` | `https://dotnettest.flgroup.co.uk/` | Home portal URL |
| `FRONTLINE_MAG_EXCEPTIONS_URL` | `https://dotnettest.flgroup.co.uk:10143/` | Magazine Exceptions app URL |
| `PLAYWRIGHT_HEADLESS` | `false` | Set `true` for CI or headless execution |
| `TEST_ARTIFACTS_DIR` | `<bin>/../../../TestResults` | Output directory for traces and screenshots |

---

## Test artifacts

After each run:

- **Traces** — `TestResults/traces/{testName}.zip` — recorded for every test; open with `playwright show-trace <file>`
- **Screenshots** — `TestResults/screenshots/{testName}.png` — captured on failure only

---

## Project structure

```
FrontlineTests.slnx
├── Frontline.Tests.Core/          ← Screenplay framework + page objects
│   └── Screenplay/
│       ├── Core/                  ← Actor, ITask, IInteraction, IQuestion
│       ├── Abilities/             ← BrowserAbility (Playwright lifecycle)
│       ├── Interactions/          ← Reusable atomic UI operations
│       ├── Questions/             ← Reusable state readers
│       ├── Configuration/         ← AppConfiguration (env-var backed)
│       ├── Targets/{Module}/      ← CSS selectors per module
│       ├── Tasks/{Module}/        ← Composite user flows per module
│       └── TestData/{Module}/     ← Test constants per module
│
├── FrontlineTests.Common/         ← NUnit base class, assertion helpers, categories
│
└── FrontlineTests.BusinessShells/ ← Test fixtures
    └── {Module}/                  ← One folder per application module
```

Feature-specific code (Targets, Tasks, TestData, test fixtures) lives in named module sub-folders. Generic framework code (Interactions, Questions, Core) is shared across all modules.

---

## Adding a new module

1. Create sub-folders: `Targets/{Module}/`, `Tasks/{Module}/`, `TestData/{Module}/`, `BusinessShells/{Module}/`
2. Add page selectors in `Targets/{Module}/{Module}PageTargets.cs`
3. Add test data constants in `TestData/{Module}/{Module}TestData.cs`
4. Add composite tasks in `Tasks/{Module}/Open{Module}.cs` (and others as needed)
5. Add test fixture in `BusinessShells/{Module}/{Module}Tests.cs` extending `ScreenplayTestBase`

Use the existing `MagazineExceptions` module as a reference implementation.

---

## Currently covered modules

| Module | Tests | Notes |
|---|---|---|
| Magazine Exceptions | TC_001–TC_021 | Grid CRUD, filtering, pagination, Add Exception dialog |

---

## Known limitations

- **Edit / Delete confirmation** — Edit action and Delete confirmation dialog are blocked by server-side defects. Affected tests (`TC_005`, `TC_006`, `TC_007`, `TC_008`, `TC_013`) are marked `[Ignore]` with defect notes and will run automatically once the defects are resolved.
- **Test data isolation** — tests currently rely on static data present in the test environment. API-based setup/teardown is not yet implemented; `TC_014` and `TC_015` add records that require manual cleanup between runs.
- **Single browser** — only Chromium is configured. Cross-browser support can be added via the `PLAYWRIGHT_BROWSER` environment variable with minor framework changes.
