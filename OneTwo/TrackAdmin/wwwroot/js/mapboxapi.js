window.StartMap = function (riderCoordinates) {

    try {
        mapboxgl.accessToken = 'YOUR_ACTUAL_SECRET_TOKEN';

        const map = new mapboxgl.Map({
            container: 'mapstart',
            style: 'mapbox://styles/mapbox/streets-v11',
            center: [77.563081, 11.462878], // Initial center
            zoom: 10
        });


        const coordinates = riderCoordinates;

        map.on('load', () => {
            // Draw route (line)
            map.addSource('route', {
                'type': 'geojson',
                'data': {
                    'type': 'Feature',
                    'properties': {},
                    'geometry': {
                        'type': 'LineString',
                        'coordinates': riderCoordinates
                    }
                }
            });

            map.addLayer({
                'id': 'route',
                'type': 'line',
                'source': 'route',
                'layout': {
                    'line-join': 'round',
                    'line-cap': 'round'
                },
                'paint': {
                    'line-color': '#FF0000',
                    'line-width': 4
                }
            });

            // Start Marker
            new mapboxgl.Marker({ color: 'green' })
                .setLngLat(riderCoordinates[0])
                .setPopup(new mapboxgl.Popup().setText("Start"))
                .addTo(map);

            // End Marker
            new mapboxgl.Marker({ color: 'blue' })
                .setLngLat(riderCoordinates[riderCoordinates.length - 1])
                .setPopup(new mapboxgl.Popup().setText("End"))
                .addTo(map);

            // Zoom to fit all points
            const bounds = new mapboxgl.LngLatBounds();
            riderCoordinates.forEach(coord => bounds.extend(coord));
            map.fitBounds(bounds, { padding: 40 });
        });
    }
    catch (e) {
        console.error(e);
    }
};
