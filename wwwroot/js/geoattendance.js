//function refreshLocation() {
//    alert("Fetching updated location...");
//}

let map, marker, circle;

function initMap(lat = 19.0760, lng = 72.8777, radiusMeters = 100000) {
    if (!window.google || !google.maps) return console.error("Google Maps not loaded");
    const center = { lat, lng };
    map = new google.maps.Map(document.getElementById("map"), {
        center,
        zoom: 10,
    });
    marker = new google.maps.Marker({ position: center, map });
    circle = new google.maps.Circle({
        map,
        center,
        radius: radiusMeters,
        fillColor: "#AEE1FF",
        strokeColor: "#1CADA3"
    });
    map.fitBounds(circle.getBounds());
}

function updateMarker(lat, lng) {
    const pos = { lat, lng };
    if (!map) initMap(lat, lng);
    marker.setPosition(pos);
    map.panTo(pos);
}

function refreshLocation() {
    if (!navigator.geolocation) {
        alert("Geolocation not supported");
        return;
    }
    navigator.geolocation.getCurrentPosition(async (pos) => {
        const lat = pos.coords.latitude;
        const lng = pos.coords.longitude;

        document.getElementById("latlng").innerText = `${lat.toFixed(4)}, ${lng.toFixed(4)}`;

        updateMarker(lat, lng);
        // Optionally reverse-geocode to show city (use Google Geocoding or another service)
    }, (err) => {
        alert("Unable to get location: " + err.message);
    }, { enableHighAccuracy: true });
}

async function markAttendance() {
    const geoSelect = document.getElementById("geotagSelect");
    const geotagId = geoSelect.value;
    const employeeId = document.getElementById("employeeId").value || "emp-demo";

    if (!navigator.geolocation) { alert("No geolocation"); return; }

    navigator.geolocation.getCurrentPosition(async (pos) => {
        const lat = pos.coords.latitude;
        const lng = pos.coords.longitude;

        const selfieInput = document.getElementById("selfieInput");
        const form = new FormData();
        form.append("EmployeeId", employeeId);
        form.append("GeoTagId", geotagId);
        form.append("Latitude", lat);
        form.append("Longitude", lng);

        if (selfieInput.files.length > 0) {
            form.append("Selfie", selfieInput.files[0]);
        }

        const res = await fetch('/GeoAttendance/api/mark', {
            method: 'POST',
            body: form
        });

        const json = await res.json();
        alert("Server response: " + JSON.stringify(json));
    }, (err) => alert("Location denied: " + err.message), { enableHighAccuracy: true });
}

// on DOM ready
document.addEventListener('DOMContentLoaded', () => {
    // wire buttons
    const refreshBtn = document.getElementById('refreshBtn');
    if (refreshBtn) refreshBtn.addEventListener('click', refreshLocation);

    const markBtn = document.getElementById('markBtn');
    if (markBtn) markBtn.addEventListener('click', markAttendance);

    // initial map load (default coords)
    initMap(19.0760, 72.8777, 100000);
});

