using Frontline.Tests.Core.Screenplay.Configuration;
using Frontline.Tests.Core.Screenplay.Tasks;
using Frontline.Tests.Core.Screenplay.Questions;
using Frontline.Tests.Core.Screenplay.Targets;
using Frontline.Tests.Core.Screenplay.TestData;
using FrontlineTests.Common;

namespace FrontlineTests.Tests;

[TestFixture]
public class MagazineExceptionsTests : ScreenplayTestBase
{
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

        await Task.Delay(2000);
    }
}