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

        // Set data-theme attribute (used by CSS selectors)
        html.setAttribute('data-theme', mode);

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

// ==========================================================================
// Keyboard Shortcuts Manager
// ==========================================================================
window.CoralLedgerKeyboardShortcuts = {
    enabled: true,
    helpVisible: false,
    initialized: false,

    shortcuts: {
        // Navigation shortcuts
        'd': { description: 'Go to Dashboard', action: function() { window.location.href = '/'; } },
        'm': { description: 'Go to Map', action: function() { window.location.href = '/map'; } },
        'b': { description: 'Go to Bleaching Status', action: function() { window.location.href = '/bleaching'; } },

        // Theme toggle
        't': { description: 'Toggle theme (dark/light)', action: function() {
            var currentMode = window.CoralLedgerThemeManager.getMode();
            var newMode = currentMode === 'dark' ? 'light' : 'dark';
            window.CoralLedgerThemeManager.setMode(newMode);
            // Click the theme toggle button to sync Blazor state
            var themeBtn = document.querySelector('.theme-toggle');
            if (themeBtn) themeBtn.click();
        }},

        // Quick actions
        's': { description: 'Sync data', action: function() {
            var syncBtn = document.querySelector('button:has(.bi-arrow-repeat), button[title*="Sync"]');
            if (syncBtn && !syncBtn.disabled) syncBtn.click();
        }},

        // Help
        '?': { description: 'Show keyboard shortcuts', action: function() {
            window.CoralLedgerKeyboardShortcuts.toggleHelp();
        }},

        // Escape to close modals/panels
        'Escape': { description: 'Close dialogs/help', action: function() {
            window.CoralLedgerKeyboardShortcuts.hideHelp();
            // Close any open Radzen dialogs
            var closeBtn = document.querySelector('.rz-dialog-close');
            if (closeBtn) closeBtn.click();
        }}
    },

    init: function() {
        // Prevent double initialization
        if (this.initialized) {
            console.log('[KeyboardShortcuts] Already initialized, skipping.');
            return;
        }
        this.initialized = true;

        var self = this;
        document.addEventListener('keydown', function(e) {
            // Don't trigger shortcuts when typing in inputs
            if (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA' || e.target.isContentEditable) {
                // Only allow Escape in inputs
                if (e.key !== 'Escape') return;
            }

            // Don't trigger if modifier keys are pressed (except for ?)
            if (e.ctrlKey || e.altKey || e.metaKey) return;

            var key = e.key;
            var shortcut = self.shortcuts[key];

            if (shortcut && self.enabled) {
                e.preventDefault();
                shortcut.action();
            }
        });

        // Create help dialog
        this.createHelpDialog();

        console.log('[KeyboardShortcuts] Initialized. Press ? for help.');
    },

    createHelpDialog: function() {
        // Check if dialog already exists (prevents duplicates)
        if (document.getElementById('keyboard-shortcuts-help')) {
            return;
        }
        var dialog = document.createElement('div');
        dialog.id = 'keyboard-shortcuts-help';
        dialog.setAttribute('role', 'dialog');
        dialog.setAttribute('aria-modal', 'true');
        dialog.setAttribute('aria-labelledby', 'shortcuts-help-title');
        dialog.innerHTML = `
            <div class="shortcuts-overlay" onclick="CoralLedgerKeyboardShortcuts.hideHelp()"></div>
            <div class="shortcuts-content">
                <h3 id="shortcuts-help-title">Keyboard Shortcuts</h3>
                <button class="shortcuts-close" onclick="CoralLedgerKeyboardShortcuts.hideHelp()" aria-label="Close">×</button>
                <div class="shortcuts-list">
                    <div class="shortcuts-section">
                        <h4>Navigation</h4>
                        <div class="shortcut-row"><kbd>D</kbd> <span>Go to Dashboard</span></div>
                        <div class="shortcut-row"><kbd>M</kbd> <span>Go to Map</span></div>
                        <div class="shortcut-row"><kbd>B</kbd> <span>Go to Bleaching Status</span></div>
                    </div>
                    <div class="shortcuts-section">
                        <h4>Actions</h4>
                        <div class="shortcut-row"><kbd>T</kbd> <span>Toggle theme</span></div>
                        <div class="shortcut-row"><kbd>S</kbd> <span>Sync data</span></div>
                    </div>
                    <div class="shortcuts-section">
                        <h4>General</h4>
                        <div class="shortcut-row"><kbd>?</kbd> <span>Show this help</span></div>
                        <div class="shortcut-row"><kbd>Esc</kbd> <span>Close dialogs</span></div>
                    </div>
                </div>
            </div>
        `;
        document.body.appendChild(dialog);
    },

    toggleHelp: function() {
        this.helpVisible ? this.hideHelp() : this.showHelp();
    },

    showHelp: function() {
        var dialog = document.getElementById('keyboard-shortcuts-help');
        if (dialog) {
            dialog.classList.add('visible');
            this.helpVisible = true;
            // Focus the close button for accessibility
            var closeBtn = dialog.querySelector('.shortcuts-close');
            if (closeBtn) closeBtn.focus();
        }
    },

    hideHelp: function() {
        var dialog = document.getElementById('keyboard-shortcuts-help');
        if (dialog) {
            dialog.classList.remove('visible');
            this.helpVisible = false;
        }
    }
};

// Initialize keyboard shortcuts on DOM ready
document.addEventListener('DOMContentLoaded', function() {
    window.CoralLedgerKeyboardShortcuts.init();
});

if (document.readyState !== 'loading') {
    window.CoralLedgerKeyboardShortcuts.init();
}
