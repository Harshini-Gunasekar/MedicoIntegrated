// ReconnectModal.razor.js - Resilient Blazor Server Reconnection Handler
const reconnectModal = document.getElementById("components-reconnect-modal");
if (reconnectModal) {
    reconnectModal.addEventListener("components-reconnect-state-changed", handleReconnectStateChanged);
}

const retryButton = document.getElementById("components-reconnect-button");
if (retryButton) {
    retryButton.addEventListener("click", retry);
}

const reloadButton = document.getElementById("components-reload-button");
if (reloadButton) {
    reloadButton.addEventListener("click", () => {
        window.location.reload();
    });
}

const resumeButton = document.getElementById("components-resume-button");
if (resumeButton) {
    resumeButton.addEventListener("click", resume);
}

let reconnectShowTimeout = null;
let reconnectRetryTimeout = null;
let retryAttempt = 0;
let isReconnecting = false;
const MAX_ATTEMPTS_BEFORE_RELOAD = 3;
const MAX_BACKOFF_MS = 6000;

function getBackoffDelay() {
    // 1s, 2s, 4s, up to 6s
    return Math.min(1000 * Math.pow(2, retryAttempt), MAX_BACKOFF_MS);
}

function scheduleSilentRetry() {
    clearTimeout(reconnectRetryTimeout);

    // If retry attempts exceed threshold, test server availability and reload automatically
    if (retryAttempt >= MAX_ATTEMPTS_BEFORE_RELOAD) {
        checkServerAndReload();
        return;
    }

    const delay = getBackoffDelay();
    reconnectRetryTimeout = setTimeout(async () => {
        retryAttempt++;
        await silentReconnect();
    }, delay);
}

async function checkServerAndReload() {
    try {
        const res = await fetch(window.location.pathname, { method: "HEAD", cache: "no-store" });
        if (res.ok || res.status < 500) {
            window.location.reload();
            return;
        }
    } catch (e) {
        // Network or server still restarting
    }

    // Ping again in 3 seconds until online
    clearTimeout(reconnectRetryTimeout);
    reconnectRetryTimeout = setTimeout(checkServerAndReload, 3000);
}

async function silentReconnect() {
    if (isReconnecting) return;
    isReconnecting = true;

    try {
        if (window.Blazor && typeof window.Blazor.reconnect === "function") {
            const successful = await window.Blazor.reconnect();
            if (successful) {
                retryAttempt = 0;
                clearTimeout(reconnectShowTimeout);
                clearTimeout(reconnectRetryTimeout);
                if (reconnectModal && reconnectModal.open) {
                    try { reconnectModal.close(); } catch { }
                }
                isReconnecting = false;
                return;
            } else {
                // Server explicitly refused/rejected this circuit (it is no longer retained in server memory)
                // Reconnecting to this circuit will NEVER succeed. We must reload the page.
                window.location.reload();
                return;
            }
        }

        if (window.Blazor && typeof window.Blazor.resumeCircuit === "function") {
            const resumeSuccessful = await window.Blazor.resumeCircuit();
            if (resumeSuccessful) {
                retryAttempt = 0;
                clearTimeout(reconnectShowTimeout);
                clearTimeout(reconnectRetryTimeout);
                if (reconnectModal && reconnectModal.open) {
                    try { reconnectModal.close(); } catch { }
                }
                isReconnecting = false;
                return;
            }
        }

        scheduleSilentRetry();
    } catch (err) {
        scheduleSilentRetry();
    } finally {
        isReconnecting = false;
    }
}

function handleReconnectStateChanged(event) {
    const state = event.detail.state;

    if (state === "show") {
        clearTimeout(reconnectShowTimeout);
        // Delay showing reconnect modal by 2 seconds so momentary network blips or tab switches don't flash UI
        reconnectShowTimeout = setTimeout(() => {
            if (reconnectModal && !reconnectModal.open) {
                try { reconnectModal.showModal(); } catch { }
            }
        }, 2000);

        scheduleSilentRetry();
    } else if (state === "hide") {
        clearTimeout(reconnectShowTimeout);
        clearTimeout(reconnectRetryTimeout);
        retryAttempt = 0;
        if (reconnectModal && reconnectModal.open) {
            try { reconnectModal.close(); } catch { }
        }
    } else if (state === "failed") {
        clearTimeout(reconnectShowTimeout);
        if (reconnectModal && !reconnectModal.open) {
            try { reconnectModal.showModal(); } catch { }
        }
        document.addEventListener("visibilitychange", retryWhenDocumentBecomesVisible);
        window.addEventListener("online", handleOnline);

        scheduleSilentRetry();
    } else if (state === "rejected") {
        // Circuit was permanently rejected/evicted by server.
        // Immediately reload page so the user never gets stuck.
        clearTimeout(reconnectShowTimeout);
        clearTimeout(reconnectRetryTimeout);
        window.location.reload();
    }
}

async function retry() {
    document.removeEventListener("visibilitychange", retryWhenDocumentBecomesVisible);
    
    if (retryButton) {
        retryButton.disabled = true;
        retryButton.innerText = "Reconnecting...";
    }

    retryAttempt = 0;
    try {
        if (window.Blazor && typeof window.Blazor.reconnect === "function") {
            const successful = await window.Blazor.reconnect();
            if (successful) {
                if (reconnectModal && reconnectModal.open) {
                    try { reconnectModal.close(); } catch { }
                }
                if (retryButton) {
                    retryButton.disabled = false;
                    retryButton.innerText = "Retry";
                }
                return;
            }
        }
    } catch (e) { }

    // If reconnect did not succeed, reload page directly!
    window.location.reload();
}

async function resume() {
    try {
        if (window.Blazor && typeof window.Blazor.resumeCircuit === "function") {
            const successful = await window.Blazor.resumeCircuit();
            if (successful) {
                if (reconnectModal && reconnectModal.open) {
                    try { reconnectModal.close(); } catch { }
                }
                return;
            }
        }
    } catch { }

    await retry();
}

async function retryWhenDocumentBecomesVisible() {
    if (document.visibilityState === "visible") {
        retryAttempt = 0;
        await silentReconnect();
    }
}

function handleOnline() {
    retryAttempt = 0;
    silentReconnect();
}

// When user returns to tab after a long time, check if modal is open or connection is dead
document.addEventListener("visibilitychange", () => {
    if (document.visibilityState === "visible") {
        if (reconnectModal && reconnectModal.open) {
            retry();
        }
    }
});

// Periodic lightweight keep-alive ping every 45s while active to prevent idle connection termination
setInterval(() => {
    if (document.visibilityState === "visible" && (!reconnectModal || !reconnectModal.open)) {
        fetch("/favicon.ico", { method: "HEAD", cache: "no-store" }).catch(() => {});
    }
}, 45000);
