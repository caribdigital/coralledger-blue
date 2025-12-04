using CoralLedger.E2E.Tests.Pages;

namespace CoralLedger.E2E.Tests.Tests;

/// <summary>
/// E2E tests for the Map page
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
    public async Task Map_LoadsSuccessfully()
    {
        // Act
        await _mapPage.NavigateAsync();

        // Assert
        var isMapVisible = await _mapPage.IsMapVisibleAsync();
        isMapVisible.Should().BeTrue("Map should be visible on the map page");
    }

    [Test]
    public async Task Map_HasMpaBoundaries()
    {
        // Arrange
        await _mapPage.NavigateAsync();
        // Give map time to render overlays
        await Task.Delay(2000);

        // Act
        var hasMpaLayer = await _mapPage.HasMpaLayerAsync();

        // Assert
        hasMpaLayer.Should().BeTrue("Map should display MPA boundary layers");
    }

    [Test]
    public async Task Map_HasLayerControls()
    {
        // Arrange
        await _mapPage.NavigateAsync();

        // Act
        var layerControls = await _mapPage.GetLayerControlsAsync();
        var isVisible = await layerControls.IsVisibleAsync();

        // Assert
        isVisible.Should().BeTrue("Map should have layer control panel");
    }

    [Test]
    public async Task Map_NoConsoleErrors()
    {
        // Act
        await _mapPage.NavigateAsync();
        await Task.Delay(2000); // Wait for map to fully load

        // Assert
        AssertNoConsoleErrors();
    }
}
