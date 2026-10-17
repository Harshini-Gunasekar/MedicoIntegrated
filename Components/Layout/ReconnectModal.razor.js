// Set up event handlers
const reconnectModal = document.getElementById("components-reconnect-modal");
reconnectModal.addEventListener("components-reconnect-state-changed", handleReconnectStateChanged);

const retryButton = document.getElementById("components-reconnect-button");
retryButton.addEventListener("click", retry);

const resumeButton = document.getElementById("components-resume-button");
resumeButton.addEventListener("click", resume);

let reconnectShowTimeout = null;
let reconnectRetryTimeout = null;
let retryAttempt = 0;
const MAX_BACKOFF_MS = 15000;

function getBackoffDelay() {
    // Exponential backoff: 1s, 2s, 4s, 8s, up to 15s max
    const delay = Math.min(1000 * Math.pow(2, retryAttempt), MAX_BACKOFF_MS);
    return delay;
}

function scheduleSilentRetry() {
    clearTimeout(reconnectRetryTimeout);
    const delay = getBackoffDelay();
    reconnectRetryTimeout = setTimeout(async () => {
        retryAttempt++;
        await silentReconnect();
    }, delay);
}

async function silentReconnect() {
    try {
        const successful = await Blazor.reconnect();
        if (successful) {
            retryAttempt = 0;
            if (reconnectModal && reconnectModal.open) {
                try { reconnectModal.close(); } catch { }
            }
            return;
        }

        const resumeSuccessful = await Blazor.resumeCircuit();
        if (resumeSuccessful) {
            retryAttempt = 0;
            if (reconnectModal && reconnectModal.open) {
                try { reconnectModal.close(); } catch { }
            }
            return;
        }

        // Schedule next retry with backoff instead of aggressive reload
        scheduleSilentRetry();
    } catch (err) {
        // Server unreachable, retry with backoff
        scheduleSilentRetry();
    }
}

function handleReconnectStateChanged(event) {
    if (event.detail.state === "show") {
        clearTimeout(reconnectShowTimeout);
        // Delay showing reconnect modal so temporary network blips / tab switches don't flash the box
        reconnectShowTimeout = setTimeout(() => {
            if (reconnectModal && !reconnectModal.open) {
                try { reconnectModal.showModal(); } catch { }
            }
        }, 3000);
        // Start silent retry loop
        scheduleSilentRetry();
    } else if (event.detail.state === "hide") {
        clearTimeout(reconnectShowTimeout);
        clearTimeout(reconnectRetryTimeout);
        retryAttempt = 0;
        if (reconnectModal && reconnectModal.open) {
            try { reconnectModal.close(); } catch { }
        }
    } else if (event.detail.state === "failed") {
        clearTimeout(reconnectShowTimeout);
        if (reconnectModal && !reconnectModal.open) {
            try { reconnectModal.showModal(); } catch { }
        }
        document.addEventListener("visibilitychange", retryWhenDocumentBecomesVisible);
        scheduleSilentRetry();
    } else if (event.detail.state === "rejected") {
        // Do NOT aggressively call location.reload()! Try resuming circuit or reconnecting silently
        clearTimeout(reconnectShowTimeout);
        scheduleSilentRetry();
    }
}

async function retry() {
    document.removeEventListener("visibilitychange", retryWhenDocumentBecomesVisible);
    retryAttempt = 0;
    await silentReconnect();
}

async function resume() {
    try {
        const successful = await Blazor.resumeCircuit();
        if (successful) {
            if (reconnectModal && reconnectModal.open) {
                try { reconnectModal.close(); } catch { }
            }
        } else {
            await silentReconnect();
        }
    } catch {
        scheduleSilentRetry();
    }
}

async function retryWhenDocumentBecomesVisible() {
    if (document.visibilityState === "visible") {
        retryAttempt = 0;
        await silentReconnect();
    }
}
