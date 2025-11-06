const ctx = document.getElementById("attendanceChart");

new Chart(ctx, {
    type: "bar",
    data: {
        labels: ["Present", "Absent", "On Leave", "Late"],
        datasets: [{
            label: "Today",
            data: [41, 3, 2, 5]
        }]
    }
});
