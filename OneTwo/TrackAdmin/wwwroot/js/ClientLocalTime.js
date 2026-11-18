// wwwroot/js/ClientTime.js
window.captureClientTime = function () {
    const localtime = new Date();
    console.log("Client Local Time Captured: ", localtime.toString());
    return localtime.toISOString(); // Client's local time in UTC
};
