/**
 * Observation Map - Location picker for citizen observation submissions
 */

window.observationMap = {
    map: null,
    marker: null,
    dotNetRef: null,

    /**
     * Initialize the observation location picker map
     */
    initialize: function(containerId, dotNetRef) {
        this.dotNetRef = dotNetRef;

        // Initialize map centered on Bahamas
        this.map = L.map(containerId, {
            center: [24.5, -77.5],
            zoom: 7,
            zoomControl: true
        });

        // Add tile layer
        const isDark = document.documentElement.getAttribute('data-theme') !== 'light';
        const tileUrl = isDark
            ? 'https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png'
            : 'https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png';

        L.tileLayer(tileUrl, {
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> &copy; <a href="https://carto.com/attributions">CARTO</a>',
            subdomains: 'abcd',
            maxZoom: 18
        }).addTo(this.map);

        // Add click handler for location selection
        this.map.on('click', (e) => this.handleMapClick(e));

        // Load MPA boundaries for context
        this.loadMpaBoundaries();
    },

    /**
     * Handle map click to select location
     */
    handleMapClick: async function(e) {
        const lat = e.latlng.lat;
        const lng = e.latlng.lng;

        // Update or create marker
        if (this.marker) {
            this.marker.setLatLng([lat, lng]);
        } else {
            this.marker = L.marker([lat, lng], {
                icon: L.divIcon({
                    className: 'observation-marker',
                    html: '<div class="marker-pin"><span class="material-icons">place</span></div>',
                    iconSize: [30, 42],
                    iconAnchor: [15, 42]
                })
            }).addTo(this.map);
        }

        // Check if location is in an MPA
        let mpaName = null;
        if (this.mpaLayer) {
            this.mpaLayer.eachLayer((layer) => {
                if (layer.getBounds && layer.getBounds().contains([lat, lng])) {
                    // Simple bounding box check - actual containment would need point-in-polygon
                    mpaName = layer.feature?.properties?.Name || null;
                }
            });
        }

        // Notify Blazor
        if (this.dotNetRef) {
            await this.dotNetRef.invokeMethodAsync('OnLocationSelected', lat, lng, mpaName);
        }
    },

    /**
     * Use browser geolocation to get current position
     */
    useCurrentLocation: function() {
        if (!navigator.geolocation) {
            console.error('Geolocation not supported');
            return;
        }

        navigator.geolocation.getCurrentPosition(
            async (position) => {
                const lat = position.coords.latitude;
                const lng = position.coords.longitude;

                // Center map on location
                this.map.setView([lat, lng], 12);

                // Trigger click event to select location
                this.handleMapClick({ latlng: { lat, lng } });
            },
            (error) => {
                console.error('Geolocation error:', error);
            },
            {
                enableHighAccuracy: true,
                timeout: 10000
            }
        );
    },

    /**
     * Load MPA boundaries for context
     */
    loadMpaBoundaries: async function() {
        try {
            const response = await fetch('/api/mpas/geojson?simplified=medium');
            if (response.ok) {
                const geojson = await response.json();
                this.mpaLayer = L.geoJSON(geojson, {
                    style: {
                        fillColor: '#00E5CC',
                        fillOpacity: 0.1,
                        weight: 1,
                        color: '#00E5CC',
                        opacity: 0.5
                    }
                }).addTo(this.map);
            }
        } catch (error) {
            console.log('Could not load MPA boundaries:', error);
        }
    },

    /**
     * Dispose map instance
     */
    dispose: function() {
        if (this.map) {
            this.map.remove();
            this.map = null;
            this.marker = null;
            this.mpaLayer = null;
        }
    }
};
