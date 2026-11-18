
var map;
var circle;
var marker;
var dotNetRef;


window.initReferralMap = function (dotnetHelper) {
    setTimeout(function () {
        dotNetRef = dotnetHelper; // 💥 Store the Blazor reference here!
        var mapContainer = document.getElementById('mapreferral');

        // Ensure the map container exists before initializing
        if (!mapContainer) {
            console.error("Map container not found. Retrying...");
            setTimeout(window.initializeLeafletMap, 500); // Retry after 500ms
            return;
        }

        // Prevent multiple initializations
        if (mapContainer._leaflet_id) {
            mapContainer._leaflet_id = null;
        }


        map = L.map('mapreferral').setView([0, 0], 2);
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png?lang=en').addTo(map);

        // 🛠️ FIX: Ensure map resizes correctly
        setTimeout(() => {
            map.invalidateSize();  // 🧠 FORCE CORRECT RENDERING
        }, 300); // Delay slightly after creation


        // Click to set destination
        map.on('click', function (e) {
            let lat = e.latlng.lat;
            let lng = e.latlng.lng;
            // Remove previous marker and circle
            if (marker) {
                map.removeLayer(marker);
            }
            if (circle) {
                map.removeLayer(circle);
            }
            // Add destination marker
            if (destinationMarker) {
                destinationMarker.setLatLng(e.latlng);
            } else {
                destinationMarker = L.marker(e.latlng).addTo(map).bindPopup("Customer Location").openPopup();
            }

            // Send to Blazor
            dotNetRef.invokeMethodAsync('ReferralLocation', lat, lng);

            marker = L.marker([lat, lng]).addTo(map);

            circle = L.circle([lat, lng], {
                color: 'blue',
                fillColor: '#30f',
                fillOpacity: 0.2,
                radius: 20
            }).addTo(map);
        });
    },500);
};
window.initialReferralMap = function (radius, defaultLat, defaultLng, name) {
    try
    { 
            if (marker) {
                map.removeLayer(marker);
            }
            if (circle) {
                map.removeLayer(circle);
            }

            map.setView([defaultLat, defaultLng], 15);
            marker = L.marker([defaultLat, defaultLng]).addTo(map)
                .bindTooltip(`Referral: ${name}`, { permanent: true, direction: 'top' })
                .bindPopup(`Lat: ${defaultLat}<br>Lng: ${defaultLng}`);;

            circle = L.circle([defaultLat, defaultLng], {
                color: 'blue',
                fillColor: '#30f',
                fillOpacity: 0.2,
                radius: radius
            }).addTo(map);
    }catch (e)
    {
        var msf = e;
    }
};

window.updateCircleRadius = function (radius) {
    if (circle) {
        circle.setRadius(radius);
    }
};
//window.initialReferralMapwithRiders = function (radius, defaultLat, defaultLng, name, riders) {

//    if (marker) {
//        map.removeLayer(marker);
//    }
//    if (circle) {
//        map.removeLayer(circle);
//    }

//    map.setView([defaultLat, defaultLng], 15);
//    marker = L.marker([defaultLat, defaultLng]).addTo(map)
//        .bindTooltip(`Referral: ${name}`, { permanent: true, direction: 'top' })
//        .bindPopup(`Lat: ${defaultLat}<br>Lng: ${defaultLng}`);;
    
//    circle = L.circle([defaultLat, defaultLng], {
//        color: 'blue',
//        fillColor: '#30f',
//        fillOpacity: 0.2,
//        radius: radius
//    }).addTo(map);
//    try {

//        const bounds = [];

//        // Add rider markers
//        riders.forEach(loc => {
//            const imagePath = loc.vehicleType === 'Bike' ? '/images/bike.png' : '/images/car.png';
//            const marker = L.marker([loc.latitude, loc.longitude])
//                .addTo(map).bindTooltip(`Rider: ${loc.riderName}<br/><b><img src="${imagePath}" width="20" height="20" alt="${loc.vehicleType}" /></b><br>`, { permanent: true, direction: 'top' })
//                .bindPopup(`<b>Rider: ${loc.riderId}</b><br>Lat: ${loc.latitude}<br>Lng: ${loc.longitude}`);
//            markers[loc.riderId] = marker;

//            bounds.push([loc.latitude, loc.longitude]);
//        });
//    } catch (e)
//    {
//        var tt = e;
//    }
//};



window.focusOnAddress = async function (mapId, address) {
    try {
        let response = await fetch(`https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(address)}&accept-language=en&addressdetails=1`);
        let data = await response.json();

        if (data.length > 0) {
            let lat = data[0].lat;
            let lon = data[0].lon;
            let englishName = data[0].display_name;
            map.setView([lat, lon], 15);
            
        } else {
            console.error("Address not found");
        }
    } catch (error) {
        console.error("Geocoding error:", error);
    }
};






window.initialReferralMapwithRiders = function (radius, defaultLat, defaultLng, name, riders, dotNetRef) {
    try {
        if (marker) map.removeLayer(marker);
        if (circle) map.removeLayer(circle);

        map.setView([defaultLat, defaultLng], 15);
        marker = L.marker([defaultLat, defaultLng]).addTo(map)
            .bindTooltip(`Referral: ${name}`, { permanent: true, direction: 'top' })
            .bindPopup(`Lat: ${defaultLat}<br>Lng: ${defaultLng}`);

        circle = L.circle([defaultLat, defaultLng], {
            color: 'blue',
            fillColor: '#30f',
            fillOpacity: 0.2,
            radius: radius
        }).addTo(map);

        const bounds = [];
        let routeResults = [];
        let completedCount = 0;

        const router = L.Routing.osrmv1();

        for (const loc of riders) {
            const imagePath = loc.vehicleType === 'Bike' ? '/images/bike.png' : '/images/car.png';
            
            // Skip invalid rider coordinates
            if (!loc.latitude || !loc.longitude || (loc.latitude === 0 && loc.longitude === 0)) {
                completedCount++;
                continue;
            }

            const riderMarker = L.marker([loc.latitude, loc.longitude])
                .addTo(map)
                .bindTooltip(`Rider: ${loc.riderName}<br/><b><img src="${imagePath}" width="20" height="20" alt="${loc.vehicleType}" /></b><br>`, { permanent: true, direction: 'top' })
                .bindPopup(`<b>Rider: ${loc.riderId}</b><br>Lat: ${loc.latitude}<br>Lng: ${loc.longitude}`);
            
            marker[loc.riderId] = riderMarker;
            bounds.push([loc.latitude, loc.longitude]);

            // Only route if destination is also valid
            if (defaultLat && defaultLng && (defaultLat !== 0 || defaultLng !== 0)) {
                router.route([
                    L.Routing.waypoint(L.latLng(loc.latitude, loc.longitude)),
                    L.Routing.waypoint(L.latLng(defaultLat, defaultLng))
                ], function (err, routes) {
                    completedCount++;

                    if (!err && routes && routes.length > 0) {
                        const route = routes[0];
                        const lineCoords = route.coordinates;

                        L.polyline(lineCoords, {
                            color: 'blue',
                            weight: 4,
                            opacity: 0.7
                        }).addTo(map);

                        const distanceInKm = (route.summary.totalDistance / 1000).toFixed(2);

                        routeResults.push({
                            riderId: loc.riderId,
                            riderName: loc.riderName,
                            distance: parseFloat(distanceInKm)
                        });
                    }

                    // Wait for all riders to be processed
                    if (completedCount === riders.length) {
                        if (dotNetRef) {
                            routeResults.sort((a, b) => a.distance - b.distance);
                            dotNetRef.invokeMethodAsync('ReceiveNearestRider', routeResults);
                        }
                    }
                });
            } else {
                completedCount++;
            }
        }
    } catch (e) {
        console.error("Error in initialReferralMapwithRiders:", e);
    }
};




window.refreshMapDataOnly = () => {
    // Clear previous markers
    markersLayer.clearLayers();

    // Example data: replace with dynamic fetch or Blazor call
    const markerData = [
        { lat: 51.505, lng: -0.09 },
        { lat: 51.515, lng: -0.1 }
    ];

    markerData.forEach(m => {
        L.marker([m.lat, m.lng]).addTo(markersLayer);
    });
};

/* Mappls Single Rider Live Tracking Map */
window.initializeMapplsSingleTrackingMap = function (riderLat, riderLng, clientLat, clientLng, riderName, clientName, vehicleType) {
    let retries = 0;
    function tryInit() {
        var container = document.getElementById('maptracking_single');
        if (!container) {
            if (retries < 25) {
                retries++;
                setTimeout(tryInit, 100);
            }
            return;
        }

        if (window.singleTrackingMapInstance) {
            window.singleTrackingMapInstance = null;
        }

        window.singleTrackingMapInstance = new mappls.Map("maptracking_single", {
            center: { lat: parseFloat(clientLat), lng: parseFloat(clientLng) },
            zoom: 13
        });

        window.singleTrackingMapInstance.on('load', function() {
            // 1. Client Destination Marker
            new mappls.Marker({
                map: window.singleTrackingMapInstance,
                position: { lat: parseFloat(clientLat), lng: parseFloat(clientLng) },
                icon: "/images/rider-pin-female.svg",
                width: 38,
                height: 48,
                popupHtml: `<b>Client Destination:</b><br/>${clientName}`
            });

            // 2. Rider Origin Marker
            var riderIcon = vehicleType === 'Bike' ? '/images/bike.png' : '/images/car.png';
            new mappls.Marker({
                map: window.singleTrackingMapInstance,
                position: { lat: parseFloat(riderLat), lng: parseFloat(riderLng) },
                icon: riderIcon,
                width: 32,
                height: 32,
                popupHtml: `<b>Rider Location:</b><br/>${riderName}`
            });

            // 3. Polyline Path Layer
            new mappls.Polyline({
                map: window.singleTrackingMapInstance,
                paths: [
                    { lat: parseFloat(riderLat), lng: parseFloat(riderLng) },
                    { lat: parseFloat(clientLat), lng: parseFloat(clientLng) }
                ],
                strokeColor: "#3b82f6",
                strokeOpacity: 0.8,
                strokeWeight: 6
            });
        });
    }
    tryInit();
};

/* Leaflet Single Rider Live Tracking Map — Premium Avatar Markers */
window.initializeLeafletSingleTrackingMap = function (riderLat, riderLng, clientLat, clientLng, riderName, clientName, vehicleType, riderAvatarUrl, riderStatus) {
    let retries = 0;
    function tryInit() {
        var container = document.getElementById('maptracking_single');
        if (!container) {
            if (retries < 25) { retries++; setTimeout(tryInit, 100); }
            return;
        }

        // Clean up previous Leaflet instance
        if (window.singleLeafletMapInstance) {
            try { window.singleLeafletMapInstance.remove(); } catch(e) {}
            window.singleLeafletMapInstance = null;
        }

        // Inject animated path keyframes once
        if (!document.getElementById('leaflet-path-anim-style')) {
            var style = document.createElement('style');
            style.id = 'leaflet-path-anim-style';
            style.textContent = `
                @keyframes marker-pulse-halo-anim {
                    0% { transform: scale(0.75); opacity: 0.9; }
                    100% { transform: scale(1.6); opacity: 0; }
                }
                .pulse-halo {
                    position: absolute; width: 44px; height: 44px; border-radius: 50%;
                    background: rgba(37, 99, 235, 0.4);
                    animation: marker-pulse-halo-anim 2s infinite ease-out; z-index: 1;
                }
                .pulse-halo.status-offline { background: rgba(148, 163, 184, 0.4); }
                .pulse-halo.status-intransit { background: rgba(245, 158, 11, 0.45); }
                .rider-avatar-marker-container { position: relative; width: 44px; height: 44px; display: flex; align-items: center; justify-content: center; }
                .rider-avatar-img { width: 36px; height: 36px; border-radius: 50%; object-fit: cover; border: 2.5px solid #fff; box-shadow: 0 4px 10px rgba(0,0,0,0.3); z-index: 2; background: #1e293b; position: relative; }
                .speed-badge { position: absolute; bottom: -6px; left: 50%; transform: translateX(-50%); background: #2563eb; color: #fff; font-size: 8px; font-weight: 900; padding: 1px 5px; border-radius: 6px; z-index: 3; white-space: nowrap; }
                .direction-chevron { position: absolute; top: -7px; left: 50%; transform: translateX(-50%); width: 0; height: 0; border-left: 5px solid transparent; border-right: 5px solid transparent; border-bottom: 7px solid #2563eb; z-index: 3; }
            `;
            document.head.appendChild(style);
        }

        try {
             // Choose premium smooth tile layer (CartoDB Positron/Dark Matter - No API key required)
             var isDark = document.documentElement.classList.contains('dark-theme');
             var tileUrl = isDark
                 ? 'https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png'
                 : 'https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png';
 
             window.singleLeafletMapInstance = L.map('maptracking_single', { zoomControl: true })
                 .setView([parseFloat(clientLat), parseFloat(clientLng)], 13);
 
             L.tileLayer(tileUrl, {
                 maxZoom: 20,
                 attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors &copy; <a href="https://carto.com/attributions">CARTO</a>'
             }).addTo(window.singleLeafletMapInstance);

            // --- Client Destination Marker ---
            var clientIconHtml = `
                <div style="position:relative;width:38px;height:38px;display:flex;align-items:center;justify-content:center;">
                    <div style="position:absolute;width:38px;height:38px;border-radius:50%;background:rgba(16,185,129,0.3);animation:marker-pulse-halo-anim 2.5s infinite ease-out;"></div>
                    <div style="width:30px;height:30px;border-radius:50%;background:#10b981;border:2.5px solid #fff;box-shadow:0 4px 10px rgba(0,0,0,0.3);display:flex;align-items:center;justify-content:center;position:relative;z-index:2;">
                        <svg width="16" height="16" viewBox="0 0 24 24" fill="white"><path d="M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7zm0 9.5c-1.38 0-2.5-1.12-2.5-2.5s1.12-2.5 2.5-2.5 2.5 1.12 2.5 2.5-1.12 2.5-2.5 2.5z"/></svg>
                    </div>
                </div>`;
            var clientIcon = L.divIcon({ html: clientIconHtml, className: '', iconSize: [38, 38], iconAnchor: [19, 19] });
            L.marker([parseFloat(clientLat), parseFloat(clientLng)], { icon: clientIcon })
                .addTo(window.singleLeafletMapInstance)
                .bindPopup(`<b>📍 ${clientName}</b><br/><small>Patient / Client Destination</small>`);

            // --- Rider Avatar Marker ---
            var avatarSrc = (riderAvatarUrl && riderAvatarUrl.trim() !== '') ? riderAvatarUrl : '/images/default-avatar.png';
            var statusClass = (riderStatus || '').toLowerCase().includes('transit') ? 'intransit'
                            : (riderStatus || '').toLowerCase().includes('active') ? 'active'
                            : 'offline';
            var riderMarkerHtml = `
                <div class="rider-avatar-marker-container">
                    <div class="pulse-halo status-${statusClass}"></div>
                    <img src="${avatarSrc}" class="rider-avatar-img" onerror="this.src='/images/default-avatar.png'" />
                    <div class="direction-chevron"></div>
                    <div class="speed-badge">Live</div>
                </div>`;
            var riderIcon = L.divIcon({ html: riderMarkerHtml, className: '', iconSize: [44, 44], iconAnchor: [22, 22] });
            L.marker([parseFloat(riderLat), parseFloat(riderLng)], { icon: riderIcon })
                .addTo(window.singleLeafletMapInstance)
                .bindPopup(`<b>🛺 ${riderName}</b><br/><small>Status: ${riderStatus || 'Online'}</small>`);

            // --- OSRM Route with animated flowing dashed line ---
            const router = L.Routing.osrmv1();
            router.route([
                L.Routing.waypoint(L.latLng(parseFloat(riderLat), parseFloat(riderLng))),
                L.Routing.waypoint(L.latLng(parseFloat(clientLat), parseFloat(clientLng)))
            ], function (err, routes) {
                if (!err && routes && routes.length > 0) {
                    var coords = routes[0].coordinates;

                    // Glow backing track
                    L.polyline(coords, { color: '#3b82f6', weight: 10, opacity: 0.12 })
                        .addTo(window.singleLeafletMapInstance);

                    // Solid base line
                    L.polyline(coords, { color: '#2563eb', weight: 4, opacity: 0.85 })
                        .addTo(window.singleLeafletMapInstance);

                    // Animated flowing dashes on top (SVG trick via className)
                    var animLine = L.polyline(coords, {
                        color: '#ffffff',
                        weight: 3,
                        opacity: 0.7,
                        dashArray: '12, 18',
                        className: 'animated-path-line'
                    }).addTo(window.singleLeafletMapInstance);

                    window.singleLeafletMapInstance.fitBounds(animLine.getBounds(), { padding: [55, 55] });

                    // Distance + Time
                    var dist = (routes[0].summary.totalDistance / 1000).toFixed(2);
                    var mins = Math.round(routes[0].summary.totalTime / 60);
                    var el1 = document.getElementById('route-distance-val');
                    var el2 = document.getElementById('route-time-val');
                    if (el1) el1.innerText = dist + ' KM';
                    if (el2) el2.innerText = mins + ' Min';
                } else {
                    // Fallback straight dashed line
                    var latlngs = [[parseFloat(riderLat), parseFloat(riderLng)], [parseFloat(clientLat), parseFloat(clientLng)]];
                    var fallback = L.polyline(latlngs, { color: '#2563eb', weight: 4, opacity: 0.75, dashArray: '12, 12' })
                        .addTo(window.singleLeafletMapInstance);
                    window.singleLeafletMapInstance.fitBounds(fallback.getBounds(), { padding: [55, 55] });
                }
            });

        } catch (err) {
            console.error('Leaflet Premium Tracking Map Error:', err);
        }
    }
    tryInit();
};

window.fitSingleTrackingMapBounds = function () {
    if (window.singleLeafletMapInstance) {
        var group = [];
        window.singleLeafletMapInstance.eachLayer(function(layer) {
            if (layer instanceof L.Marker) {
                group.push(layer.getLatLng());
            } else if (layer instanceof L.Polyline) {
                var latlngs = layer.getLatLngs();
                if (Array.isArray(latlngs)) {
                    latlngs.forEach(function(ll) {
                        if (Array.isArray(ll)) {
                            ll.forEach(function(innerLL) { group.push(innerLL); });
                        } else {
                            group.push(ll);
                        }
                    });
                }
            }
        });
        if (group.length > 0) {
            window.singleLeafletMapInstance.fitBounds(L.latLngBounds(group), { padding: [55, 55] });
        }
    }
};

/* ══ DRAGGABLE CHAT HUB ══ */
window.initializeDraggableHub = function () {
    const hub = document.getElementById('globalCommHub');
    if (!hub) return;

    // Restore saved position
    const saved = localStorage.getItem('chatHubPos');
    if (saved) {
        try {
            const pos = JSON.parse(saved);
            hub.style.right = 'auto';
            hub.style.bottom = 'auto';
            hub.style.left = pos.left + 'px';
            hub.style.top = pos.top + 'px';
        } catch (e) {}
    }

    let isDragging = false;
    let hasMoved = false;
    let startX, startY, startLeft, startTop;

    const onStart = (cx, cy) => {
        isDragging = true;
        hasMoved = false;
        const rect = hub.getBoundingClientRect();
        startX = cx;
        startY = cy;
        startLeft = rect.left;
        startTop = rect.top;
        hub.classList.add('dragging');
        hub.style.right = 'auto';
        hub.style.bottom = 'auto';
        hub.style.left = rect.left + 'px';
        hub.style.top = rect.top + 'px';
        hub.style.transition = 'none';
    };

    const onMove = (cx, cy) => {
        if (!isDragging) return;
        const dx = cx - startX;
        const dy = cy - startY;
        if (Math.abs(dx) > 3 || Math.abs(dy) > 3) hasMoved = true;
        let newLeft = startLeft + dx;
        let newTop = startTop + dy;
        // Keep within viewport bounds
        newLeft = Math.max(10, Math.min(window.innerWidth - hub.offsetWidth - 10, newLeft));
        newTop = Math.max(10, Math.min(window.innerHeight - hub.offsetHeight - 10, newTop));
        hub.style.left = newLeft + 'px';
        hub.style.top = newTop + 'px';
    };

    const onEnd = () => {
        if (!isDragging) return;
        isDragging = false;
        hub.classList.remove('dragging');
        hub.style.transition = '';
        // Save position
        localStorage.setItem('chatHubPos', JSON.stringify({
            left: parseInt(hub.style.left),
            top: parseInt(hub.style.top)
        }));
    };

    // Mouse events
    hub.addEventListener('mousedown', e => onStart(e.clientX, e.clientY));
    document.addEventListener('mousemove', e => onMove(e.clientX, e.clientY));
    document.addEventListener('mouseup', onEnd);

    // Touch events
    hub.addEventListener('touchstart', e => {
        const t = e.touches[0];
        onStart(t.clientX, t.clientY);
    }, { passive: true });
    document.addEventListener('touchmove', e => {
        if (!isDragging) return;
        e.preventDefault();
        const t = e.touches[0];
        onMove(t.clientX, t.clientY);
    }, { passive: false });
    document.addEventListener('touchend', onEnd);
};

