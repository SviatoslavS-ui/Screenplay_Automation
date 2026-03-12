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
