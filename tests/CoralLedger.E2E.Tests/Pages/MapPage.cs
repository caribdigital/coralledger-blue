namespace CoralLedger.E2E.Tests.Pages;

/// <summary>
/// Page object for the Map page
/// </summary>
public class MapPage : BasePage
{
    public override string Path => "/map";

    public MapPage(IPage page, string baseUrl) : base(page, baseUrl)
    {
    }

    protected override async Task WaitForPageLoadAsync()
    {
        await base.WaitForPageLoadAsync();
        // Wait for map container to be visible
        await Page.WaitForSelectorAsync(".leaflet-container, [class*='map-container'], #map",
            new() { Timeout = 15000 });
    }

    public async Task<bool> IsMapVisibleAsync()
    {
        var map = Page.Locator(".leaflet-container, [class*='map-container'], #map");
        return await map.IsVisibleAsync();
    }

    public async Task<bool> HasMpaLayerAsync()
    {
        // Check for MPA boundary layers or legend
        var mpaLayer = Page.Locator(".leaflet-overlay-pane svg, [class*='mpa'], [data-layer='mpa']");
        return await mpaLayer.IsVisibleAsync();
    }

    public async Task<ILocator> GetLayerControlsAsync()
    {
        return Page.Locator(".leaflet-control-layers, [class*='layer-control']");
    }

    public async Task ToggleFishingLayerAsync()
    {
        var layerControl = await GetLayerControlsAsync();
        if (await layerControl.IsVisibleAsync())
        {
            // Look for fishing/vessel layer toggle
            var fishingToggle = Page.GetByText("Fishing").Or(
                Page.GetByText("Vessels")).Or(
                Page.GetByLabel("Fishing"));
            if (await fishingToggle.IsVisibleAsync())
            {
                await fishingToggle.ClickAsync();
            }
        }
    }

    public async Task<bool> HasLegendAsync()
    {
        var legend = Page.Locator(".leaflet-control, [class*='legend']");
        return await legend.IsVisibleAsync();
    }
}
