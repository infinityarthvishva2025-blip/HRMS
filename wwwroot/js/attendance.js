function checkin() {

    navigator.geolocation.getCurrentPosition(function (pos) {

        let lat = pos.coords.latitude;
        let lng = pos.coords.longitude;

        fetch('/Attendance/MarkAttendance', {

            method: 'POST',
            headers: { 'Content-Type': 'application/json' },

            body: JSON.stringify({
                latitude: lat,
                longitude: lng
            })

        })
            .then(r => r.json())
            .then(d => {

                document.getElementById("msg").innerText = d.message;

            });

    });

}