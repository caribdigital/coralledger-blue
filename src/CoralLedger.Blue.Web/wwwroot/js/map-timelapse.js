/**
 * Map Time-Lapse - Animated temporal data visualization
 */

window.mapTimeLapse = {
    bleachingLayer: null,
    fishingLayer: null,
    vesselLayer: null,
    heatmapLayer: null,

    /**
     * Update bleaching data layer for a specific date
     */
    updateBleachingLayer: async function(mapId, date, stats) {
        const map = window.leafletMap?.maps?.[mapId];
        if (!map) return;

        // Remove existing layer
        if (this.bleachingLayer) {
            map.removeLayer(this.bleachingLayer);
        }

        try {
            // Fetch bleaching data for date
            const response = await fetch(`/api/bleaching/bahamas?date=${date}`);
            if (!response.ok) {
                console.log('No bleaching data for date:', date);
                return;
            }

            const data = await response.json();
            if (!data || !data.length) return;

            // Create heatmap-style visualization
            const points = data.map(d => ({
                lat: d.latitude,
                lng: d.longitude,
                value: d.degreeHeatingWeek || 0,
                sst: d.seaSurfaceTemperature,
                alertLevel: d.alertLevel
            }));

            // Create circle markers with DHW-based colors
            this.bleachingLayer = L.layerGroup();

            points.forEach(p => {
                const color = this.getDhwColor(p.value);
                const radius = Math.max(5000, p.value * 2000); // 5km to 25km based on DHW

                L.circle([p.lat, p.lng], {
                    radius: radius,
                    fillColor: color,
                    fillOpacity: 0.4,
                    stroke: false
                }).bindPopup(`
                    <div class="bleaching-popup">
                        <strong>Bleaching Data</strong><br>
                        SST: ${p.sst?.toFixed(1) || 'N/A'}°C<br>
                        DHW: ${p.value?.toFixed(1) || '0'} °C-weeks<br>
                        Alert Level: ${p.alertLevel || 0}
                    </div>
                `).addTo(this.bleachingLayer);
            });

            this.bleachingLayer.addTo(map);

        } catch (error) {
            console.error('Error loading bleaching data:', error);
        }
    },

    /**
     * Update fishing events layer for a specific date
     */
    updateFishingLayer: async function(mapId, date, stats) {
        const map = window.leafletMap?.maps?.[mapId];
        if (!map) return;

        // Remove existing layer
        if (this.fishingLayer) {
            map.removeLayer(this.fishingLayer);
        }

        try {
            // Fetch fishing events for date range (date to date+1)
            const startDate = date;
            const endDate = new Date(new Date(date).getTime() + 86400000).toISOString().split('T')[0];

            const response = await fetch(`/api/vessels/fishing-events/bahamas?startDate=${startDate}&endDate=${endDate}`);
            if (!response.ok) {
                console.log('No fishing data for date:', date);
                return;
            }

            const events = await response.json();
            if (!events || !events.length) return;

            this.fishingLayer = L.layerGroup();

            events.forEach(evt => {
                const isViolation = evt.isInMpa === true;
                const color = isViolation ? '#ef4444' : '#58a6ff';

                const marker = L.circleMarker([evt.latitude, evt.longitude], {
                    radius: 6,
                    fillColor: color,
                    fillOpacity: 0.8,
                    weight: isViolation ? 2 : 1,
                    color: isViolation ? '#ff0000' : '#ffffff',
                    opacity: 0.8
                }).bindPopup(`
                    <div class="fishing-popup">
                        <strong>${evt.vesselName || 'Unknown Vessel'}</strong><br>
                        Type: ${evt.eventType || 'Fishing'}<br>
                        Duration: ${evt.durationHours?.toFixed(1) || 'N/A'} hours<br>
                        ${isViolation ? '<span style="color:#ef4444">⚠️ Inside MPA</span>' : ''}
                    </div>
                `).addTo(this.fishingLayer);
            });

            this.fishingLayer.addTo(map);

        } catch (error) {
            console.error('Error loading fishing data:', error);
        }
    },

    /**
     * Update vessel positions layer for a specific date
     */
    updateVesselLayer: async function(mapId, date) {
        const map = window.leafletMap?.maps?.[mapId];
        if (!map) return;

        // Remove existing layer
        if (this.vesselLayer) {
            map.removeLayer(this.vesselLayer);
        }

        try {
            const response = await fetch(`/api/vessels/positions?date=${date}`);
            if (!response.ok) return;

            const vessels = await response.json();
            if (!vessels || !vessels.length) return;

            this.vesselLayer = L.layerGroup();

            vessels.forEach(v => {
                const icon = L.divIcon({
                    className: 'vessel-icon',
                    html: `<div class="vessel-marker" style="transform: rotate(${v.heading || 0}deg)">
                        <span class="material-icons">navigation</span>
                    </div>`,
                    iconSize: [20, 20],
                    iconAnchor: [10, 10]
                });

                L.marker([v.latitude, v.longitude], { icon })
                    .bindPopup(`
                        <strong>${v.name || 'Unknown'}</strong><br>
                        Flag: ${v.flag || 'Unknown'}<br>
                        Type: ${v.vesselType || 'Unknown'}<br>
                        Speed: ${v.speed?.toFixed(1) || 'N/A'} knots
                    `)
                    .addTo(this.vesselLayer);
            });

            this.vesselLayer.addTo(map);

        } catch (error) {
            console.error('Error loading vessel data:', error);
        }
    },

    /**
     * Get color based on DHW value
     */
    getDhwColor: function(dhw) {
        if (dhw >= 8) return '#ef4444';      // Critical - bright red
        if (dhw >= 4) return '#fd7e14';      // High - orange
        if (dhw >= 1) return '#f7c549';      // Medium - yellow
        if (dhw > 0) return '#22c55e';       // Low - green
        return '#58a6ff';                     // No stress - blue
    },

    /**
     * Clear all timelapse layers
     */
    clearAllLayers: function(mapId) {
        const map = window.leafletMap?.maps?.[mapId];
        if (!map) return;

        if (this.bleachingLayer) {
            map.removeLayer(this.bleachingLayer);
            this.bleachingLayer = null;
        }
        if (this.fishingLayer) {
            map.removeLayer(this.fishingLayer);
            this.fishingLayer = null;
        }
        if (this.vesselLayer) {
            map.removeLayer(this.vesselLayer);
            this.vesselLayer = null;
        }
    }
};
