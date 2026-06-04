export function getOrigin() {
    return window.location.origin;
}

export function showAlert(message) {
    globalThis.alert(message);
}