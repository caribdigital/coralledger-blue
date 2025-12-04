// Leaflet Map Interop for CoralLedger Blue
window.leafletMap = {
    maps: {},
    mpaLayers: {},
    fishingLayers: {},

    // Check if Leaflet is loaded
    isLeafletReady: function() {
        return typeof L !== 'undefined';
    },

    // Initialize a new map
    initialize: function (mapId, centerLat, centerLng, zoom) {
        // Check if Leaflet is loaded
        if (!this.isLeafletReady()) {
            console.error('Leaflet library (L) is not loaded. Make sure leaflet.js is included before leaflet-map.js');
            throw new Error('Leaflet library not loaded');
        }
        if (this.maps[mapId]) {
            this.maps[mapId].remove();
        }

        const map = L.map(mapId).setView([centerLat, centerLng], zoom);

        // Add OpenStreetMap tiles
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
        }).addTo(map);

        this.maps[mapId] = map;
        return true;
    },

    // Add GeoJSON MPA layer
    addMpaLayer: function (mapId, geojsonData, dotNetHelper) {
        const map = this.maps[mapId];
        if (!map) return false;

        // Remove existing MPA layer
        if (this.mpaLayers[mapId]) {
            map.removeLayer(this.mpaLayers[mapId]);
        }

        const getColor = (protectionLevel) => {
            switch (protectionLevel) {
                case 'NoTake': return '#dc3545';
                case 'HighlyProtected': return '#fd7e14';
                case 'LightlyProtected': return '#0dcaf0';
                default: return '#6c757d';
            }
        };

        const style = (feature) => ({
            fillColor: getColor(feature.properties.ProtectionLevel),
            weight: 2,
            opacity: 1,
            color: getColor(feature.properties.ProtectionLevel),
            fillOpacity: 0.4
        });

        const highlightStyle = {
            weight: 4,
            color: '#ffc107',
            fillOpacity: 0.6
        };

        const layer = L.geoJSON(geojsonData, {
            style: style,
            onEachFeature: (feature, layer) => {
                // Popup with MPA info
                const props = feature.properties;
                layer.bindPopup(`
                    <strong>${props.Name}</strong><br/>
                    <small>${props.IslandGroup}</small><br/>
                    <span class="badge" style="background-color: ${getColor(props.ProtectionLevel)}; color: white;">
                        ${props.ProtectionLevel}
                    </span><br/>
                    <small>Area: ${props.AreaSquareKm.toFixed(1)} km²</small>
                `);

                layer.on({
                    mouseover: (e) => {
                        e.target.setStyle(highlightStyle);
                        e.target.bringToFront();
                    },
                    mouseout: (e) => {
                        this.mpaLayers[mapId].resetStyle(e.target);
                    },
                    click: (e) => {
                        map.fitBounds(e.target.getBounds(), { padding: [50, 50] });
                        if (dotNetHelper) {
                            dotNetHelper.invokeMethodAsync('OnMpaClicked', feature.id);
                        }
                    }
                });
            }
        }).addTo(map);

        this.mpaLayers[mapId] = layer;

        // Fit map to show all MPAs
        if (layer.getBounds().isValid()) {
            map.fitBounds(layer.getBounds(), { padding: [50, 50] });
        }

        return true;
    },

    // Add fishing events layer
    addFishingEventsLayer: function (mapId, fishingEvents, dotNetHelper) {
        const map = this.maps[mapId];
        if (!map) return false;

        // Remove existing fishing layer
        if (this.fishingLayers[mapId]) {
            map.removeLayer(this.fishingLayers[mapId]);
        }

        const markers = L.layerGroup();

        fishingEvents.forEach(evt => {
            const daysAgo = (Date.now() - new Date(evt.startTime).getTime()) / (1000 * 60 * 60 * 24);
            let color;
            if (daysAgo < 7) color = '#dc3545';
            else if (daysAgo < 14) color = '#fd7e14';
            else if (daysAgo < 30) color = '#ffc107';
            else color = '#6c757d';

            const isViolation = evt.isInMpa === true;
            const radius = isViolation ? 8 : 6;
            const borderColor = isViolation ? '#ff0000' : '#ffffff';
            const borderWeight = isViolation ? 3 : 2;

            const marker = L.circleMarker([evt.latitude, evt.longitude], {
                radius: radius,
                fillColor: color,
                color: borderColor,
                weight: borderWeight,
                opacity: 1,
                fillOpacity: 0.8
            });

            let popupContent = `
                <strong><i class="bi bi-water"></i> Fishing Event</strong>
                ${isViolation ? '<span class="badge bg-danger ms-2">MPA Violation</span>' : ''}
                <hr style="margin: 4px 0;"/>
                <small><strong>Vessel:</strong> ${evt.vesselName || evt.vesselId}</small><br/>
                <small><strong>Date:</strong> ${new Date(evt.startTime).toLocaleDateString()}</small>
            `;
            if (evt.durationHours) {
                popupContent += `<br/><small><strong>Duration:</strong> ${evt.durationHours.toFixed(1)} hours</small>`;
            }
            if (isViolation && evt.mpaName) {
                popupContent += `<br/><small class="text-danger"><strong>Inside:</strong> ${evt.mpaName}</small>`;
            }

            marker.bindPopup(popupContent);

            marker.on('click', () => {
                if (dotNetHelper) {
                    dotNetHelper.invokeMethodAsync('OnFishingEventClicked', evt.eventId);
                }
            });

            markers.addLayer(marker);
        });

        markers.addTo(map);
        this.fishingLayers[mapId] = markers;

        return true;
    },

    // Remove fishing events layer
    removeFishingEventsLayer: function (mapId) {
        const map = this.maps[mapId];
        if (!map) return false;

        if (this.fishingLayers[mapId]) {
            map.removeLayer(this.fishingLayers[mapId]);
            delete this.fishingLayers[mapId];
        }

        return true;
    },

    // Zoom to specific MPA by ID
    zoomToMpa: function (mapId, mpaId) {
        const map = this.maps[mapId];
        const mpaLayer = this.mpaLayers[mapId];
        if (!map || !mpaLayer) return false;

        let found = false;
        mpaLayer.eachLayer((layer) => {
            if (layer.feature && layer.feature.id === mpaId) {
                map.fitBounds(layer.getBounds(), { padding: [50, 50] });
                layer.openPopup();
                found = true;
            }
        });

        return found;
    },

    // Highlight specific MPA
    highlightMpa: function (mapId, mpaId) {
        const mpaLayer = this.mpaLayers[mapId];
        if (!mpaLayer) return false;

        mpaLayer.eachLayer((layer) => {
            if (layer.feature && layer.feature.id === mpaId) {
                layer.setStyle({
                    weight: 4,
                    color: '#ffc107',
                    fillOpacity: 0.6
                });
                layer.bringToFront();
            } else if (layer.feature) {
                mpaLayer.resetStyle(layer);
            }
        });

        return true;
    },

    // Dispose map
    dispose: function (mapId) {
        if (this.maps[mapId]) {
            this.maps[mapId].remove();
            delete this.maps[mapId];
            delete this.mpaLayers[mapId];
            delete this.fishingLayers[mapId];
        }
    }
};
