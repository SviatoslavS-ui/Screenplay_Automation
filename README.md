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
| Database access | Microsoft.Data.SqlClient 6.0 (test data cleanup) |
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
- **Ability** — capability an actor holds (`BrowserAbility` for Playwright, `DatabaseAbility` for SQL Server)

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
dotnet test --filter "Category=Smoke"          # 3 tests, ~1 min — navigation, data read, create
dotnet test --filter "Category=Functional"     # bulk — all feature tests
dotnet test --filter "Category=EdgeCase"       # boundary conditions
dotnet test --filter "Category=Security"       # access control
dotnet test                                    # full regression — all categories
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
| `FRONTLINE_SQL_ENABLED` | `true` | Set `false` to disable database cleanup |
| `FRONTLINE_SQL_CONNECTION` | *(Windows Auth to flgsqlstdtest)* | SQL Server connection string for test data cleanup |

---

## Test artifacts

After each run:

- **Traces** — `TestResults/traces/{testName}.zip` — recorded for every test; open with `playwright show-trace <file>`
- **Screenshots** — `TestResults/screenshots/{testName}.png` — captured on failure only

---

## Test categories

```
Regression (all tests — nightly / release gate)
  ├── Smoke       3 tests, ~1 min   "Is the app alive?"
  ├── Functional  bulk               Feature coverage
  ├── EdgeCase    boundary tests     Empty states, no-results, special chars
  └── Security    access control     Auth verification
```

| Category | Tests | When to run |
|---|---|---|
| **Smoke** | TC_001 (navigate), TC_001b (read data), TC_014 (create) | Every deploy, every PR |
| **Functional** | TC_002–TC_010, TC_014–TC_019, TC_021 | Daily CI, pre-merge |
| **EdgeCase** | TC_011, TC_013, TC_020 | Full regression |
| **Security** | TC_012 | Full regression |
| **Regression** | All of the above (no filter) | Nightly, release gate |

---

## Test data cleanup

Tests that create records via the UI (e.g. `TC_014`, `TC_015`) automatically clean up after themselves using direct SQL Server access via `DatabaseAbility`. The cleanup deletes records matching the test's business keys (company + reason code) and the Windows user who created them.

All DB operations in test bodies go through generic Screenplay abstractions:
- **`DbExecute`** (Interaction) — parameterised DELETE/INSERT
- **`DbRecordExists`** (Question) — "does the record exist?" → bool
- **`DbScalar<T>`** (Question) — generic scalar query → typed result

**Graceful degradation:** If the database is unreachable (e.g. cloud CI agents without network access), a warning is logged and tests run normally — cleanup is simply skipped. Tests that require DB access (TC_014, TC_015) will fail fast with a clear error.

---

## CI/CD setup

### Azure DevOps pipeline example (self-hosted agent on domain)

```yaml
trigger:
  branches:
    include:
      - master
      - feature/*

pool:
  name: 'SelfHosted'  # Agent on the domain with access to flgsqlstdtest

steps:
  - task: UseDotNet@2
    inputs:
      version: '9.0.x'

  - script: dotnet build
    displayName: 'Build'

  - script: |
      dotnet build
      pwsh Frontline.Tests.Core/bin/Debug/net9.0/playwright.ps1 install chromium
    displayName: 'Install Playwright browsers'

  - script: dotnet test --logger trx --results-directory $(Build.ArtifactStagingDirectory)/TestResults
    displayName: 'Run tests'
    env:
      PLAYWRIGHT_HEADLESS: 'true'
      # SQL cleanup uses Windows Auth defaults — no config needed on domain agents

  - task: PublishTestResults@2
    inputs:
      testResultsFormat: 'VSTest'
      testResultsFiles: '**/*.trx'
      searchFolder: $(Build.ArtifactStagingDirectory)/TestResults
    condition: always()

  - publish: $(Build.ArtifactStagingDirectory)/TestResults
    artifact: TestResults
    condition: always()
```

### Cloud-hosted agent (no domain access)

```yaml
pool:
  vmImage: 'windows-latest'

# Same steps as above, but disable SQL cleanup:
env:
  PLAYWRIGHT_HEADLESS: 'true'
  FRONTLINE_SQL_ENABLED: 'false'
```

### Environment matrix

| Environment | DB reachable | `FRONTLINE_SQL_ENABLED` | Cleanup behaviour |
|---|---|---|---|
| Local dev | Yes | `true` (default) | Full cleanup after TC_014/015 |
| CI self-hosted (domain) | Yes | `true` (default) | Full cleanup |
| CI cloud-hosted | No | `false` | Tests pass, cleanup skipped |
| CI cloud-hosted | No | `true` (default) | Warning logged, tests pass, cleanup skipped |

---

## Project structure

```
FrontlineTests.slnx
├── Frontline.Tests.Core/          ← Screenplay framework + page objects
│   └── Screenplay/
│       ├── Core/                  ← Actor, ITask, IInteraction, IQuestion
│       ├── Abilities/             ← BrowserAbility (Playwright), DatabaseAbility (SQL Server)
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
| Magazine Exceptions | TC_001–TC_021 (37 test cases) | Navigation, grid filtering, pagination, Add Exception dialog, DB verification. 9 tests `[Ignore]`d pending defect fixes. |

