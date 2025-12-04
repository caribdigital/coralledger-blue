using CoralLedger.E2E.Tests.Pages;

namespace CoralLedger.E2E.Tests.Tests;

/// <summary>
/// E2E tests for the Observations page
/// </summary>
[TestFixture]
public class ObservationsTests : PlaywrightFixture
{
    private ObservationsPage _observationsPage = null!;

    [SetUp]
    public async Task SetUp()
    {
        _observationsPage = new ObservationsPage(Page, BaseUrl);
    }

    [Test]
    public async Task Observations_LoadsSuccessfully()
    {
        // Act
        await _observationsPage.NavigateAsync();

        // Assert
        var title = await Page.TitleAsync();
        title.Should().Contain("CoralLedger");
    }

    [Test]
    public async Task Observations_HasForm()
    {
        // Arrange
        await _observationsPage.NavigateAsync();

        // Act
        var hasForm = await _observationsPage.HasObservationFormAsync();

        // Assert
        hasForm.Should().BeTrue("Observations page should have observation submission form");
    }

    [Test]
    public async Task Observations_HasObservationList()
    {
        // Arrange
        await _observationsPage.NavigateAsync();
        await Task.Delay(1000); // Wait for data to load

        // Act
        var hasList = await _observationsPage.HasObservationListAsync();

        // Assert
        hasList.Should().BeTrue("Observations page should display observation list");
    }

    [Test]
    public async Task Observations_NoConsoleErrors()
    {
        // Act
        await _observationsPage.NavigateAsync();
        await Task.Delay(2000); // Wait for page to fully load

        // Assert
        AssertNoConsoleErrors();
    }
}
