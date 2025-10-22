window.VisualProposalDesignerMap = (function () {
    let map;
    let dotnetRef = null;

    let gisLayers = {};
    let currentRevsion = {};
    let scratchFeatures = [];

    let resizeHandlerAdded = false;

    let drawControl, drawnItems;

    const theme = { bg: "#373735", fg: "#EAEAEA", accent: "#F3B133" };

    const layerColors = {
        ElectricFacilityFence: "#0000FF", // teal polygons
        UGLineSegment: "#4FC3F7",         // light blue
        OHLineSegment: "#F06292"          // pink
    };

    function init() {
        const container = document.getElementById('map');
        if (!container) return;

        // If a map exists but is bound to a dead/old container, remove it
        if (map && (!map._container || map._container !== container)) {
            dispose();
        }

        // If map is already bound to the current container, ensure size is recalculated and return
        if (map) {
            try { map.invalidateSize(); } catch (e) { /* no-op */ }
            return;
        }

        map = L.map(container, { zoomControl: true }).setView([37.773972, -122.431297], 12);

        initializeMapTitles();

        attachScratchHandler();

        // When map is ready, ensure it recalculates layout (fixes tiles not showing after hidden/resize)
        try {
            map.whenReady(() => {
                try { map.invalidateSize(); } catch (e) { /* no-op */ }
            });
        } catch (e) { /* older leaflet safety */ }

        // Also run a delayed invalidate in case layout changes take longer
        setTimeout(() => {
            try { if (map) map.invalidateSize(); } catch (e) { /* no-op */ }
        }, 200);

        // Only add a single resize listener once
        if (!resizeHandlerAdded) {
            window.addEventListener('resize', () => {
                try { if (map) map.invalidateSize(); } catch (e) { /* no-op */ }
            });
            resizeHandlerAdded = true;
        }
    }

    function dispose() {
        try {
            if (map) {
                map.remove();
            }
        } catch (e) { }

        map = null;

        gisLayers = {};
        currentRevsion = {};

        scratchFeatures = [];
        notifyScratchChanged();

        drawControl = null;
        drawnItems = null;
    }

    function initializeMapTitles() {
        // Base layers
        const osm = L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '&copy; OpenStreetMap contributors'
        });

        const esriSat = L.tileLayer('https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}', {
            maxZoom: 19,
            attribution: 'Tiles &copy; Esri  Source: Esri and contributors'
        });

        const esriLabels = L.tileLayer(
            'https://services.arcgisonline.com/ArcGIS/rest/services/Reference/World_Boundaries_and_Places/MapServer/tile/{z}/{y}/{x}',
            { maxZoom: 19, attribution: 'Labels &copy; Esri' }
        );

        // Start with satellite (or switch to `osm.addTo(map)` if you prefer)
        esriSat.addTo(map);
        // Add after the basemap so it sits on top
        esriLabels.addTo(map);

        // Layer chooser
        L.control.layers(
            { "Satellite": esriSat, "Street (OSM)": osm },
            { "Labels": esriLabels },
            { position: 'topright', collapsed: false }
        ).addTo(map);
    }

    function attachScratchHandler() {
        drawnItems = new L.FeatureGroup();
        map.addLayer(drawnItems);

        map.on(L.Draw.Event.CREATED, function (evt) {
            const layer = evt.layer;
            styleLayer(layer);
            drawnItems.addLayer(layer);
            scratchFeatures.push(layer.toGeoJSON());
            notifyScratchChanged();
        });
    }

    function styleLayer(layer) {
        if (layer.setStyle) layer.setStyle({ color: theme.accent, weight: 3 });
        if (layer instanceof L.Marker) {
            const icon = L.divIcon({
                className: 'custom-marker',
                html: `<div style="width:12px;height:12px;border-radius:6px;background:${theme.accent};border:1px solid ${theme.accent}"></div>`,
                iconSize: [12, 12]
            });
            layer.setIcon(icon);
        }
    }

    function addOrReplaceGeoJsonLayer(key, geoJsonString, styleObj) {
        if (!map) throw new Error("Map not initialized");

        try {
            const data = JSON.parse(geoJsonString);

            // remove existing layer
            if (gisLayers[key]) {
                try { map.removeLayer(gisLayers[key]); } catch (e) { /* no-op */ }
                delete gisLayers[key];
            }

            gisLayers[key] = L.geoJSON(data, {
                style: (feature) => {
                    const layerName = feature?.properties?.layer || key;
                    const color = layerColors[layerName] || styleObj?.color || theme.accent;
                    return {
                        color,
                        weight: styleObj?.weight ?? 3,
                        fillOpacity: styleObj?.fillOpacity ?? 0.3,
                        dashArray: styleObj?.dashArray
                    };
                },
                onEachFeature: (feature, layer) => {
                    const props = feature.properties || {};
                    const label =
                        props.name_ets ??
                        props.tline_nm ??
                        props.layer ??
                        "Feature";
                    layer.bindTooltip(label);
                },
                pointToLayer: (feature, latlng) => {
                    const layerName = feature?.properties?.layer || key;
                    const color = layerColors[layerName] || theme.accent;
                    return L.circleMarker(latlng, {
                        radius: 5,
                        color,
                        fillColor: color,
                        fillOpacity: 1,
                        weight: 2
                    });
                }
            }).addTo(map);

            // ensure newly-added layers trigger a size recompute (in some cases tiles need reflow)
            try { map.invalidateSize(); } catch (e) { /* no-op */ }
        } catch (err) {
            console.error("Invalid GeoJSON for layer:", key, err);
        }
    }

    function updateCurrentRevision(json) {
        init();

        for (const key in currentRevsion) {
            currentRevsion[key]?.remove?.();
        }

        currentRevsion = {};

        if (!json) return;

        console.log(json);

        let revs;
        try { revs = JSON.parse(json); } catch { return; }

        console.log("showRevisions: revs=", revs.length,
            "visible features:", revs.reduce((a, r) => a + ((r.features || r.Features || []).length), 0));

        const parseGeo = (raw) => {
            if (!raw) return null;
            if (typeof raw === "string") { try { return JSON.parse(raw); } catch { return null; } }
            if (typeof raw === "object") return raw; // already parsed
            return null;
        };

        revs.forEach(revision => {
            const group = L.featureGroup();

            (revision.features || revision.Features || []).forEach(f => {
                // tolerate camelCase/PascalCase and string/object payloads
                const gj = parseGeo(f.geoJson || f.GeoJson);
                if (!gj) return;

                const layer = L.geoJSON(gj, {
                    style: (feature) => {
                        const c = feature?.properties?.color || theme.accent;
                        return { color: c, weight: 3 };
                    },
                    pointToLayer: (feature, latlng) => {
                        const c = feature?.properties?.color || theme.accent;
                        return L.circleMarker(latlng, {
                            radius: 6, color: c, weight: 2, fillOpacity: 1, fillColor: c
                        });
                    }
                });

                group.addLayer(layer);
            });

            group.addTo(map);
            currentRevsion[revision.id || revision.Id] = group;
        });

        try { map.invalidateSize(); } catch { }
        try { fitToVisible(); } catch { }
    }

    // Expose an explicit invalidate function for Blazor to call if desired
    function invalidateSize() {
        try { if (map) map.invalidateSize(); } catch (e) { /* no-op */ }
    }

    function startDrawing(kind, colorHex) {
        init();
        if (drawControl) { map.removeControl(drawControl); }

        const color = (typeof colorHex === 'string' && colorHex.trim()) ? colorHex : theme.accent;

        const opts = { draw: { polyline: false, polygon: false, marker: false, rectangle: false, circle: false, circlemarker: false }, edit: false };

        if (kind === 'polyline') opts.draw.polyline = { shapeOptions: { color, weight: 3 } };
        if (kind === 'polygon') opts.draw.polygon = { shapeOptions: { color, weight: 3 } };
        if (kind === 'marker') opts.draw.marker = {};

        drawControl = new L.Control.Draw(opts);
        map.addControl(drawControl);
    }

    // New: remove/hide the draw controls from the map.
    // Use this from Blazor: `VisualProposalDesignerMap.removeDrawControls()` (or `stopDrawing()` alias).
    function removeDrawControls() {
        try {
            if (drawControl && map) {
                try { map.removeControl(drawControl); } catch (e) { /* no-op */ }
            }
        } catch (e) { /* no-op */ }
        drawControl = null;

        // Optionally remove any lingering toolbar DOM nodes (defensive)
        try {
            const toolbars = document.querySelectorAll('.leaflet-draw-toolbar, .leaflet-draw-toolbar-top, .leaflet-draw-toolbar-bottom');
            toolbars.forEach(n => n.parentNode && n.parentNode.removeChild(n));
        } catch (e) { /* no-op */ }
    }

    // alias for clarity
    const stopDrawing = removeDrawControls;

    function getScratchAndClear(name, type, colorHex) {
        if (scratchFeatures.length === 0) return "";
        const color = (typeof colorHex === 'string' && colorHex.trim()) ? colorHex : theme.accent;

        scratchFeatures = scratchFeatures.map(g => {
            g.properties = g.properties || {};
            g.properties.name = name || "";
            g.properties.type = type || "";
            g.properties.color = color; // persist color per feature
            return g;
        });

        const fc = { type: "FeatureCollection", features: scratchFeatures };
        const json = JSON.stringify(fc);
        scratchFeatures = [];
        notifyScratchChanged();
        if (drawnItems) drawnItems.clearLayers();
        return json;
    }

    function fitToVisible() {
        if (!map) return;

        // Collect all non-empty feature groups
        const groups = Object.values(currentRevsion)
            .filter(g => g && typeof g.getLayers === "function" && g.getLayers().length > 0);

        if (groups.length === 0) return;

        const all = L.featureGroup(groups);
        const bounds = all.getBounds();
        if (bounds && bounds.isValid && bounds.isValid()) {
            map.fitBounds(bounds, {
                padding: [40, 40],
                maxZoom: 17,
                animate: true
            });
        }
    }

    function scratchCount() { return Array.isArray(scratchFeatures) ? scratchFeatures.length : 0; }

    function setScratchNotifier(ref) { dotnetRef = ref; }
    function notifyScratchChanged() { try { if (dotnetRef) dotnetRef.invokeMethodAsync("OnScratchChanged", scratchCount()); } catch (e) { console.warn(e); } }

    return {
        init,
        dispose,
        setScratchNotifier,
        addOrReplaceGeoJsonLayer,
        invalidateSize,
        startDrawing,
        getScratchAndClear,
        updateCurrentRevision,
        // exposed removal function(s)
        removeDrawControls,
        stopDrawing
    };
})();