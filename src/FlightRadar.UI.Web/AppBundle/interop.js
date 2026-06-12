export function getOrigin() {
    return window.location.origin;
}

export function consoleLog(message) {
    console.log(message);
}

export function openUrl(url) {
    window.open(url, '_blank');
}

export function initAudio() {
}

export function playPing(base64) {
    try {
        const audio = new Audio("data:audio/wav;base64," + base64);
        audio.volume = 0.4;
        audio.play().catch(function(e) {
            console.warn("playPing:play()", e.message);
        });
    } catch(e) {
        console.warn("playPing:error", e);
    }
}

export function getSystemLanguage() {
    return navigator.language || "en-US";
}

export function saveMute(muted) {
    try {
        localStorage.setItem("radarMuted", muted ? "1" : "0");
    } catch (e) {}
}

export function loadMute() {
    try {
        return localStorage.getItem("radarMuted") !== "0";
    } catch (e) {
        return true;
    }
}