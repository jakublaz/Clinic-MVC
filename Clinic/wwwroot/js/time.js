function updateClock() {
    var now = new Date();
    var currentTime = now.getHours().toString().padStart(2, '0') + ":" +
                      now.getMinutes().toString().padStart(2, '0') + ":" +
                      now.getSeconds().toString().padStart(2, '0') + " " +
                      now.getDate().toString().padStart(2, '0') + "-" +
                      (now.getMonth() + 1).toString().padStart(2, '0') + "-" +
                      now.getFullYear().toString();

    document.getElementById('currentTime').innerText = "Current Time and Date: " + currentTime;
}

// Update the clock every second
setInterval(updateClock, 1000);

// Initial call to display the time immediately
updateClock();