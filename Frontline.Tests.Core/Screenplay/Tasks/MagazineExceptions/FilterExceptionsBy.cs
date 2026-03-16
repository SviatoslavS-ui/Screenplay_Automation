using Frontline.Tests.Core.Screenplay.Core;
using Frontline.Tests.Core.Screenplay.Interactions;

namespace Frontline.Tests.Core.Screenplay.Tasks.MagazineExceptions;

/// <summary>Fills a grid filter field. Follow with ShouldEventuallyRead to wait for rows to settle.</summary>
public class FilterExceptionsBy(string filterFieldSelector, string filterValue) : ITask
{
    public string Description => $"Filter exceptions by '{filterFieldSelector}' = '{filterValue}'";

    public async Task PerformAsync(Actor actor)
    {
        ArgumentNullException.ThrowIfNull(filterFieldSelector);
        ArgumentNullException.ThrowIfNull(filterValue);
        await actor.Performs(new Fill(filterFieldSelector, filterValue));
    }
}
