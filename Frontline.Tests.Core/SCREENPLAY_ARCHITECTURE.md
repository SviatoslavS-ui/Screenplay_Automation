# Screenplay Pattern Framework - Architecture Guide

## Overview

This framework implements the **Screenplay Pattern**, a modern BDD (Behavior-Driven Development) approach to test automation that emphasizes:
- **Actor-centric perspective**: Tests are written from the actor's point of view
- **Reusability**: Tasks and questions are highly reusable across tests
- **Maintainability**: Changes to UI selectors/interactions are localized
- **Readability**: Test code reads like natural language

## Core Components

### 1. **Actor** (`Actor.cs`)
**What it does**: Represents a test user performing actions in your application.

**Key methods**:
- `Can(ability)` - Grants the actor an ability
- `Performs(task/interaction)` - Executes a task or interaction
- `Asks<T>(question)` - Asks a question and gets an answer

**Example**:
```csharp
var alice = new Actor("Alice");
alice.Can(new BrowserAbility());
await alice.Performs(new NavigateTo("https://example.com"));
```

### 2. **Ability** (`IAbility`)
**What it does**: Represents a capability an actor possesses to interact with the system.

**Examples**:
- `BrowserAbility` - Navigate pages, interact with elements
- `RestApiAbility` - Make HTTP requests
- `DatabaseAbility` - Query databases

**Key implementations**:
- **`BrowserAbility.cs`** - Manages Playwright browser/context/page lifecycle
  - Initializes browser with Chromium, Firefox, or WebKit
  - Provides access to Page, Context, and Browser
  - Handles cleanup

### 3. **Task** (`ITask`)
**What it does**: Represents a user-level action composed of one or more interactions.

**Characteristics**:
- Composed of atomic interactions
- Reusable across multiple tests
- Should represent a complete user action

**Example implementations**:
- **`NavigateTo.cs`** - Navigate to a URL
- A Login task would contain: Fill username, Fill password, Click login button

**Example**:
```csharp
await alice.Performs(new NavigateTo("https://example.com"));
```

### 4. **Interaction** (`IInteraction`)
**What it does**: Represents an atomic browser action (lowest-level operations).

**Examples**:
- **`Click.cs`** - Click an element
- **`Fill.cs`** - Fill a text field
- Hover, Scroll, WaitFor, etc.

**Example**:
```csharp
await actor.Performs(new Click("#login-button"));
```

### 5. **Question** (`IQuestion<TAnswer>`)
**What it does**: Retrieves information from the application to verify state or make assertions.

**Benefits**:
- Separates verification logic from test logic
- Makes assertions more expressive
- Reusable across tests

**Example implementations**:
- **`IsVisible.cs`** - Check if element is visible
- **`TextOf.cs`** - Get text content of element
- ValueOf, CountOf, etc.

**Example**:
```csharp
var isVisible = await actor.Asks(new IsVisible("#success-message"));
Assert.That(isVisible, Is.True);
```

### 6. **ActorLibrary** (`ActorLibrary.cs`)
**What it does**: Manages multiple actors in a test scenario.

**Key methods**:
- `GetActor(name)` - Get or create an actor
- `GetAllActors()` - Get all actors
- `RemoveActor(name)` - Remove an actor
- `Clear()` - Clear all actors

**Example**:
```csharp
var library = new ActorLibrary();
var alice = library.GetActor("Alice");
var bob = library.GetActor("Bob");
```

## Directory Structure

```
Frontline.Tests.Core/
├── Screenplay/
│   ├── Core/
│   │   ├── Actor.cs              // Main actor class
│   │   ├── IAbility.cs           // Ability interface
│   │   ├── ITask.cs              // Task interface
│   │   ├── IInteraction.cs       // Interaction interface
│   │   ├── IQuestion.cs          // Question interface
│   │   └── ScreenplayException.cs
│   ├── Abilities/
│   │   └── BrowserAbility.cs     // Playwright browser management
│   ├── Tasks/
│   │   └── NavigateTo.cs         // Navigation task
│   ├── Interactions/
│   │   ├── Click.cs              // Click interaction
│   │   └── Fill.cs               // Fill interaction
│   ├── Questions/
│   │   ├── IsVisible.cs          // Visibility question
│   │   └── TextOf.cs             // Text retrieval question
│   └── Infrastructure/
│       └── ActorLibrary.cs       // Actor management
```

## Usage Example

```csharp
[Test]
public async Task UserCanLoginSuccessfully()
{
    // Arrange - Create an actor and give them abilities
    var actor = new Actor("John");
    var browserAbility = new BrowserAbility();
    await browserAbility.InitializeAsync();
    actor.Can(browserAbility);

    // Act - Perform tasks
    await actor.Performs(new NavigateTo("https://app.example.com/login"));
    await actor.Performs(new Fill("#username", "john@example.com"));
    await actor.Performs(new Fill("#password", "password123"));
    await actor.Performs(new Click("#login-button"));

    // Assert - Ask questions
    var isLoggedIn = await actor.Asks(new IsVisible("#dashboard"));
    Assert.That(isLoggedIn, Is.True);

    // Cleanup
    await browserAbility.CloseAsync();
}
```

## Key Design Principles

1. **Separation of Concerns**
   - Abilities handle "how" to do things
   - Tasks define "what" needs to be done
   - Questions define "what" to verify

2. **Reusability**
   - Tasks and questions can be reused across multiple tests
   - Abilities can be shared between actors
   - UI locators are encapsulated in interactions/questions

3. **Readability**
   - Code reads like a story: "Alice navigates to, fills in, clicks, then verifies"
   - Business stakeholders can understand test flow
   - Clear intent of each action

4. **Maintainability**
   - UI changes require updates in one place (interactions/questions)
   - New features can be added without modifying existing tests
   - Easy to add new abilities, tasks, or questions

## Next Steps - Recommended Additions

1. **Composite Tasks**
   - `Login` task = Fill + Fill + Click
   - `CompleteCheckout` task = Multiple tasks combined

2. **Additional Interactions**
   - `Press` - Press keyboard keys
   - `Hover` - Hover over elements
   - `WaitFor` - Wait for conditions
   - `SelectOption` - Select dropdown values

3. **Additional Questions**
   - `ValueOf` - Get input values
   - `CountOf` - Count elements
   - `AttributeOf` - Get element attributes
   - `CurrentUrl` - Get current page URL

4. **Test Fixtures/Base Classes**
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
