        let maps, markers = {};

    window.initMap = function () {
        maps = L.maps('maps').setView([0, 0], 2);
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png').addTo(maps);
    };

    window.updateMarkers = function (locations) {
        for (let id in markers) {
            maps.removeLayer(markers[id]);
        }
        markers = {};

        locations.forEach(loc => {
            const marker = L.marker([loc.latitude, loc.longitude]).addTo(maps).bindPopup(loc.riderId);
            markers[loc.riderId] = marker;
        });
    };
 