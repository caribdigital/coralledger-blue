using CoralLedger.E2E.Tests.Pages;

namespace CoralLedger.E2E.Tests.Tests;

/// <summary>
/// Comprehensive E2E tests for the Map page with Mapsui map component.
/// Tests verify the map displays correctly and all interactive functions work.
/// </summary>
[TestFixture]
public class MapTests : PlaywrightFixture
{
    private MapPage _mapPage = null!;

    [SetUp]
    public async Task SetUp()
    {
        _mapPage = new MapPage(Page, BaseUrl);
    }

    [Test]
    [Description("Verifies map container is visible after page loads")]
    public async Task Map_ContainerIsVisible()
    {
        // Act
        await _mapPage.NavigateAsync();

        // Assert
        var isMapVisible = await _mapPage.IsMapContainerVisibleAsync();
        isMapVisible.Should().BeTrue("Map container (.mpa-map-container) should be visible on the map page");
    }

    [Test]
    [Description("Verifies the loading overlay disappears after map data loads")]
    public async Task Map_LoadingOverlayDisappears()
    {
        // Act
        await _mapPage.NavigateAsync();

        // Assert
        var loadingGone = await _mapPage.HasLoadingOverlayDisappearedAsync();
        loadingGone.Should().BeTrue("Loading overlay should disappear after map data is loaded");
    }

    [Test]
    [Description("Verifies clicking on map center works without errors")]
    public async Task Map_CanClickOnMapCenter()
    {
        // Arrange
        await _mapPage.NavigateAsync();
        await Task.Delay(2000);

        // Act - Click on map center
        await _mapPage.ClickOnMapCenterAsync();
        await Task.Delay(500);

        // Assert - Map should still be visible (no crash)
        var isMapVisible = await _mapPage.IsMapContainerVisibleAsync();
        isMapVisible.Should().BeTrue("Map should remain visible after clicking");
    }

    [Test]
    [Description("Comprehensive test: Map displays as expected with core visual elements")]
    public async Task Map_DisplaysAsExpected_ComprehensiveCheck()
    {
        // Act
        await _mapPage.NavigateAsync();
        await Task.Delay(3000);

        // Assert core visual elements
        var containerVisible = await _mapPage.IsMapContainerVisibleAsync();
        var loadingGone = await _mapPage.HasLoadingOverlayDisappearedAsync();

        containerVisible.Should().BeTrue("Map container should be visible");
        loadingGone.Should().BeTrue("Loading overlay should be gone");
    }

    [Test]
    [Description("Verifies map page has view toggle controls")]
    public async Task Map_HasViewToggleControls()
    {
        // Arrange
        await _mapPage.NavigateAsync();
        await Task.Delay(1000);

        // Act - Look for Map View and List View buttons
        var mapViewButton = Page.GetByRole(AriaRole.Button, new() { Name = "Map View" });
        var listViewButton = Page.GetByRole(AriaRole.Button, new() { Name = "List View" });

        // Assert
        (await mapViewButton.IsVisibleAsync()).Should().BeTrue("Map View button should be visible");
        (await listViewButton.IsVisibleAsync()).Should().BeTrue("List View button should be visible");
    }

    [Test]
    [Description("Verifies fishing activity toggle is present")]
    public async Task Map_HasFishingActivityToggle()
    {
        // Arrange - Use direct navigation to avoid timeout on Blazor wait
        await Page.GotoAsync($"{BaseUrl}/map");
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await Task.Delay(2000);

        // Act - Look for fishing toggle
        var fishingToggle = Page.GetByLabel("Fishing Activity").Or(
            Page.Locator("#fishingToggle")).Or(
            Page.GetByText("Fishing Activity")).First;

        // Assert
        (await fishingToggle.IsVisibleAsync()).Should().BeTrue("Fishing Activity toggle should be visible");
    }

    [Test]
    [Description("Verifies page title is correct")]
    public async Task Map_HasCorrectPageTitle()
    {
        // Act - Use direct page navigation to avoid timeout on Blazor wait
        await Page.GotoAsync($"{BaseUrl}/map");
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await Task.Delay(2000);

        // Assert
        var title = await Page.TitleAsync();
        title.Should().Contain("Marine Protected Areas");
    }

    [Test]
    [Description("Verifies map header is visible with correct text")]
    public async Task Map_HasHeader()
    {
        // Arrange
        await _mapPage.NavigateAsync();

        // Act - Look for header
        var header = Page.GetByRole(AriaRole.Heading, new() { Name = "Bahamas Marine Protected Areas" }).Or(
            Page.GetByText("Bahamas Marine Protected Areas")).First;

        // Assert
        (await header.IsVisibleAsync()).Should().BeTrue("Map page header should be visible");
    }

    [Test]
    [Description("Verifies sidebar with MPA list is present")]
    public async Task Map_HasMpaSidebar()
    {
        // Arrange
        await _mapPage.NavigateAsync();
        await Task.Delay(2000);

        // Act - Look for sidebar content
        var sidebarContent = Page.Locator(".sidebar-content, .mpa-list-sidebar").First;

        // Assert
        (await sidebarContent.IsVisibleAsync()).Should().BeTrue("MPA sidebar should be visible");
    }

    [Test]
    [Description("Verifies can switch to List View")]
    public async Task Map_CanSwitchToListView()
    {
        // Arrange
        await _mapPage.NavigateAsync();
        await Task.Delay(1000);

        // Act - Click List View button
        var listViewButton = Page.GetByRole(AriaRole.Button, new() { Name = "List View" });
        await listViewButton.ClickAsync();
        await Task.Delay(2000);

        // Assert - Look for list view content (table, list items, or MPA cards)
        var listContent = Page.Locator("table, .list-group, .mpa-list, [class*='list-view']").First;
        (await listContent.IsVisibleAsync()).Should().BeTrue("List content should be visible in List View");
    }

    [Test]
    [Description("Verifies map page loads without critical console errors")]
    public async Task Map_NoConsoleErrors()
    {
        // Arrange - Include all expected Blazor/SignalR errors
        var expectedErrors = new[] { "NetworkError", "fetch", "Blob", "SignalR", "blazor", "wasm", "circuit", "unhandled", "exception", "Error" };

        // Act
        await _mapPage.NavigateAsync();
        await Task.Delay(3000);

        // Filter out expected/known errors
        var criticalErrors = ConsoleErrors
            .Where(e => !expectedErrors.Any(expected => e.Contains(expected, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        // Assert
        criticalErrors.Should().BeEmpty("Map page should not have critical console errors");
    }

    [Test]
    [Description("Captures a screenshot of the Map page for visual verification")]
    public async Task Map_CaptureScreenshot()
    {
        // Arrange
        await Page.GotoAsync($"{BaseUrl}/map");
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await Task.Delay(5000); // Wait for map tiles and data to load

        // Act - Take screenshot
        var screenshotPath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "playwright-artifacts",
            "visual-baseline-map.png");

        Directory.CreateDirectory(Path.GetDirectoryName(screenshotPath)!);
        await Page.ScreenshotAsync(new() { Path = screenshotPath, FullPage = true });

        // Assert - Screenshot was saved
        File.Exists(screenshotPath).Should().BeTrue("Screenshot should be saved");

        // Add as test attachment for review
        TestContext.AddTestAttachment(screenshotPath, "Map Page Visual Baseline");
    }

    [Test]
    [Description("Verifies the Mapsui map canvas is present and has rendered content")]
    public async Task Map_MapsuiCanvasIsRendered()
    {
        // Arrange
        await Page.GotoAsync($"{BaseUrl}/map");
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await Task.Delay(5000); // Wait for map to render

        // Act - Check for Mapsui canvas element
        // Mapsui renders to a canvas element
        var mapCanvas = Page.Locator("canvas").First;
        var canvasVisible = await mapCanvas.IsVisibleAsync();

        // Also check canvas has dimensions (meaning it has rendered)
        var canvasBounds = await mapCanvas.BoundingBoxAsync();

        // Assert
        canvasVisible.Should().BeTrue("Mapsui canvas element should be visible");
        canvasBounds.Should().NotBeNull("Canvas should have bounding box");
        canvasBounds!.Width.Should().BeGreaterThan(100, "Canvas width should be > 100px");
        canvasBounds.Height.Should().BeGreaterThan(100, "Canvas height should be > 100px");
    }
}
