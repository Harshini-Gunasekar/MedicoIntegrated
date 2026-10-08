// Set up event handlers
const reconnectModal = document.getElementById("components-reconnect-modal");
reconnectModal.addEventListener("components-reconnect-state-changed", handleReconnectStateChanged);

const retryButton = document.getElementById("components-reconnect-button");
retryButton.addEventListener("click", retry);

const resumeButton = document.getElementById("components-resume-button");
resumeButton.addEventListener("click", resume);

let reconnectShowTimeout = null;

function handleReconnectStateChanged(event) {
    if (event.detail.state === "show") {
        clearTimeout(reconnectShowTimeout);
        // Delay showing reconnect modal by 2 seconds so normal page refreshes never flash the box
        reconnectShowTimeout = setTimeout(() => {
            if (reconnectModal && !reconnectModal.open) {
                try { reconnectModal.showModal(); } catch { }
            }
        }, 2000);
    } else if (event.detail.state === "hide") {
        clearTimeout(reconnectShowTimeout);
        if (reconnectModal && reconnectModal.open) {
            try { reconnectModal.close(); } catch { }
        }
    } else if (event.detail.state === "failed") {
        clearTimeout(reconnectShowTimeout);
        if (reconnectModal && !reconnectModal.open) {
            try { reconnectModal.showModal(); } catch { }
        }
        document.addEventListener("visibilitychange", retryWhenDocumentBecomesVisible);
    } else if (event.detail.state === "rejected") {
        clearTimeout(reconnectShowTimeout);
        location.reload();
    }
}

async function retry() {
    document.removeEventListener("visibilitychange", retryWhenDocumentBecomesVisible);

    try {
        // Reconnect will asynchronously return:
        // - true to mean success
        // - false to mean we reached the server, but it rejected the connection (e.g., unknown circuit ID)
        // - exception to mean we didn't reach the server (this can be sync or async)
        const successful = await Blazor.reconnect();
        if (!successful) {
            // We have been able to reach the server, but the circuit is no longer available.
            // We'll reload the page so the user can continue using the app as quickly as possible.
            const resumeSuccessful = await Blazor.resumeCircuit();
            if (!resumeSuccessful) {
                location.reload();
            } else {
                reconnectModal.close();
            }
        }
    } catch (err) {
        // We got an exception, server is currently unavailable
        document.addEventListener("visibilitychange", retryWhenDocumentBecomesVisible);
    }
}

async function resume() {
    try {
        const successful = await Blazor.resumeCircuit();
        if (!successful) {
            location.reload();
        }
    } catch {
        reconnectModal.classList.replace("components-reconnect-paused", "components-reconnect-resume-failed");
    }
}

async function retryWhenDocumentBecomesVisible() {
    if (document.visibilityState === "visible") {
        await retry();
    }
}
