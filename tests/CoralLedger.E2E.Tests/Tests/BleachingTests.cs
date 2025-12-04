using CoralLedger.E2E.Tests.Pages;

namespace CoralLedger.E2E.Tests.Tests;

/// <summary>
/// E2E tests for the Bleaching page
/// </summary>
[TestFixture]
public class BleachingTests : PlaywrightFixture
{
    private BleachingPage _bleachingPage = null!;

    [SetUp]
    public async Task SetUp()
    {
        _bleachingPage = new BleachingPage(Page, BaseUrl);
    }

    [Test]
    public async Task Bleaching_LoadsSuccessfully()
    {
        // Act
        await _bleachingPage.NavigateAsync();

        // Assert
        var title = await Page.TitleAsync();
        title.Should().Contain("CoralLedger");
    }

    [Test]
    public async Task Bleaching_DisplaysBleachingData()
    {
        // Arrange
        await _bleachingPage.NavigateAsync();
        await Task.Delay(2000); // Wait for data to load

        // Act
        var hasData = await _bleachingPage.HasBleachingDataAsync();

        // Assert
        hasData.Should().BeTrue("Bleaching page should display bleaching data");
    }

    [Test]
    public async Task Bleaching_HasMpaSelector()
    {
        // Arrange
        await _bleachingPage.NavigateAsync();

        // Act
        var hasDropdown = await _bleachingPage.HasMpaDropdownAsync();

        // Assert - Should have MPA selector
        hasDropdown.Should().BeTrue("Bleaching page should have MPA selection dropdown");
    }

    [Test]
    public async Task Bleaching_NoConsoleErrors()
    {
        // Act
        await _bleachingPage.NavigateAsync();
        await Task.Delay(2000); // Wait for data to load

        // Assert
        AssertNoConsoleErrors();
    }
}
