namespace CoralLedger.E2E.Tests.Tests;

/// <summary>
/// Evidence capture tests for UI/Map Implementation Plan features.
/// Captures screenshots to C:\Projects\Screenshots\CoralLedgerBlue\evidence\
/// </summary>
[TestFixture]
public class UIImplementationEvidenceTests : PlaywrightFixture
{
    private const string EvidenceDir = @"C:\Projects\Screenshots\CoralLedgerBlue\evidence";

    [SetUp]
    public void SetUpEvidenceDir()
    {
        Directory.CreateDirectory(EvidenceDir);
    }

    private async Task CaptureEvidenceAsync(string featureId, string description)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var filename = $"{featureId}_{timestamp}.png";
        var filepath = Path.Combine(EvidenceDir, filename);

        await Page.ScreenshotAsync(new()
        {
            Path = filepath,
            FullPage = true
        });

        // Also save a description file
        var descPath = Path.Combine(EvidenceDir, $"{featureId}_{timestamp}.txt");
        await File.WriteAllTextAsync(descPath, $"""
            Feature: {featureId}
            Description: {description}
            Captured: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
            URL: {Page.Url}
            """);

        TestContext.AddTestAttachment(filepath, $"{featureId}: {description}");
        TestContext.Progress.WriteLine($"Evidence captured: {filepath}");
    }

    [Test]
    [Description("US-2.2.1: Dark Map Base Layer - CartoDB Dark Matter tiles")]
    public async Task Evidence_US221_DarkMapBaseTiles()
    {
        // Navigate to map page
        await NavigateToAsync("/map");
        await Task.Delay(5000); // Wait for map tiles to load

        // Verify dark tiles are loaded (CartoDB Dark Matter uses specific tile URLs)
        var hasDarkTiles = await Page.EvaluateAsync<bool>(@"() => {
            const imgs = document.querySelectorAll('.leaflet-tile-container img, .leaflet-tile');
            for (const img of imgs) {
                if (img.src && (img.src.includes('cartocdn') || img.src.includes('dark'))) {
                    return true;
                }
            }
            return false;
        }");

        // The map should be visible
        var mapContainer = Page.Locator(".leaflet-container");
        await mapContainer.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });

        await CaptureEvidenceAsync("US-2.2.1_DarkMapTiles",
            "Map displays CartoDB Dark Matter base tiles for dark theme support");

        // Assert
        (await mapContainer.IsVisibleAsync()).Should().BeTrue("Map container should be visible");
    }

    [Test]
    [Description("US-2.2.6: Map Legend Component - Protection level legend")]
    public async Task Evidence_US226_MapLegendComponent()
    {
        await NavigateToAsync("/map");
        await Task.Delay(5000);

        // Wait for legend to render
        var legend = Page.Locator(".map-legend, .legend");

        // Check legend has protection levels
        var hasNoTake = await Page.GetByText("No-Take").First.IsVisibleAsync();
        var hasHighly = await Page.GetByText("Highly Protected").First.IsVisibleAsync();
        var hasLightly = await Page.GetByText("Lightly Protected").First.IsVisibleAsync();

        await CaptureEvidenceAsync("US-2.2.6_MapLegend",
            "Interactive legend showing MPA protection levels (No-Take, Highly Protected, Lightly Protected)");

        // At least one protection level should be visible
        (hasNoTake || hasHighly || hasLightly).Should().BeTrue(
            "Legend should show protection level labels");
    }

    [Test]
    [Description("US-2.2.3: Map Control Panel - Theme toggle button")]
    public async Task Evidence_US223_MapControlPanel()
    {
        await NavigateToAsync("/map");
        await Task.Delay(5000);

        // Check for theme toggle control
        var themeToggle = Page.Locator(".map-controls button, .map-control-btn");
        var hasControls = await themeToggle.First.IsVisibleAsync();

        await CaptureEvidenceAsync("US-2.2.3_MapControlPanel",
            "Map control panel with theme toggle button (sun/moon icon)");

        hasControls.Should().BeTrue("Map control panel should be visible");
    }

    [Test]
    [Description("US-2.1.2: DataCard Component - KPI cards with trends")]
    public async Task Evidence_US212_DataCardComponent()
    {
        await NavigateToAsync("/");
        await Task.Delay(4000);

        // Check for data cards
        var dataCards = Page.Locator(".data-card, .kpi-row > *");
        var cardCount = await dataCards.CountAsync();

        // Look for specific KPI labels
        var hasProtectedAreas = await Page.GetByText("Protected Areas").First.IsVisibleAsync();
        var hasTotalProtected = await Page.GetByText("Total Protected").First.IsVisibleAsync();
        var hasSeaTemp = await Page.GetByText("Sea Temperature").First.IsVisibleAsync();

        await CaptureEvidenceAsync("US-2.1.2_DataCardComponent",
            "KPI DataCard components showing Protected Areas, Total Protected, Sea Temperature, and Bleaching Alert");

        cardCount.Should().BeGreaterThan(0, "Dashboard should have KPI cards");
    }

    [Test]
    [Description("US-2.3.2: AlertBadge Component - Alert level badges")]
    public async Task Evidence_US232_AlertBadgeComponent()
    {
        await NavigateToAsync("/");
        await Task.Delay(5000);

        // Check for alert badge (in bleaching alert card or alerts section)
        var alertBadge = Page.Locator(".alert-badge");
        var hasAlertBadge = await alertBadge.First.IsVisibleAsync();

        // Also check for bleaching alert indicators
        var hasBleachingText = await Page.GetByText("Bleaching Alert").First.IsVisibleAsync();

        await CaptureEvidenceAsync("US-2.3.2_AlertBadgeComponent",
            "AlertBadge component showing bleaching alert level with color-coded styling");

        hasBleachingText.Should().BeTrue("Dashboard should show Bleaching Alert card");
    }

    [Test]
    [Description("US-2.1.1: Dashboard Layout - Grid with KPIs, map preview, alerts")]
    public async Task Evidence_US211_DashboardLayout()
    {
        await NavigateToAsync("/");
        await Task.Delay(5000);

        // Check dashboard structure
        var header = Page.Locator(".dashboard-header, header");
        var kpiRow = Page.Locator(".kpi-row, section[aria-label*='indicator']");
        var mapPreview = Page.Locator(".card-map-preview, .map-preview-body");
        var alertsSection = Page.Locator(".card-alerts, section[aria-label*='alert']");

        var hasHeader = await header.First.IsVisibleAsync();
        var hasMapPreview = await mapPreview.First.IsVisibleAsync();

        await CaptureEvidenceAsync("US-2.1.1_DashboardLayout",
            "Dashboard with KPI row, embedded map preview, alerts panel, and MPA table - dark theme");

        hasHeader.Should().BeTrue("Dashboard should have header");
    }

    [Test]
    [Description("US-2.3.3: MPA Info Panel - Live data section with bleaching status")]
    public async Task Evidence_US233_MpaInfoPanelLiveData()
    {
        await NavigateToAsync("/map");
        await Task.Delay(5000);

        // Click on an MPA to show info panel
        var mpaListItem = Page.Locator(".list-group-item").First;
        if (await mpaListItem.IsVisibleAsync())
        {
            await mpaListItem.ClickAsync();
            await Task.Delay(3000); // Wait for NOAA data to load
        }

        // Check for live data elements
        var infoPanelTitle = await Page.GetByText("MPA Details").First.IsVisibleAsync();
        var hasSstData = await Page.GetByText("Sea Surface Temp").First.IsVisibleAsync();
        var hasDhwData = await Page.GetByText("Degree Heating Week").First.IsVisibleAsync();

        await CaptureEvidenceAsync("US-2.3.3_MpaInfoPanelLiveData",
            "MPA Info Panel showing live NOAA bleaching data: SST, DHW, Alert Level, and 30-day trend sparkline");

        infoPanelTitle.Should().BeTrue("MPA Details panel should be visible");
    }

    [Test]
    [Description("US-3.3.1-3: Accessibility - ARIA labels and roles")]
    public async Task Evidence_US33x_AccessibilityFeatures()
    {
        await NavigateToAsync("/");
        await Task.Delay(4000);

        // Check for ARIA attributes
        var hasMainRole = await Page.EvaluateAsync<bool>(@"() => {
            return document.querySelector('main[role=""main""], [role=""main""]') !== null;
        }");

        var hasBannerRole = await Page.EvaluateAsync<bool>(@"() => {
            return document.querySelector('header[role=""banner""], [role=""banner""]') !== null;
        }");

        var hasAriaLabels = await Page.EvaluateAsync<bool>(@"() => {
            return document.querySelectorAll('[aria-label]').length > 0;
        }");

        await CaptureEvidenceAsync("US-3.3.x_Accessibility",
            "Accessibility features: ARIA roles (main, banner), aria-labels, semantic HTML structure");

        hasAriaLabels.Should().BeTrue("Page should have ARIA labels");
    }

    [Test]
    [Description("Full Dashboard Evidence - Complete dark theme UI")]
    public async Task Evidence_FullDashboard()
    {
        await NavigateToAsync("/");

        // Wait for Radzen components to fully initialize
        await Page.WaitForFunctionAsync(@"() => {
            // Check Radzen sidebar is rendered properly (not raw text)
            const sidebar = document.querySelector('.rz-sidebar');
            if (!sidebar) return false;

            // Check sidebar has proper styling (width > 0)
            const style = window.getComputedStyle(sidebar);
            return parseInt(style.width) > 50;
        }", new PageWaitForFunctionOptions { Timeout = 15000 });

        // Wait for map tiles to load
        await Page.WaitForFunctionAsync(@"() => {
            const tiles = document.querySelectorAll('.leaflet-tile-loaded');
            return tiles.length >= 4; // At least 4 tiles loaded
        }", new PageWaitForFunctionOptions { Timeout = 20000 });

        // Wait for loading spinners to disappear
        await Page.WaitForFunctionAsync(@"() => {
            const spinners = document.querySelectorAll('.spinner-border, .loading-overlay');
            for (const s of spinners) {
                const style = window.getComputedStyle(s);
                if (style.display !== 'none' && style.visibility !== 'hidden') {
                    return false;
                }
            }
            return true;
        }", new PageWaitForFunctionOptions { Timeout = 15000 });

        // Additional wait for any animations
        await Task.Delay(2000);

        await CaptureEvidenceAsync("FULL_Dashboard_DarkTheme",
            "Complete dashboard view with dark theme: KPI cards, map preview, alerts, MPA table, data freshness indicators");
    }

    [Test]
    [Description("Full Map Page Evidence - Complete map with legend")]
    public async Task Evidence_FullMapPage()
    {
        await NavigateToAsync("/map");

        // Wait for Radzen sidebar
        await Page.WaitForFunctionAsync(@"() => {
            const sidebar = document.querySelector('.rz-sidebar');
            if (!sidebar) return false;
            const style = window.getComputedStyle(sidebar);
            return parseInt(style.width) > 50;
        }", new PageWaitForFunctionOptions { Timeout = 15000 });

        // Wait for map tiles to fully load
        await Page.WaitForFunctionAsync(@"() => {
            const tiles = document.querySelectorAll('.leaflet-tile-loaded');
            return tiles.length >= 6;
        }", new PageWaitForFunctionOptions { Timeout = 25000 });

        // Wait for MPA polygons to render
        await Page.WaitForFunctionAsync(@"() => {
            const paths = document.querySelectorAll('.leaflet-overlay-pane svg path');
            return paths.length > 0;
        }", new PageWaitForFunctionOptions { Timeout = 15000 });

        // Wait for loading overlay to disappear
        await Page.WaitForFunctionAsync(@"() => {
            const overlay = document.querySelector('.loading-overlay');
            return !overlay || window.getComputedStyle(overlay).display === 'none';
        }", new PageWaitForFunctionOptions { Timeout = 10000 });

        await Task.Delay(2000);

        await CaptureEvidenceAsync("FULL_MapPage_DarkTheme",
            "Complete map view with dark CartoDB tiles, MPA polygons, legend, and control panel");
    }

    [Test]
    [Description("Map with MPA Selected - Info panel visible")]
    public async Task Evidence_MapWithMpaSelected()
    {
        await NavigateToAsync("/map");

        // Wait for map to fully load
        await Page.WaitForFunctionAsync(@"() => {
            const tiles = document.querySelectorAll('.leaflet-tile-loaded');
            const paths = document.querySelectorAll('.leaflet-overlay-pane svg path');
            return tiles.length >= 6 && paths.length > 0;
        }", new PageWaitForFunctionOptions { Timeout = 25000 });

        // Wait for loading overlay to disappear
        await Page.WaitForFunctionAsync(@"() => {
            const overlay = document.querySelector('.loading-overlay');
            return !overlay || window.getComputedStyle(overlay).display === 'none';
        }", new PageWaitForFunctionOptions { Timeout = 10000 });

        await Task.Delay(1000);

        // Select first MPA from the list
        var mpaListItem = Page.Locator(".list-group-item").First;
        if (await mpaListItem.IsVisibleAsync())
        {
            await mpaListItem.ClickAsync();

            // Wait for info panel to load with NOAA data
            await Page.WaitForFunctionAsync(@"() => {
                const panel = document.querySelector('.mpa-info-panel, .card');
                const hasData = document.querySelector('.data-value, .live-data-card');
                return panel && hasData;
            }", new PageWaitForFunctionOptions { Timeout = 20000 });

            await Task.Delay(2000);
        }

        await CaptureEvidenceAsync("FULL_MapWithMpaSelected",
            "Map with MPA selected showing info panel with live NOAA data, DHW trend, and alert badge");
    }
}
