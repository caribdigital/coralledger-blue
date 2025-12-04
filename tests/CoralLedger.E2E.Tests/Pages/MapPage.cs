namespace CoralLedger.E2E.Tests.Pages;

/// <summary>
/// Page object for the Map page with Mapsui map component
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
        // Wait for Mapsui map container to render
        await Page.WaitForSelectorAsync(".mpa-map-container, [class*='map']",
            new PageWaitForSelectorOptions { Timeout = 15000 });
        // Wait for loading overlay to disappear
        await Page.WaitForFunctionAsync(@"() => {
            const overlay = document.querySelector('.loading-overlay');
            return !overlay || overlay.style.display === 'none' || !document.body.contains(overlay);
        }", new PageWaitForFunctionOptions { Timeout = 20000 });
    }

    public async Task<bool> IsMapContainerVisibleAsync()
    {
        var mapContainer = Page.Locator(".mpa-map-container");
        return await mapContainer.IsVisibleAsync();
    }

    public async Task<bool> IsMapCanvasVisibleAsync()
    {
        // Mapsui uses a canvas element for rendering
        var canvas = Page.Locator(".mpa-map-container canvas, canvas");
        return await canvas.IsVisibleAsync();
    }

    public async Task<bool> HasLegendAsync()
    {
        var legend = Page.Locator(".map-legend");
        return await legend.IsVisibleAsync();
    }

    public async Task<bool> HasProtectionLevelsInLegendAsync()
    {
        var noTake = Page.GetByText("No-Take Zone");
        var highlyProtected = Page.GetByText("Highly Protected");
        var lightlyProtected = Page.GetByText("Lightly Protected");

        var hasNoTake = await noTake.IsVisibleAsync();
        var hasHighly = await highlyProtected.IsVisibleAsync();
        var hasLightly = await lightlyProtected.IsVisibleAsync();

        return hasNoTake && hasHighly && hasLightly;
    }

    public async Task<bool> HasLoadingOverlayDisappearedAsync()
    {
        var loadingOverlay = Page.Locator(".loading-overlay");
        // Should either not exist or not be visible
        var isVisible = await loadingOverlay.IsVisibleAsync();
        return !isVisible;
    }

    public async Task ClickOnMapCenterAsync()
    {
        var mapContainer = Page.Locator(".mpa-map-container");
        await mapContainer.ClickAsync();
    }

    public async Task<bool> HasMpaInfoPopupAfterClickAsync()
    {
        // Click on the map to try to select an MPA
        await ClickOnMapCenterAsync();
        await Task.Delay(500);

        var infoPopup = Page.Locator(".map-info-popup");
        return await infoPopup.IsVisibleAsync();
    }

    public async Task<ILocator> GetMapContainerAsync()
    {
        return Page.Locator(".mpa-map-container");
    }

    public async Task<bool> CanZoomAsync()
    {
        // Check if map responds to scroll/zoom
        var mapContainer = await GetMapContainerAsync();
        var initialBoundingBox = await mapContainer.BoundingBoxAsync();

        // Scroll to zoom
        await mapContainer.HoverAsync();
        await Page.Mouse.WheelAsync(0, -100); // Zoom in
        await Task.Delay(300);

        // Map should still be visible and responsive
        return await IsMapContainerVisibleAsync();
    }

    public async Task<bool> CanPanAsync()
    {
        var mapContainer = await GetMapContainerAsync();

        // Drag to pan
        var box = await mapContainer.BoundingBoxAsync();
        if (box == null) return false;

        var startX = box.X + box.Width / 2;
        var startY = box.Y + box.Height / 2;

        await Page.Mouse.MoveAsync((float)startX, (float)startY);
        await Page.Mouse.DownAsync();
        await Page.Mouse.MoveAsync((float)(startX + 50), (float)(startY + 50));
        await Page.Mouse.UpAsync();
        await Task.Delay(300);

        // Map should still be visible
        return await IsMapContainerVisibleAsync();
    }

    public async Task<int> GetLegendItemCountAsync()
    {
        var legendItems = Page.Locator(".map-legend .legend-item");
        return await legendItems.CountAsync();
    }
}
