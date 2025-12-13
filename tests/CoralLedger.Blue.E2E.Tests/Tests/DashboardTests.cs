using CoralLedger.Blue.E2E.Tests.Pages;

namespace CoralLedger.Blue.E2E.Tests.Tests;

/// <summary>
/// E2E tests for the Dashboard page
/// </summary>
[TestFixture]
public class DashboardTests : PlaywrightFixture
{
    private DashboardPage _dashboard = null!;

    [SetUp]
    public async Task SetUp()
    {
        _dashboard = new DashboardPage(Page, BaseUrl);
    }

    [Test]
    public async Task Dashboard_LoadsSuccessfully()
    {
        // Act
        await _dashboard.NavigateAsync();

        // Assert
        var title = await Page.TitleAsync();
        title.Should().Contain("CoralLedger");
    }

    [Test]
    public async Task Dashboard_HasStatCards()
    {
        // Arrange
        await _dashboard.NavigateAsync();

        // Act
        var cards = await _dashboard.GetStatCardsAsync();

        // Assert - Dashboard should have stat cards
        cards.Count.Should().BeGreaterOrEqualTo(1);
    }

    [Test]
    public async Task Dashboard_DisplaysMpaInfo()
    {
        // Arrange
        await _dashboard.NavigateAsync();

        // Act & Assert
        var hasMpa = await _dashboard.HasMpaCountAsync();
        hasMpa.Should().BeTrue("Dashboard should display MPA information");
    }

    [Test]
    public async Task Dashboard_NoConsoleErrors()
    {
        // Act
        await _dashboard.NavigateAsync();
        await Task.Delay(2000); // Wait for any async operations

        // Assert
        AssertNoConsoleErrors();
    }
}
