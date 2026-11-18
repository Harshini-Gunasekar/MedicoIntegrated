window.getLocation = function () {
    return new Promise(function (resolve, reject) {
        if (!navigator.geolocation) {
            reject("Geolocation is not supported by your browser.");
        } else {
            navigator.geolocation.getCurrentPosition(
                function (position) {
                    resolve({
                        latitude: position.coords.latitude,
                        longitude: position.coords.longitude
                    });
                },
                function (error) {
                    reject(error.message);
                }
            );
        }
    });
};
