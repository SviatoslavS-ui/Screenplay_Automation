using Frontline.Tests.Core.Screenplay.Core;
using Frontline.Tests.Core.Screenplay.Interactions;

namespace Frontline.Tests.Core.Screenplay.Tasks;

/// <summary>Navigates the actor's browser to the specified URL and waits for Blazor to fully render.</summary>
public class NavigateTo(string url) : ITask
{
    public string Description => $"Navigate to {url}";

    public async Task PerformAsync(Actor actor)
    {
        ArgumentNullException.ThrowIfNull(url);
        var browserAbility = actor.UsesAbility<Abilities.BrowserAbility>();
        await browserAbility.Page.GotoAsync(url);
        await actor.Performs(WaitForPageLoaded.ForFullLoad());
    }
}
