namespace CoralLedger.E2E.Tests.Pages;

/// <summary>
/// Page object for the Bleaching page
/// </summary>
public class BleachingPage : BasePage
{
    public override string Path => "/bleaching";

    public BleachingPage(IPage page, string baseUrl) : base(page, baseUrl)
    {
    }

    public async Task<bool> HasBleachingDataAsync()
    {
        // Wait for data to load
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Look for specific bleaching-related content using First to avoid strict mode violations
        var content = Page.GetByText("Alert Level").Or(
            Page.GetByText("DHW")).Or(
            Page.GetByText("Degree Heating")).Or(
            Page.GetByText("SST")).Or(
            Page.GetByText("Temperature")).First;
        return await content.IsVisibleAsync();
    }

    public async Task<bool> HasChartsAsync()
    {
        // Look for chart elements (ApexCharts or similar)
        var chart = Page.Locator("canvas, svg[class*='chart'], .apexcharts-canvas, [class*='chart']").First;
        return await chart.IsVisibleAsync();
    }

    public async Task<IReadOnlyList<ILocator>> GetBleachingCardsAsync()
    {
        var cards = Page.Locator(".card, [class*='bleaching-card']");
        return await cards.AllAsync();
    }

    public async Task<bool> HasMpaDropdownAsync()
    {
        var dropdown = Page.Locator("select, [class*='dropdown'], [role='listbox']").First;
        return await dropdown.IsVisibleAsync();
    }

    public async Task SelectMpaAsync(string mpaName)
    {
        var dropdown = Page.Locator("select, [class*='dropdown']").First;
        if (await dropdown.IsVisibleAsync())
        {
            await dropdown.SelectOptionAsync(new SelectOptionValue { Label = mpaName });
        }
    }
}
