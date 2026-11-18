window.initGoogleMap = function (routePoints) {
    if (!routePoints || routePoints.length < 2) {
        console.error("At least two points are required.");
        return;
    }

    const directionsService = new google.maps.DirectionsService();
    const directionsRenderer = new google.maps.DirectionsRenderer({
        suppressMarkers: true,
        preserveViewport: true,
        polylineOptions: {
            strokeColor: "#FF0000",
            strokeWeight: 4,
        },
    });

    const map = new google.maps.Map(document.getElementById("mapgoogle"), {
        zoom: 7,
        center: { lat: routePoints[0].lat, lng: routePoints[0].lng },
    });

    directionsRenderer.setMap(map);

    const origin = routePoints[0];
    const destination = routePoints[routePoints.length - 1];

    // Directions API max waypoints: 23
    const waypoints = routePoints.slice(1, routePoints.length - 1).map(p => ({
        location: { lat: p.lat, lng: p.lng },
        stopover: false
    }));

    directionsService.route({
        origin: origin,
        destination: destination,
        waypoints: waypoints,
        travelMode: google.maps.TravelMode.DRIVING,
        optimizeWaypoints: false
    },
        (response, status) => {
            if (status === 'OK') {
                directionsRenderer.setDirections(response);

                // Optional start/end markers
                new google.maps.Marker({
                    position: origin,
                    map,
                    label: 'Start'
                });

                new google.maps.Marker({
                    position: destination,
                    map,
                    label: 'End'
                });
            } else {
                console.error('Directions request failed due to ' + status);
            }
        });
}