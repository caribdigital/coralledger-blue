// Theme Manager for CoralLedger Blue
window.CoralLedgerThemeManager = {
    storageKey: 'coralledger-theme',

    getMode: function() {
        return localStorage.getItem(this.storageKey) || 'dark';
    },

    setMode: function(mode) {
        localStorage.setItem(this.storageKey, mode);
        this.applyMode(mode);
    },

    applyMode: function(mode) {
        const html = document.documentElement;
        const body = document.body;

        // Remove existing theme classes
        html.classList.remove('theme-dark', 'theme-light');
        body.classList.remove('theme-dark', 'theme-light');

        // Add new theme class
        html.classList.add('theme-' + mode);
        body.classList.add('theme-' + mode);

        // Update CSS custom properties for theme
        if (mode === 'light') {
            html.style.setProperty('--bg-primary', '#f8f9fa');
            html.style.setProperty('--bg-secondary', '#ffffff');
            html.style.setProperty('--bg-tertiary', '#e9ecef');
            html.style.setProperty('--text-primary', '#212529');
            html.style.setProperty('--text-secondary', '#495057');
            html.style.setProperty('--text-muted', '#6c757d');
            html.style.setProperty('--border-color', '#dee2e6');
            html.style.setProperty('--card-bg', '#ffffff');
            html.style.setProperty('--sidebar-bg', '#f8f9fa');
        } else {
            // Dark mode (default)
            html.style.setProperty('--bg-primary', '#0a1929');
            html.style.setProperty('--bg-secondary', '#0d2137');
            html.style.setProperty('--bg-tertiary', '#132f4c');
            html.style.setProperty('--text-primary', '#ffffff');
            html.style.setProperty('--text-secondary', '#b2bac2');
            html.style.setProperty('--text-muted', '#8b949e');
            html.style.setProperty('--border-color', '#1e4976');
            html.style.setProperty('--card-bg', '#0d2137');
            html.style.setProperty('--sidebar-bg', '#071318');
        }

        // Update Radzen theme if available
        this.updateRadzenTheme(mode);

        // Update map tiles if leaflet map exists
        console.log('[ThemeManager] applyMode called with mode: ' + mode);
        if (window.leafletMap && typeof window.leafletMap.setTileTheme === 'function') {
            // Update all active maps
            var mapIds = Object.keys(window.leafletMap.maps);
            console.log('[ThemeManager] Updating ' + mapIds.length + ' maps to theme: ' + mode);
            mapIds.forEach(function(mapId) {
                console.log('[ThemeManager] Switching map ' + mapId + ' to ' + mode);
                var result = window.leafletMap.setTileTheme(mapId, mode);
                console.log('[ThemeManager] setTileTheme result: ' + result);
            });
        } else {
            console.log('[ThemeManager] No leafletMap found or setTileTheme not available. maps:', window.leafletMap ? Object.keys(window.leafletMap.maps) : 'undefined');
        }
    },

    updateRadzenTheme: function(mode) {
        // Find existing Radzen stylesheet
        const radzenLink = document.querySelector('link[href*="Radzen.Blazor/css/material"]');
        if (radzenLink) {
            const newHref = mode === 'light'
                ? '_content/Radzen.Blazor/css/material-base.css'
                : '_content/Radzen.Blazor/css/material-dark-base.css';

            if (!radzenLink.href.endsWith(newHref)) {
                radzenLink.href = newHref;
            }
        }
    },

    init: function() {
        const savedMode = this.getMode();
        this.applyMode(savedMode);
    }
};

// Initialize theme on page load
document.addEventListener('DOMContentLoaded', function() {
    window.CoralLedgerThemeManager.init();
});

// Also initialize immediately if DOM is already ready
if (document.readyState !== 'loading') {
    window.CoralLedgerThemeManager.init();
}

// Listen for storage changes (for cross-tab sync and Blazor navigation)
window.addEventListener('storage', function(e) {
    if (e.key === 'coralledger-theme' && e.newValue) {
        console.log('[ThemeManager] Storage event detected, switching to:', e.newValue);
        window.CoralLedgerThemeManager.applyMode(e.newValue);
    }
});

// Periodically check for theme changes (backup for single-tab scenarios)
setInterval(function() {
    var currentTheme = window.CoralLedgerThemeManager.getMode();
    var appliedTheme = document.documentElement.classList.contains('theme-light') ? 'light' : 'dark';
    if (currentTheme !== appliedTheme) {
        console.log('[ThemeManager] Theme mismatch detected. localStorage:', currentTheme, 'DOM:', appliedTheme);
        window.CoralLedgerThemeManager.applyMode(currentTheme);
    }
}, 500);
