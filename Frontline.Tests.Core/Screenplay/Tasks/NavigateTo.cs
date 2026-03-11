using Frontline.Tests.Core.Screenplay.Core;

namespace Frontline.Tests.Core.Screenplay.Tasks;

/// <summary>
/// Task to navigate to a URL.
/// Composite task: opens a page and navigates to the specified URL.
/// </summary>
public class NavigateTo(string url) : ITask
{
    public string TaskDescription => $"Navigate to {url}";

    public async Task PerformAsync(Actor actor)
    {
        ArgumentNullException.ThrowIfNull(url);

        var browserAbility = actor.UsesAbility<Abilities.BrowserAbility>("BrowserAbility");
        await browserAbility.Page.GotoAsync(url);
    }
}
