namespace CoralLedger.E2E.Tests.Tests;

/// <summary>
/// E2E tests for navigation between pages
/// </summary>
[TestFixture]
public class NavigationTests : PlaywrightFixture
{
    [Test]
    public async Task Navigation_CanAccessDashboard()
    {
        // Act
        await NavigateToAsync("/");

        // Assert
        var url = Page.Url;
        url.Should().Contain(BaseUrl);
    }

    [Test]
    public async Task Navigation_CanAccessMap()
    {
        // Act
        await NavigateToAsync("/map");

        // Assert
        var url = Page.Url;
        url.Should().Contain("/map");
    }

    [Test]
    public async Task Navigation_CanAccessBleaching()
    {
        // Act
        await NavigateToAsync("/bleaching");

        // Assert
        var url = Page.Url;
        url.Should().Contain("/bleaching");
    }

    [Test]
    public async Task Navigation_CanAccessObservations()
    {
        // Act
        await NavigateToAsync("/observations");

        // Assert
        var url = Page.Url;
        url.Should().Contain("/observations");
    }

    [Test]
    public async Task Navigation_NavbarExists()
    {
        // Arrange
        await NavigateToAsync("/");

        // Act
        var nav = Page.Locator("nav, [role='navigation'], .navbar");
        var isVisible = await nav.IsVisibleAsync();

        // Assert
        isVisible.Should().BeTrue("Navigation bar should be visible");
    }

    [Test]
    public async Task Navigation_ClickMapLink()
    {
        // Arrange
        await NavigateToAsync("/");

        // Act
        var mapLink = Page.GetByRole(AriaRole.Link, new() { Name = "Map" }).Or(
            Page.Locator("a[href='/map'], a[href*='map']"));

        if (await mapLink.IsVisibleAsync())
        {
            await mapLink.ClickAsync();
            await WaitForBlazorAsync();

            // Assert
            Page.Url.Should().Contain("/map");
        }
    }

    [Test]
    public async Task Navigation_NoConsoleErrorsOnAllPages()
    {
        // Test each main page for console errors
        var pages = new[] { "/", "/map", "/bleaching", "/observations" };

        foreach (var path in pages)
        {
            ConsoleErrors.Clear();
            await NavigateToAsync(path);
            await Task.Delay(1000);

            ConsoleErrors.Should().BeEmpty($"Page {path} should not have console errors");
        }
    }
}
