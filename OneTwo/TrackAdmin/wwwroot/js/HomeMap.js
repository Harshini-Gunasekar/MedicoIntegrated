let map, markers = {};
let destinationMarker;
let dotNetRef = null;

// Replace this with your actual Mapbox Access Token
const MAPBOX_ACCESS_TOKEN = 'YOUR_ACTUAL_SECRET_TOKEN';

window.initMap = function () {
    navigator.geolocation.getCurrentPosition(
        (position) => {
            const latitude = position.coords.latitude;
            const longitude = position.coords.longitude;

            map = L.map('map', {
                scrollWheelZoom: true,
                zoomControl: true
            }).setView([latitude, longitude], 12);

            L.tileLayer(`https://api.mapbox.com/styles/v1/{id}/tiles/{z}/{x}/{y}?access_token=${MAPBOX_ACCESS_TOKEN}`, {
                attribution: '© <a href="https://www.mapbox.com/about/maps/">Mapbox</a> © <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>',
                maxZoom: 20,
                tileSize: 512,
                zoomOffset: -1,
                id: 'mapbox/streets-v11',
                accessToken: MAPBOX_ACCESS_TOKEN
            }).addTo(map);
        },
        (error) => {
            console.error('Geolocation failed:', error);
        }
    );
};

//window.updateMarkers = function (locations) {
//    if (!window.markers) {
//        window.markers = {};
//    }

//    const bikeIcon = L.icon({
//        iconUrl: '/images/bike.png',
//        iconSize: [32, 32],
//        iconAnchor: [16, 32],
//        popupAnchor: [0, -32]
//    });

//    const carIcon = L.icon({
//        iconUrl: '/images/car.png',
//        iconSize: [32, 32],
//        iconAnchor: [16, 32],
//        popupAnchor: [0, -32]
//    });

//    const activeRiderIds = new Set();

//    locations.forEach(loc => {
//        const riderId = loc.riderId;
//        const icon = loc.vehicleType === 'Bike' ? bikeIcon : carIcon;
//        const latLng = L.latLng(loc.latitude, loc.longitude);
//        activeRiderIds.add(riderId);

//        if (markers[riderId]) {
//            markers[riderId].setLatLng(latLng);
//        } else {
//            const marker = L.marker(latLng)
//                .addTo(map)
//                .bindTooltip(
//                    `<div style=display:inline-flex;color:Blue;padding:1px 10px 1px 4px;> <img style=width:30px;height:30px;padding:5px; src=${icon.options.iconUrl} /> <b> ${loc.riderName}</b> </div> `,
//                    { permanent: true, direction: 'top' }
//                )
//                .bindPopup(
//                    `<b>Rider: ${riderId}</b> - ${loc.riderName}<br>Lat: ${loc.latitude}<br>Lng: ${loc.longitude}`
//                );

//            markers[riderId] = marker;
//        }
//    });
//};

window.updateMarkers = function (locations) {
    if (!window.markers) window.markers = {};

    const bikeIcon = L.icon({
        iconUrl: '/images/bike.png',
        iconSize: [32, 32],
        iconAnchor: [16, 32],
        popupAnchor: [0, -32]
    });

    const carIcon = L.icon({
        iconUrl: '/images/car.png',
        iconSize: [32, 32],
        iconAnchor: [16, 32],
        popupAnchor: [0, -32]
    });

    locations.forEach(loc => {
        const riderId = loc.riderId;
        const icon = loc.vehicleType === "Bike" ? bikeIcon : carIcon;
        const newLatLng = L.latLng(loc.latitude, loc.longitude);

        if (markers[riderId]) {
            // Smooth animation to new position
            animateMarkerMove(markers[riderId], newLatLng, 1000);
        } else {
            // First-time marker creation
            markers[riderId] = L.marker(newLatLng, { icon: icon })
                .addTo(map)
                .bindTooltip(`<b>${loc.riderName}</b>`, { permanent: true })
                .bindPopup(`Rider: ${loc.riderName}`);
        }
    });
};
function animateMarkerMove(marker, newLatLng, duration) {
    const start = marker.getLatLng();
    const end = newLatLng;
    const startTime = performance.now();

    function animate() {
        const now = performance.now();
        const progress = Math.min((now - startTime) / duration, 1);

        const currentLat =
            start.lat + (end.lat - start.lat) * progress;
        const currentLng =
            start.lng + (end.lng - start.lng) * progress;

        marker.setLatLng([currentLat, currentLng]);

        if (progress < 1) {
            requestAnimationFrame(animate);
        }
    }

    requestAnimationFrame(animate);
}















//let map, markers = {};
//let destinationMarker;
//let dotNetRef = null;

//window.initMap = function () {
//    navigator.geolocation.getCurrentPosition(
//        (position) => {
//            const latitude = position.coords.latitude;
//            const longitude = position.coords.longitude;

//            map = L.map('map', {
//                scrollWheelZoom: true,
//                zoomControl: true
//            }).setView([latitude, longitude], 12);

//            L.tileLayer('https://maps.geoapify.com/v1/tile/osm-bright/{z}/{x}/{y}.png?apiKey=47bc95f2ad4f40f3b1d10615d39896c9', {
//                attribution: '&copy; <a href="https://www.geoapify.com/">Geoapify</a>, © OpenStreetMap contributors',
//                maxZoom: 20
//            }).addTo(map);
//        },
//        (error) => {
//            console.error('Geolocation failed:', error);
//        }
//    );
//};

//window.updateMarkers = function (locations) {
//    if (!window.markers) {
//        window.markers = {};
//    }

//    const bikeIcon = L.icon({
//        iconUrl: '/images/bike.png',
//        iconSize: [32, 32],
//        iconAnchor: [16, 32],
//        popupAnchor: [0, -32]
//    });

//    const carIcon = L.icon({
//        iconUrl: '/images/car.png',
//        iconSize: [32, 32],
//        iconAnchor: [16, 32],
//        popupAnchor: [0, -32]
//    });

//    const activeRiderIds = new Set();

//    locations.forEach(loc => {
//        const riderId = loc.riderId;
//        const icon = loc.vehicleType === 'Bike' ? bikeIcon : carIcon;
//        const latLng = L.latLng(loc.latitude, loc.longitude);
//        activeRiderIds.add(riderId);

//        if (markers[riderId]) {
//            markers[riderId].setLatLng(latLng);
//        } else {
//            const marker = L.marker(latLng, { icon: icon })
//                .addTo(map)
//                .bindTooltip(
//                    `Rider: ${loc.riderName}<br/><b><img src="${icon.options.iconUrl}" width="20" height="20" alt="${loc.vehicleType}" /></b><br>`,
//                    { permanent: true, direction: 'top' }
//                )
//                .bindPopup(
//                    `<b>Rider: ${riderId}</b><br>Lat: ${loc.latitude}<br>Lng: ${loc.longitude}`
//                );

//            markers[riderId] = marker;
//        }
//    });
//};





//                let routePolyline;
//                window.drawRiderRoute = function (latLngList) {
//            if (routePolyline) {
//                    map.removeLayer(routePolyline); // Remove previous line
//            }

//            const pathPoints = latLngList.map(p => [p.latitude, p.longitude]);

//                routePolyline = L.polyline(pathPoints, {
//                    color: 'blue',
//                weight: 4
//            }).addTo(map);

//                // Fit the map view to the route
//                map.fitBounds(routePolyline.getBounds());
//        };

          
//                window.drawRiderPath = function (latLngArray) {
//            if (!map) return;

//                let polyline = L.polyline(latLngArray, {color: 'blue' }).addTo(map);
//                map.fitBounds(polyline.getBounds());
//        };
          