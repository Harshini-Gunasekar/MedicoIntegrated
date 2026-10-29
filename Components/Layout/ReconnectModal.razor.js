// ReconnectModal.razor.js - Resilient Blazor Server Reconnection & Wake-from-Sleep Handler
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
let isCheckingHealth = false;
let lastHeartbeatTime = Date.now();
let tabHiddenTimestamp = 0;

const MAX_ATTEMPTS_BEFORE_RELOAD = 3;
const MAX_BACKOFF_MS = 5000;

function getBackoffDelay() {
    // 1s, 2s, 4s, up to 5s
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

    // Ping again in 2 seconds until online
    clearTimeout(reconnectRetryTimeout);
    reconnectRetryTimeout = setTimeout(checkServerAndReload, 2000);
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
                console.warn("[Medico Reconnect] Circuit rejected by server. Reloading page...");
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
        // Delay showing reconnect modal by 1.5 seconds so momentary blips don't flicker UI
        reconnectShowTimeout = setTimeout(() => {
            if (reconnectModal && !reconnectModal.open) {
                try { reconnectModal.showModal(); } catch { }
            }
        }, 1500);

        scheduleSilentRetry();
    } else if (state === "retrying") {
        if (reconnectModal && !reconnectModal.open) {
            try { reconnectModal.showModal(); } catch { }
        }
        scheduleSilentRetry();
    } else if (state === "hide") {
        clearTimeout(reconnectShowTimeout);
        clearTimeout(reconnectRetryTimeout);
        retryAttempt = 0;
        isReconnecting = false;
        if (reconnectModal && reconnectModal.open) {
            try { reconnectModal.close(); } catch { }
        }
    } else if (state === "failed") {
        clearTimeout(reconnectShowTimeout);
        if (reconnectModal && !reconnectModal.open) {
            try { reconnectModal.showModal(); } catch { }
        }
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

// Handler for when user switches back to tab or device wakes from sleep
async function handleTabOrWakeResume(elapsedMs) {
    if (isCheckingHealth) return;

    const isModalOpen = reconnectModal && reconnectModal.open;
    const isModalShownClass = reconnectModal && (
        reconnectModal.classList.contains("components-reconnect-show") ||
        reconnectModal.classList.contains("components-reconnect-retrying") ||
        reconnectModal.classList.contains("components-reconnect-failed") ||
        reconnectModal.classList.contains("components-reconnect-rejected")
    );

    // If away for less than 3s and modal is not active, connection is likely unaffected
    if (!isModalOpen && !isModalShownClass && elapsedMs < 3000) {
        return;
    }

    isCheckingHealth = true;

    try {
        // Step 1: Probe server availability with a fast HEAD request
        try {
            const probe = await fetch("/favicon.ico?probe=" + Date.now(), {
                method: "HEAD",
                cache: "no-store"
            });
            if (!probe.ok && probe.status >= 500) {
                scheduleSilentRetry();
                return;
            }
        } catch (probeErr) {
            // Network is still offline (e.g. WiFi reconnecting right after wake)
            window.addEventListener("online", () => handleTabOrWakeResume(elapsedMs), { once: true });
            return;
        }

        // Step 2: Server is alive. Verify and restore the Blazor circuit.
        if (window.Blazor && typeof window.Blazor.reconnect === "function") {
            const reconnected = await window.Blazor.reconnect();
            if (reconnected) {
                // Reconnected successfully to the existing circuit!
                retryAttempt = 0;
                clearTimeout(reconnectShowTimeout);
                clearTimeout(reconnectRetryTimeout);
                if (reconnectModal && reconnectModal.open) {
                    try { reconnectModal.close(); } catch { }
                }
                return;
            } else {
                // Circuit was evicted on the server (session expired during sleep).
                // Auto-reload to immediately give the user a fresh, responsive page!
                console.warn("[Medico Reconnect] Circuit evicted on server. Auto-reloading page...");
                window.location.reload();
                return;
            }
        }

        if (window.Blazor && typeof window.Blazor.resumeCircuit === "function") {
            const resumed = await window.Blazor.resumeCircuit();
            if (resumed) {
                retryAttempt = 0;
                clearTimeout(reconnectShowTimeout);
                clearTimeout(reconnectRetryTimeout);
                if (reconnectModal && reconnectModal.open) {
                    try { reconnectModal.close(); } catch { }
                }
                return;
            }
        }
    } catch (e) {
        console.warn("[Medico Reconnect] Error during resume check:", e);
        if (elapsedMs > 5000 || isModalOpen || isModalShownClass) {
            window.location.reload();
        }
    } finally {
        isCheckingHealth = false;
    }
}

// Listen for tab visibility changes
document.addEventListener("visibilitychange", async () => {
    if (document.visibilityState === "hidden") {
        tabHiddenTimestamp = Date.now();
    } else if (document.visibilityState === "visible") {
        const awayTime = tabHiddenTimestamp > 0 ? (Date.now() - tabHiddenTimestamp) : 0;
        tabHiddenTimestamp = 0;
        await handleTabOrWakeResume(awayTime);
    }
});

// Listen for window focus & pageshow
window.addEventListener("focus", async () => {
    if (document.visibilityState === "visible") {
        await handleTabOrWakeResume(0);
    }
});

window.addEventListener("pageshow", async () => {
    if (document.visibilityState === "visible") {
        await handleTabOrWakeResume(0);
    }
});

window.addEventListener("online", async () => {
    retryAttempt = 0;
    await silentReconnect();
});

// Heartbeat timer running every 2.5s.
// If the interval between ticks is > 6s, the PC woke up from sleep or tab was frozen!
setInterval(async () => {
    const now = Date.now();
    const gap = now - lastHeartbeatTime;
    lastHeartbeatTime = now;

    if (gap > 6000) {
        console.log(`[Medico Reconnect] Wake-from-sleep detected (gap: ${Math.round(gap / 1000)}s).`);
        await handleTabOrWakeResume(gap);
    }
}, 2500);

// Periodic lightweight keep-alive ping every 25s while active to prevent idle connection drop
setInterval(() => {
    if (document.visibilityState === "visible" && (!reconnectModal || !reconnectModal.open)) {
        fetch("/favicon.ico", { method: "HEAD", cache: "no-store" }).catch(() => {});
    }
}, 25000);
