window.mapInstance = null;
window.riderOverlays = {};
window.userInteracted = false;
window.currentPopupRiderId = null;
window.popupYOffset = 120;
window.tenantId = "";
window.dotNetRef = null;

const riderStatusColors = {
    available: "#10b981",
    active: "#10b981",
    online: "#10b981",
    busy: "#f59e0b",
    engaged: "#f59e0b",
    "on hold": "#f59e0b",
    "on-hold": "#f59e0b",
    permission: "#94a3b8",
    offline: "#dc2626",
    idle: "#f59e0b",
    default: "#3b82f6"
};

window.initMappel = function (tId, objRef) {
    if (tId) window.tenantId = tId;
    if (objRef) window.dotNetRef = objRef;
    try {
        if (!window.mappls) {
            console.error("Mappls SDK missing");
            return;
        }

        // Inject style sheet for map marker avatar halos
        if (!document.getElementById('mappls-avatar-marker-style')) {
            var style = document.createElement('style');
            style.id = 'mappls-avatar-marker-style';
            style.textContent = `
                @keyframes marker-pulse-halo-anim {
                    0% { transform: scale(0.75); opacity: 0.9; }
                    100% { transform: scale(1.6); opacity: 0; }
                }
                .pulse-halo {
                    position: absolute; width: 44px; height: 44px; border-radius: 50%;
                    background: rgba(37, 99, 235, 0.4);
                    animation: marker-pulse-halo-anim 2s infinite ease-out; z-index: 1;
                }
                .pulse-halo.status-offline { background: rgba(220, 38, 38, 0.45); }
                .pulse-halo.status-active { background: rgba(34, 197, 94, 0.45); }
                .pulse-halo.status-onhold { background: rgba(245, 158, 11, 0.45); }
                .pulse-halo.status-permission { background: rgba(148, 163, 184, 0.45); }
                .rider-avatar-marker-container { position: relative; width: 44px; height: 44px; display: flex; align-items: center; justify-content: center; }
                .rider-avatar-img { width: 36px; height: 36px; border-radius: 50%; object-fit: cover; border: 2.5px solid #fff; box-shadow: 0 4px 10px rgba(0,0,0,0.3); z-index: 2; background: #1e293b; position: relative; }
                .speed-badge { position: absolute; bottom: -6px; left: 50%; transform: translateX(-50%); background: #2563eb; color: #fff; font-size: 8px; font-weight: 900; padding: 1px 5px; border-radius: 6px; z-index: 3; white-space: nowrap; }
                .direction-chevron { position: absolute; top: -7px; left: 50%; transform: translateX(-50%); width: 0; height: 0; border-left: 5px solid transparent; border-right: 5px solid transparent; border-bottom: 7px solid #2563eb; z-index: 3; }
            `;
            document.head.appendChild(style);
        }

        if (!window.mapInstance) {
            window.mapInstance = new mappls.Map("mappel", {
                center: { lat: 11.0168, lng: 76.9558 },
                zoom: 11
            });

            console.log("Map created");

            if (typeof window.mapInstance.on === "function") {
                window.mapInstance.on("dragstart", () => {
                    window.userInteracted = true;
                    repositionAllRiders();
                    updatePopupPosition();
                });

                window.mapInstance.on("zoomstart", () => {
                    window.userInteracted = true;
                    repositionAllRiders();
                    updatePopupPosition();
                });

                window.mapInstance.on("move", () => {
                    repositionAllRiders();
                    updatePopupPosition();
                });

                window.mapInstance.on("zoom", () => {
                    repositionAllRiders();
                    updatePopupPosition();
                });

                window.mapInstance.on("click", () => {
                    closeCustomPopup();
                });
            }
        }

        ensureOverlayLayer();
        ensureCustomPopupElement();
    } catch (e) {
        console.error("initMap error:", e);
    }
};

function safeNumber(val) {
    if (val === null || val === undefined) return NaN;
    return parseFloat(String(val).replace(",", "."));
}

function getStatusColor(status) {
    const key = String(status || "").trim().toLowerCase();
    return riderStatusColors[key] || riderStatusColors.default;
}

function getRiderPinIcon(rider) {
    return "/images/default-avatar.png";
}

function getShortName(riderData) {
    return (
        riderData.shortName ??
        riderData.ShortName ??
        riderData.riderName ??
        riderData.RiderName ??
        "Rider"
    );
}

function ensureOverlayLayer() {
    const mapContainer = document.getElementById("mappel");
    if (!mapContainer) return null;

    const style = window.getComputedStyle(mapContainer);
    if (style.position === "static") {
        mapContainer.style.position = "relative";
    }

    let layer = document.getElementById("rider-overlay-layer");
    if (!layer) {
        layer = document.createElement("div");
        layer.id = "rider-overlay-layer";
        layer.style.position = "absolute";
        layer.style.inset = "0";
        layer.style.pointerEvents = "none";
        layer.style.zIndex = "500";
        mapContainer.appendChild(layer);
    }

    return layer;
}

function createRiderElement(riderData, riderId) {
    const wrapper = document.createElement("div");
    wrapper.className = "rider-overlay-marker";
    wrapper.dataset.riderId = riderId;
    wrapper.style.position = "absolute";
    wrapper.style.width = "72px";
    wrapper.style.height = "76px";
    wrapper.style.transform = "translate(-50%, -100%)";
    wrapper.style.pointerEvents = "auto";
    wrapper.style.cursor = "pointer";
    wrapper.style.display = "flex";
    wrapper.style.flexDirection = "column";
    wrapper.style.alignItems = "center";
    wrapper.style.justifyContent = "flex-start";

    const label = document.createElement("div");
    label.className = "rider-shortname-label";
    label.innerText = getShortName(riderData);
    label.style.background = "rgba(15, 23, 42, 0.95)";
    label.style.color = "#ffffff";
    label.style.fontSize = "10.5px";
    label.style.fontWeight = "800";
    label.style.lineHeight = "1";
    label.style.padding = "4px 8px";
    label.style.borderRadius = "10px";
    label.style.boxShadow = "0 4px 12px rgba(0,0,0,0.15)";
    label.style.marginBottom = "4px";
    label.style.whiteSpace = "nowrap";
    label.style.maxWidth = "80px";
    label.style.overflow = "hidden";
    label.style.textOverflow = "ellipsis";
    label.style.textAlign = "center";
    label.style.border = "1px solid rgba(255,255,255,0.08)";

    const avatarContainer = document.createElement("div");
    avatarContainer.className = "rider-avatar-marker-container";
    avatarContainer.style.position = "relative";
    avatarContainer.style.width = "44px";
    avatarContainer.style.height = "44px";
    avatarContainer.style.display = "flex";
    avatarContainer.style.alignItems = "center";
    avatarContainer.style.justifyContent = "center";

    const status = (riderData.status ?? riderData.Status ?? "active").toLowerCase();
    const statusClass = status.includes("permission") ? "permission"
                      : (status.includes("offline") || status.includes("loggedout")) ? "offline"
                      : (status.includes("hold") || status.includes("busy") || status.includes("engaged") || status.includes("transit")) ? "onhold"
                      : "active";

    const halo = document.createElement("div");
    halo.className = `pulse-halo status-${statusClass}`;
    halo.style.position = "absolute";
    halo.style.width = "44px";
    halo.style.height = "44px";
    halo.style.borderRadius = "50%";

    const profilePic = riderData.profilePicture ?? riderData.ProfilePicture ?? "/images/default-avatar.png";
    const img = document.createElement("img");
    img.src = profilePic;
    img.className = "rider-avatar-img";
    img.style.width = "36px";
    img.style.height = "36px";
    img.style.borderRadius = "50%";
    img.style.objectFit = "cover";
    img.style.border = "2.5px solid #ffffff";
    img.style.boxShadow = "0 4px 10px rgba(0,0,0,0.3)";
    img.style.zIndex = "2";
    img.style.position = "relative";
    img.style.background = "#1e293b";
    img.onerror = function() {
        img.src = "/images/default-avatar.png";
    };

    const chevron = document.createElement("div");
    chevron.className = "direction-chevron";
    chevron.style.position = "absolute";
    chevron.style.top = "-7px";
    chevron.style.left = "50%";
    chevron.style.transform = "translateX(-50%)";
    chevron.style.width = "0";
    chevron.style.height = "0";
    chevron.style.borderLeft = "5px solid transparent";
    chevron.style.borderRight = "5px solid transparent";
    const statusColor = statusClass === "active" ? "#10b981" 
                      : statusClass === "onhold" ? "#f59e0b" 
                      : statusClass === "permission" ? "#94a3b8" 
                      : "#dc2626";
    chevron.style.borderBottom = `7px solid ${statusColor}`;
    chevron.style.zIndex = "3";

    const badge = document.createElement("div");
    badge.className = "speed-badge";
    badge.innerText = statusClass === "active" ? "Online" 
                    : statusClass === "onhold" ? "On Hold" 
                    : statusClass === "permission" ? "Permission" 
                    : "Offline";
    badge.style.position = "absolute";
    badge.style.bottom = "-6px";
    badge.style.left = "50%";
    badge.style.transform = "translateX(-50%)";
    badge.style.background = statusColor;
    badge.style.color = "#ffffff";
    badge.style.fontSize = "8px";
    badge.style.fontWeight = "900";
    badge.style.padding = "1px 5px";
    badge.style.borderRadius = "6px";
    badge.style.zIndex = "3";
    badge.style.whiteSpace = "nowrap";

    avatarContainer.appendChild(halo);
    avatarContainer.appendChild(img);
    avatarContainer.appendChild(chevron);
    avatarContainer.appendChild(badge);

    wrapper.appendChild(label);
    wrapper.appendChild(avatarContainer);

    wrapper.addEventListener("click", function (e) {
        e.preventDefault();
        e.stopPropagation();
        console.log("Marker clicked:", riderData);
        showCustomPopup(riderData, riderId);
    });

    return { wrapper, img, dot: badge, label, data: riderData };
}

function updateRiderElement(overlayObj, riderData) {
    if (!overlayObj) return;

    const profilePic = riderData.profilePicture ?? riderData.ProfilePicture ?? "/images/default-avatar.png";
    overlayObj.img.src = profilePic;

    const status = (riderData.status ?? riderData.Status ?? "active").toLowerCase();
    const statusClass = status.includes("permission") ? "permission"
                      : (status.includes("offline") || status.includes("loggedout")) ? "offline"
                      : (status.includes("hold") || status.includes("busy") || status.includes("engaged") || status.includes("transit")) ? "onhold"
                      : "active";

    const halo = overlayObj.wrapper.querySelector('.pulse-halo');
    if (halo) {
        halo.className = `pulse-halo status-${statusClass}`;
    }

    const badge = overlayObj.wrapper.querySelector('.speed-badge');
    if (badge) {
        badge.innerText = statusClass === "active" ? "Online" 
                        : statusClass === "onhold" ? "On Hold" 
                        : statusClass === "permission" ? "Permission" 
                        : "Offline";
        const statusColor = statusClass === "active" ? "#10b981" 
                          : statusClass === "onhold" ? "#f59e0b" 
                          : statusClass === "permission" ? "#94a3b8" 
                          : "#dc2626";
        badge.style.background = statusColor;
    }

    const chevron = overlayObj.wrapper.querySelector('.direction-chevron');
    if (chevron) {
        const statusColor = statusClass === "active" ? "#10b981" 
                          : statusClass === "onhold" ? "#f59e0b" 
                          : statusClass === "permission" ? "#94a3b8" 
                          : "#dc2626";
        chevron.style.borderBottom = `7px solid ${statusColor}`;
    }

    if (overlayObj.label) {
        overlayObj.label.innerText = getShortName(riderData);
    }

    overlayObj.data = riderData;
}

function placeOverlayElement(overlayObj) {
    if (!overlayObj || !overlayObj.data || !window.mapInstance) return;

    const lat = safeNumber(overlayObj.data.latitude ?? overlayObj.data.Latitude);
    const lng = safeNumber(overlayObj.data.longitude ?? overlayObj.data.Longitude);

    if (isNaN(lat) || isNaN(lng)) {
        overlayObj.wrapper.style.display = "none";
        return;
    }

    try {
        if (typeof window.mapInstance.project === "function") {
            const point = window.mapInstance.project([lng, lat]);
            if (point) {
                overlayObj.wrapper.style.left = `${point.x}px`;
                overlayObj.wrapper.style.top = `${point.y}px`;
                overlayObj.wrapper.style.display = "flex";
                return;
            }
        }
    } catch (err) {
        console.warn("placeOverlayElement error:", err);
    }

    overlayObj.wrapper.style.display = "none";
}

function repositionAllRiders() {
    Object.keys(window.riderOverlays).forEach(id => {
        placeOverlayElement(window.riderOverlays[id]);
    });
}

function ensureCustomPopupElement() {
    let popup = document.getElementById("custom-rider-popup");

    if (!popup) {
        popup = document.createElement("div");
        popup.id = "custom-rider-popup";

        popup.style.position = "absolute";
        popup.style.zIndex = "999999";
        popup.style.display = "none";
        popup.style.visibility = "hidden";
        popup.style.opacity = "0";
        popup.style.pointerEvents = "auto";

        popup.style.width = "fit-content";
        popup.style.maxWidth = "280px";
        popup.style.height = "auto";
        popup.style.padding = "0";
        popup.style.margin = "0";
        popup.style.background = "transparent";
        popup.style.transform = "translate(-50%, 0)";
        popup.style.fontFamily = "Arial, sans-serif";
        popup.style.whiteSpace = "normal";

        popup.innerHTML = `
            <div id="custom-rider-popup-inner" style="
                display:inline-block;
                width:auto;
                max-width:250px;
                max-height:220px;
                background:#111827;
                color:#ffffff;
                border-radius:14px;
                padding:14px 16px;
                box-shadow:0 14px 30px rgba(0,0,0,0.28);
                border:1px solid rgba(255,255,255,0.08);
                white-space:normal;
            "></div>
        `;

        popup.addEventListener("click", function (e) {
            e.stopPropagation();
        });

        const mapContainer = document.getElementById("mappel");
        if (mapContainer) {
            const style = window.getComputedStyle(mapContainer);
            if (style.position === "static") {
                mapContainer.style.position = "relative";
            }
            mapContainer.appendChild(popup);
        }
    }

    return popup;
}
function buildPopupHtml(riderData) {
    const riderName = riderData.fullName ?? riderData.FullName ?? getShortName(riderData);
    const riderId = riderData.riderId ?? riderData.RiderId ?? "";
    const riderStatus = riderData.status ?? riderData.Status ?? "Active";
    const vehicleType = riderData.vehicleType ?? riderData.VehicleType ?? "Bike";
    const lat = riderData.latitude ?? riderData.Latitude ?? "";
    const lng = riderData.longitude ?? riderData.Longitude ?? "";
    const color = getStatusColor(riderStatus);
    const profilePic = riderData.profilePicture ?? riderData.ProfilePicture ?? "/images/default-avatar.png";
    const isBase64 = profilePic.startsWith('data:image');
    const finalSrc = isBase64 ? profilePic : "/images/default-avatar.png";

    return `
        <div style="display:flex;align-items:center;gap:12px;margin-bottom:12px;">
            <div style="position:relative;flex:none;">
                <img class="rider-profile-img" src="${finalSrc}" data-rider-id="${riderId}" style="width:52px;height:52px;border-radius:50%;object-fit:cover;border:2px solid #3b82f6;background:#1f2937;" />
                <div style="position:absolute;bottom:2px;right:2px;width:12px;height:12px;border-radius:50%;background:${color};border:2px solid #111827;box-shadow:0 0 0 1px rgba(255,255,255,0.1);"></div>
            </div>
            <div style="min-width:0;">
                <div style="font-size:16px;font-weight:700;color:#ffffff;white-space:nowrap;overflow:hidden;text-overflow:ellipsis;">${riderName}</div>
                <div style="font-size:11px;color:#9ca3af;">Rider ID: ${riderId}</div>
            </div>
        </div>
        <div style="font-size:12px;line-height:1.7;color:#e5e7eb;background:rgba(255,255,255,0.05);padding:10px 12px;border-radius:12px;border:1px solid rgba(255,255,255,0.05);">
            <div style="display:flex;justify-content:space-between;margin-bottom:4px;">
                <span style="color:#9ca3af;">Status</span>
                <span style="font-weight:600;color:${color}">${riderStatus}</span>
            </div>
            <div style="display:flex;justify-content:space-between;margin-bottom:4px;">
                <span style="color:#9ca3af;">Vehicle</span>
                <span style="font-weight:600;">${vehicleType}</span>
            </div>
            <div style="margin-top:12px;display:flex;flex-direction:column;gap:8px;">
                <button onclick="window.openRiderChat('${riderId}')" style="
                    width:100%;
                    padding:8px 10px;
                    border-radius:10px;
                    border:none;
                    background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
                    color:white;
                    font-size:12px;
                    font-weight:700;
                    cursor:pointer;
                    display:flex;
                    align-items:center;
                    justify-content:center;
                    gap:8px;
                    box-shadow: 0 4px 12px rgba(37, 99, 235, 0.3);
                    transition: all 0.2s ease;
                ">
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"></path></svg>
                    Start Chat
                </button>
                <button onclick="window.triggerRoutePlayback('${riderId}', '${riderName}')" style="
                    width:100%;
                    padding:8px 10px;
                    border-radius:10px;
                    border:none;
                    background: linear-gradient(135deg, #0ea5e9 0%, #0284c7 100%);
                    color:white;
                    font-size:12px;
                    font-weight:700;
                    cursor:pointer;
                    display:flex;
                    align-items:center;
                    justify-content:center;
                    gap:8px;
                    box-shadow: 0 4px 12px rgba(14, 165, 233, 0.3);
                    transition: all 0.2s ease;
                ">
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polygon points="5 3 19 12 5 21 5 3"></polygon></svg>
                    Route Playback
                </button>
            </div>
        </div>
    `;
}

function showCustomPopup(riderData, riderId) {
    const popup = ensureCustomPopupElement();
    const inner = document.getElementById("custom-rider-popup-inner");

    if (!popup || !inner) return;

    inner.innerHTML = buildPopupHtml(riderData);

    const img = inner.querySelector('.rider-profile-img');
    
    // Fetch if it's currently the default avatar
    if (img && img.src.includes('default-avatar.png') && window.dotNetRef) {
        console.log("Fetching profile image via proxy for popup:", riderId);
        window.dotNetRef.invokeMethodAsync('GetRiderProfileImage', String(riderId))
        .then(base64 => {
            if (base64 && base64.startsWith('data:image')) {
                img.src = base64;
            }
        })
        .catch(err => {
            console.error("Profile image error in popup:", err);
        });
    }

    popup.style.display = "block";
    popup.style.visibility = "visible";
    popup.style.opacity = "1";
    popup.style.zIndex = "999999";

    window.currentPopupRiderId = String(riderId);

    requestAnimationFrame(() => {
        updatePopupPosition();
    });
}
function closeCustomPopup() {
    const popup = document.getElementById("custom-rider-popup");
    if (popup) {
        popup.style.display = "none";
        popup.style.visibility = "hidden";
        popup.style.opacity = "0";
    }
    window.currentPopupRiderId = null;
}

function updatePopupPosition() {
    if (!window.currentPopupRiderId) return;

    const popup = document.getElementById("custom-rider-popup");
    if (!popup || popup.style.display === "none") return;

    const overlayObj = window.riderOverlays[window.currentPopupRiderId];
    if (!overlayObj || !overlayObj.data || !overlayObj.wrapper) return;

    const lat = safeNumber(overlayObj.data.latitude ?? overlayObj.data.Latitude);
    const lng = safeNumber(overlayObj.data.longitude ?? overlayObj.data.Longitude);

    if (isNaN(lat) || isNaN(lng)) return;

    try {
        if (typeof window.mapInstance.project === "function") {
            const point = window.mapInstance.project([lng, lat]);
            if (point) {
                popup.style.left = `${point.x}px`;
                popup.style.top = `${point.y - window.popupYOffset}px`;
                return;
            }
        }

        const markerRect = overlayObj.wrapper.getBoundingClientRect();
        const mapEl = document.getElementById("mappel");
        const mapRect = mapEl.getBoundingClientRect();

        popup.style.left = `${markerRect.left - mapRect.left + (markerRect.width / 2)}px`;
        popup.style.top = `${markerRect.top - mapRect.top - window.popupYOffset}px`;
    } catch (err) {
        console.warn("updatePopupPosition error:", err);
    }
}

window.focusRider = function(riderId) {
    const overlay = window.riderOverlays[riderId];
    if (overlay && overlay.data && window.mapInstance) {
        const lat = safeNumber(overlay.data.latitude ?? overlay.data.Latitude);
        const lng = safeNumber(overlay.data.longitude ?? overlay.data.Longitude);
        
        if (!isNaN(lat) && !isNaN(lng)) {
            window.mapInstance.setCenter({ lat, lng });
            window.mapInstance.setZoom(16);
            showCustomPopup(overlay.data, riderId);
        }
    }
};

window.openRiderChat = function(riderId) {
    console.log("Chat requested for:", riderId);
    if (window.dotNetRef) {
        window.dotNetRef.invokeMethodAsync('OpenRiderChat', String(riderId));
    }
};

window.toggleRiderStatus = function(riderId, currentStatus) {
    console.log("Toggle status requested for:", riderId, currentStatus);
    if (window.dotNetRef) {
        window.dotNetRef.invokeMethodAsync('ToggleRiderStatus', String(riderId), currentStatus);
    }
};

window.updateMarkers = function (locations) {
    try {
        if (!window.mapInstance || !locations) return;

        ensureOverlayLayer();
        ensureCustomPopupElement();

        const layer = document.getElementById("rider-overlay-layer");
        if (!layer) return;

        const activeIds = new Set();
        const validPoints = [];

        locations.forEach((item) => {
            const riderId = item.riderId ?? item.RiderId;
            const riderName = item.riderName ?? item.RiderName ?? "Unknown";
            const riderStatus = item.status ?? item.Status ?? "Active";
            const vehicleType = item.vehicleType ?? item.VehicleType ?? "bike";
            const lat = safeNumber(item.latitude ?? item.Latitude);
            const lng = safeNumber(item.longitude ?? item.Longitude);

            console.log("RIDER:", riderId, riderName, riderStatus, vehicleType, lat, lng);

            if (!riderId || isNaN(lat) || isNaN(lng)) return;

            if (lat < -90 || lat > 90 || lng < -180 || lng > 180) {
                console.error("INVALID GPS:", item);
                return;
            }

            activeIds.add(String(riderId));
            validPoints.push([lng, lat]);

            if (window.riderOverlays[riderId]) {
                updateRiderElement(window.riderOverlays[riderId], item);
            } else {
                const overlay = createRiderElement(item, riderId);
                layer.appendChild(overlay.wrapper);
                window.riderOverlays[riderId] = overlay;
            }

            placeOverlayElement(window.riderOverlays[riderId]);

            // Update proximity circles if enabled
            if (window.showProximityCircles) {
                const status = riderStatus.toLowerCase();
                const statusClass = status.includes("permission") ? "permission"
                                  : (status.includes("offline") || status.includes("loggedout")) ? "offline"
                                  : (status.includes("hold") || status.includes("busy") || status.includes("engaged") || status.includes("transit")) ? "onhold"
                                  : "active";
                updateOrCreateRiderCircle(riderId, lat, lng, statusClass);
            }
        });

        Object.keys(window.riderOverlays).forEach(id => {
            if (!activeIds.has(String(id))) {
                const overlayObj = window.riderOverlays[id];
                if (overlayObj && overlayObj.wrapper && overlayObj.wrapper.parentNode) {
                    overlayObj.wrapper.parentNode.removeChild(overlayObj.wrapper);
                }

                if (window.currentPopupRiderId === String(id)) {
                    closeCustomPopup();
                }

                removeRiderCircle(id);

                delete window.riderOverlays[id];
            }
        });

        if (!window.userInteracted && validPoints.length > 0) {
            if (validPoints.length === 1) {
                const p = validPoints[0];
                if (typeof window.mapInstance.setCenter === "function") {
                    window.mapInstance.setCenter({ lng: p[0], lat: p[1] });
                }
                if (typeof window.mapInstance.setZoom === "function") {
                    window.mapInstance.setZoom(15);
                }
            } else if (window.mapInstance.fitBounds) {
                const bounds = validPoints.reduce(
                    (b, p) => b.extend(p),
                    new mappls.LngLatBounds(validPoints[0], validPoints[0])
                );

                window.mapInstance.fitBounds(bounds, { padding: 60 });
            }
        }

        repositionAllRiders();
        updatePopupPosition();
    } catch (e) {
        console.error("updateMarkers error:", e);
    }
};

/* Proximity Circles Layer - 5km active rider overlays */
window.riderCircles = {};
window.showProximityCircles = false;

window.toggleProximityZones = function(enabled) {
    window.showProximityCircles = !!enabled;
    if (!window.showProximityCircles) {
        Object.keys(window.riderCircles).forEach(id => {
            removeRiderCircle(id);
        });
    } else {
        Object.keys(window.riderOverlays).forEach(id => {
            const item = window.riderOverlays[id].data;
            const lat = safeNumber(item.latitude ?? item.Latitude);
            const lng = safeNumber(item.longitude ?? item.Longitude);
            const status = (item.status ?? item.Status ?? "active").toLowerCase();
            const statusClass = status.includes("permission") ? "permission"
                              : (status.includes("offline") || status.includes("loggedout")) ? "offline"
                              : (status.includes("hold") || status.includes("busy") || status.includes("engaged") || status.includes("transit")) ? "onhold"
                              : "active";
            updateOrCreateRiderCircle(id, lat, lng, statusClass);
        });
    }
};

function updateOrCreateRiderCircle(riderId, lat, lng, statusClass) {
    if (!window.showProximityCircles) {
        removeRiderCircle(riderId);
        return;
    }

    const radiusInMeters = 5000; // 5km
    const statusColor = statusClass === "active" ? "#10b981" 
                      : statusClass === "onhold" ? "#f59e0b" 
                      : statusClass === "permission" ? "#94a3b8" 
                      : "#dc2626";

    // Draw only for online riders (Ready / Busy / On Hold)
    const isOnline = statusClass === "active" || statusClass === "onhold";
    if (!isOnline) {
        removeRiderCircle(riderId);
        return;
    }

    let existingCircle = window.riderCircles[riderId];
    if (existingCircle) {
        if (typeof existingCircle.setOptions === "function") {
            existingCircle.setOptions({
                center: { lat, lng },
                strokeColor: statusColor,
                fillColor: statusColor
            });
            return;
        } else {
            removeRiderCircle(riderId);
        }
    }

    try {
        window.riderCircles[riderId] = new mappls.Circle({
            map: window.mapInstance,
            center: { lat, lng },
            radius: radiusInMeters,
            fillColor: statusColor,
            fillOpacity: 0.12,
            strokeColor: statusColor,
            strokeOpacity: 0.4,
            strokeWeight: 1.5
        });
    } catch (e) {
        console.error("Circle creation error:", e);
    }
}

function removeRiderCircle(riderId) {
    const circle = window.riderCircles[riderId];
    if (circle) {
        try {
            if (typeof mappls.remove === "function") {
                mappls.remove({ map: window.mapInstance, layer: circle });
            } else if (typeof mappls.removeLayer === "function") {
                mappls.removeLayer({ map: window.mapInstance, layer: circle });
            } else if (typeof circle.remove === "function") {
                circle.remove();
            }
        } catch (e) {
            console.error("Error removing circle:", e);
        }
        delete window.riderCircles[riderId];
    }
}

/* Client heatmaps based on dynamic weighted pickup queues */
window.clientHeatmapInstance = null;

window.toggleClientHeatmap = function (enabled, points) {
    if (window.clientHeatmapInstance) {
        try {
            if (Array.isArray(window.clientHeatmapInstance)) {
                window.clientHeatmapInstance.forEach(layer => {
                    if (typeof mappls.remove === "function") {
                        mappls.remove({ map: window.mapInstance, layer: layer });
                    } else if (typeof mappls.removeLayer === "function") {
                        mappls.removeLayer({ map: window.mapInstance, layer: layer });
                    } else if (typeof layer.remove === "function") {
                        layer.remove();
                    }
                });
            } else {
                if (typeof mappls.remove === "function") {
                    mappls.remove({ map: window.mapInstance, layer: window.clientHeatmapInstance });
                } else if (typeof mappls.removeLayer === "function") {
                    mappls.removeLayer({ map: window.mapInstance, layer: window.clientHeatmapInstance });
                } else if (typeof window.clientHeatmapInstance.remove === "function") {
                    window.clientHeatmapInstance.remove();
                }
            }
        } catch (e) {
            console.error("Error removing heatmap:", e);
        }
        window.clientHeatmapInstance = null;
    }

    if (!enabled || !points || points.length === 0) return;

    // Clean and validate points
    const cleanPoints = points.map(p => ({
        lat: safeNumber(p.lat),
        lng: safeNumber(p.lng),
        weight: safeNumber(p.weight) || 1
    })).filter(p => !isNaN(p.lat) && !isNaN(p.lng) && p.lat !== 0 && p.lng !== 0);

    if (cleanPoints.length === 0) return;

    try {
        console.log("Initializing client heatmap with points:", cleanPoints);
        if (typeof mappls.HeatmapLayer === "function") {
            window.clientHeatmapInstance = new mappls.HeatmapLayer({
                map: window.mapInstance,
                data: cleanPoints,
                opacity: 0.85,
                radius: 25,
                maxIntensity: 10
            });
        } else {
            console.warn("mappls.HeatmapLayer missing. Using custom overlapping client indicators.");
            window.clientHeatmapInstance = [];
            cleanPoints.forEach(p => {
                const heatCircle = new mappls.Circle({
                    map: window.mapInstance,
                    center: { lat: p.lat, lng: p.lng },
                    radius: 1000 + (p.weight * 400),
                    fillColor: "#ef4444",
                    fillOpacity: Math.min(0.1 + (p.weight * 0.08), 0.45),
                    strokeColor: "#ef4444",
                    strokeOpacity: 0.15,
                    strokeWeight: 1
                });
                window.clientHeatmapInstance.push(heatCircle);
            });
        }
    } catch (e) {
        console.error("Error drawing heatmap, using fallback circles:", e);
        if (!window.clientHeatmapInstance) {
            window.clientHeatmapInstance = [];
            cleanPoints.forEach(p => {
                const heatCircle = new mappls.Circle({
                    map: window.mapInstance,
                    center: { lat: p.lat, lng: p.lng },
                    radius: 1000 + (p.weight * 400),
                    fillColor: "#ef4444",
                    fillOpacity: Math.min(0.1 + (p.weight * 0.08), 0.45),
                    strokeColor: "#ef4444",
                    strokeOpacity: 0.15,
                    strokeWeight: 1
                });
                window.clientHeatmapInstance.push(heatCircle);
            });
        }
    }
};

/* Rider Route Playback System - Frame-based Smooth GPS animation */
window.playbackState = {
    active: false,
    riderId: null,
    points: [],
    currentIndex: 0,
    isPlaying: false,
    speed: 1,
    playbackMarker: null,
    playbackPath: null,
    animationFrameId: null,
    lastTime: null
};

window.triggerRoutePlayback = function(riderId, riderName) {
    closeCustomPopup();
    if (window.dotNetRef) {
        window.dotNetRef.invokeMethodAsync('RequestRoutePlayback', String(riderId), String(riderName));
    }
};

function createPlaybackControls() {
    let controls = document.getElementById("playback-controls-panel");
    if (!controls) {
        controls = document.createElement("div");
        controls.id = "playback-controls-panel";
        controls.style.position = "absolute";
        controls.style.bottom = "20px";
        controls.style.left = "50%";
        controls.style.transform = "translateX(-50%)";
        controls.style.zIndex = "99999";
        controls.style.width = "90%";
        controls.style.maxWidth = "440px";
        controls.style.background = "rgba(15, 23, 42, 0.9)";
        controls.style.backdropFilter = "blur(12px)";
        controls.style.border = "1px solid rgba(255,255,255,0.12)";
        controls.style.borderRadius = "16px";
        controls.style.padding = "14px 20px";
        controls.style.boxShadow = "0 20px 40px rgba(0,0,0,0.4)";
        controls.style.display = "flex";
        controls.style.flexDirection = "column";
        controls.style.gap = "8px";
        controls.style.pointerEvents = "auto";

        const mapEl = document.getElementById("mappel");
        if (mapEl) {
            mapEl.appendChild(controls);
        }
    }
    return controls;
}

window.startRoutePlayback = function (riderId, riderName, points) {
    console.log("startRoutePlayback called for rider:", riderId, points);

    window.stopRoutePlayback();

    if (!points || points.length === 0) {
        alert("No historical GPS data found for this rider today.");
        return;
    }

    // Add visual styling for marker dimming to head if it does not exist
    if (!document.getElementById('mappls-playback-dim-style')) {
        var style = document.createElement('style');
        style.id = 'mappls-playback-dim-style';
        style.textContent = `
            .dimmed-all-except-playback .rider-overlay-marker {
                opacity: 0.15 !important;
                pointer-events: none !important;
                transition: opacity 0.3s ease;
            }
            .dimmed-all-except-playback .pulse-halo {
                animation: none !important;
                opacity: 0 !important;
            }
        `;
        document.head.appendChild(style);
    }

    const layer = document.getElementById("rider-overlay-layer");
    if (layer) {
        layer.classList.add("dimmed-all-except-playback");
    }

    const firstPt = points[0];
    if (window.mapInstance) {
        window.mapInstance.setCenter({ lat: firstPt.lat, lng: firstPt.lng });
        window.mapInstance.setZoom(14.5);
    }

    window.playbackState = {
        active: true,
        riderId: riderId,
        points: points,
        currentIndex: 0,
        isPlaying: true,
        speed: 1,
        playbackMarker: null,
        playbackPath: null,
        animationFrameId: null,
        lastTime: null
    };

    try {
        window.playbackState.playbackPath = new mappls.Polyline({
            map: window.mapInstance,
            paths: points.map(p => ({ lat: p.lat, lng: p.lng })),
            strokeColor: "#0ea5e9",
            strokeOpacity: 0.85,
            strokeWeight: 5
        });
    } catch(e) {
        console.error("Polyline creation error:", e);
    }

    try {
        window.playbackState.playbackMarker = new mappls.Marker({
            map: window.mapInstance,
            position: { lat: firstPt.lat, lng: firstPt.lng },
            icon: "/images/bike.png",
            width: 32,
            height: 32
        });
    } catch(e) {
        console.error("Marker creation error:", e);
    }

    const panel = createPlaybackControls();
    panel.style.display = "flex";
    panel.innerHTML = `
        <div style="display:flex;justify-content:space-between;align-items:center;color:#fff;font-size:12px;font-weight:700;">
            <span id="playback-rider-name" style="color:#0ea5e9;">Tracking: ${riderName}</span>
            <span id="playback-timestamp" style="background:rgba(255,255,255,0.08);padding:2px 8px;border-radius:6px;font-family:monospace;">12:00 PM</span>
        </div>
        <div style="display:flex;align-items:center;gap:10px;width:100%;">
            <input type="range" id="playback-scrubber" min="0" max="100" value="0" style="
                flex:1;
                height:6px;
                border-radius:3px;
                background:rgba(255,255,255,0.15);
                outline:none;
                cursor:pointer;
                accent-color:#0ea5e9;
            "/>
        </div>
        <div style="display:flex;justify-content:space-between;align-items:center;margin-top:4px;">
            <div style="display:flex;align-items:center;gap:12px;">
                <button id="playback-play-btn" style="
                    background:none;
                    border:none;
                    color:#fff;
                    font-size:18px;
                    cursor:pointer;
                    width:30px;
                    height:30px;
                    display:flex;
                    align-items:center;
                    justify-content:center;
                    border-radius:50%;
                    background:rgba(255,255,255,0.08);
                    transition:all 0.2s;
                "><i class="bi bi-pause-fill" style="font-size:18px;"></i></button>
                
                <select id="playback-speed-select" style="
                    background:rgba(15,23,42,0.6);
                    border:1px solid rgba(255,255,255,0.12);
                    color:#fff;
                    font-size:11px;
                    font-weight:700;
                    padding:3px 6px;
                    border-radius:6px;
                    outline:none;
                    cursor:pointer;
                ">
                    <option value="1">1x Speed</option>
                    <option value="2.5">2.5x Speed</option>
                    <option value="5">5x Speed</option>
                    <option value="12">12x Speed</option>
                </select>
            </div>
            
            <button id="playback-close-btn" style="
                background:rgba(239,68,68,0.15);
                border:1px solid rgba(239,68,68,0.3);
                color:#f87171;
                font-size:11px;
                font-weight:800;
                padding:4px 12px;
                border-radius:8px;
                cursor:pointer;
                transition:all 0.2s;
            ">Exit Playback</button>
        </div>
    `;

    const playBtn = document.getElementById("playback-play-btn");
    if (playBtn) {
        playBtn.onclick = function() {
            if (window.playbackState.isPlaying) {
                window.playbackState.isPlaying = false;
                playBtn.innerHTML = '<i class="bi bi-play-fill" style="font-size:18px;"></i>';
            } else {
                const maxIdx = window.playbackState.points.length - 1;
                if (window.playbackState.currentIndex >= maxIdx) {
                    window.playbackState.currentIndex = 0;
                }
                window.playbackState.isPlaying = true;
                window.playbackState.lastTime = null;
                playBtn.innerHTML = '<i class="bi bi-pause-fill" style="font-size:18px;"></i>';
                window.playbackState.animationFrameId = requestAnimationFrame(playbackLoop);
            }
        };
    }

    const speedSelect = document.getElementById("playback-speed-select");
    if (speedSelect) {
        speedSelect.onchange = function() {
            window.playbackState.speed = parseFloat(this.value);
        };
    }

    const closeBtn = document.getElementById("playback-close-btn");
    if (closeBtn) {
        closeBtn.onclick = function() {
            window.stopRoutePlayback();
        };
    }

    const scrubber = document.getElementById("playback-scrubber");
    if (scrubber) {
        scrubber.addEventListener("input", function() {
            const pct = parseFloat(this.value) / 100;
            const progress = pct * (window.playbackState.points.length - 1);
            updatePlaybackPosition(progress);
            window.playbackState.currentIndex = progress;
        });
    }

    window.playbackState.animationFrameId = requestAnimationFrame(playbackLoop);
};

function updatePlaybackPosition(progress) {
    if (!window.playbackState.active || window.playbackState.points.length === 0) return;

    const maxIdx = window.playbackState.points.length - 1;
    let p = Math.max(0, Math.min(progress, maxIdx));

    const idx = Math.floor(p);
    const fraction = p - idx;

    let lat, lng, timeStr;

    if (idx >= maxIdx) {
        const lastPt = window.playbackState.points[maxIdx];
        lat = lastPt.lat;
        lng = lastPt.lng;
        timeStr = lastPt.time;
    } else {
        const ptA = window.playbackState.points[idx];
        const ptB = window.playbackState.points[idx + 1];

        lat = ptA.lat + fraction * (ptB.lat - ptA.lat);
        lng = ptA.lng + fraction * (ptB.lng - ptA.lng);
        timeStr = ptA.time;
    }

    if (window.playbackState.playbackMarker) {
        window.playbackState.playbackMarker.setPosition({ lat, lng });
    }

    const scrubber = document.getElementById("playback-scrubber");
    if (scrubber) {
        scrubber.value = (p / maxIdx) * 100;
    }

    const timestampDisplay = document.getElementById("playback-timestamp");
    if (timestampDisplay && timeStr) {
        try {
            const date = new Date(timeStr);
            if (!isNaN(date.getTime())) {
                timestampDisplay.innerText = date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' });
            } else {
                timestampDisplay.innerText = timeStr.split('T')[1]?.split('.')[0] || timeStr;
            }
        } catch(e) {
            timestampDisplay.innerText = timeStr;
        }
    }
}

function playbackLoop(timestamp) {
    if (!window.playbackState.active || !window.playbackState.isPlaying) return;

    if (!window.playbackState.lastTime) {
        window.playbackState.lastTime = timestamp;
    }

    const elapsed = timestamp - window.playbackState.lastTime;
    window.playbackState.lastTime = timestamp;

    const baseStepPerSecond = 0.8; 
    const increment = (elapsed / 1000) * baseStepPerSecond * window.playbackState.speed;

    window.playbackState.currentIndex += increment;

    const maxIdx = window.playbackState.points.length - 1;
    if (window.playbackState.currentIndex >= maxIdx) {
        window.playbackState.currentIndex = maxIdx;
        window.playbackState.isPlaying = false;
        
        const playBtn = document.getElementById("playback-play-btn");
        if (playBtn) playBtn.innerHTML = '<i class="bi bi-play-fill" style="font-size:18px;"></i>';
    }

    updatePlaybackPosition(window.playbackState.currentIndex);

    if (window.playbackState.isPlaying) {
        window.playbackState.animationFrameId = requestAnimationFrame(playbackLoop);
    }
}

window.stopRoutePlayback = function () {
    if (window.playbackState.animationFrameId) {
        cancelAnimationFrame(window.playbackState.animationFrameId);
    }

    if (window.playbackState.playbackMarker) {
        try {
            if (typeof mappls.remove === "function") {
                mappls.remove({ map: window.mapInstance, layer: window.playbackState.playbackMarker });
            } else if (typeof mappls.removeLayer === "function") {
                mappls.removeLayer({ map: window.mapInstance, layer: window.playbackState.playbackMarker });
            } else if (typeof window.playbackState.playbackMarker.remove === "function") {
                window.playbackState.playbackMarker.remove();
            }
        } catch(e) {}
    }

    if (window.playbackState.playbackPath) {
        try {
            if (typeof mappls.remove === "function") {
                mappls.remove({ map: window.mapInstance, layer: window.playbackState.playbackPath });
            } else if (typeof mappls.removeLayer === "function") {
                mappls.removeLayer({ map: window.mapInstance, layer: window.playbackState.playbackPath });
            } else if (typeof window.playbackState.playbackPath.remove === "function") {
                window.playbackState.playbackPath.remove();
            }
        } catch(e) {}
    }

    const layer = document.getElementById("rider-overlay-layer");
    if (layer) {
        layer.classList.remove("dimmed-all-except-playback");
    }

    const panel = document.getElementById("playback-controls-panel");
    if (panel) {
        panel.style.display = "none";
    }

    window.playbackState.active = false;
    window.playbackState.isPlaying = false;
};