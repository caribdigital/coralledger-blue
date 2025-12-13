namespace CoralLedger.Blue.E2E.Tests.Pages;

/// <summary>
/// Page object for the Dashboard page
/// </summary>
public class DashboardPage : BasePage
{
    public override string Path => "/";

    public DashboardPage(IPage page, string baseUrl) : base(page, baseUrl)
    {
    }

    public async Task<IReadOnlyList<ILocator>> GetStatCardsAsync()
    {
        // Look for stat cards - they typically have a consistent structure
        var cards = Page.Locator(".stat-card, .card, [class*='stat']");
        await cards.First.WaitForAsync(new() { Timeout = 10000 });
        return await cards.AllAsync();
    }

    public async Task<bool> HasMpaCountAsync()
    {
        // Look for MPA count display - use First to avoid strict mode violations
        var mpaElement = Page.GetByText("Marine Protected Areas").Or(
            Page.GetByText("MPAs")).Or(
            Page.GetByText("Protected Areas")).First;
        return await mpaElement.IsVisibleAsync();
    }

    public async Task<bool> HasBleachingStatusAsync()
    {
        var bleachingElement = Page.GetByText("Bleaching").Or(
            Page.GetByText("Coral")).First;
        return await bleachingElement.IsVisibleAsync();
    }

    public async Task<bool> HasVesselCountAsync()
    {
        var vesselElement = Page.GetByText("Vessel").Or(
            Page.GetByText("Fishing")).First;
        return await vesselElement.IsVisibleAsync();
    }

    public async Task<bool> HasObservationsCountAsync()
    {
        var observationsElement = Page.GetByText("Observation").Or(
            Page.GetByText("Sighting")).First;
        return await observationsElement.IsVisibleAsync();
    }
}
