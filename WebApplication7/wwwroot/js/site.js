// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

let map;
function initMap() {
    const startCords = { lat: 50.81428919759016, lng: 19.10011028298656 };
    map = new google.maps.Map(document.getElementById("map"), {
        zoom: 12,
        center: startCords,
    });
}

window.initMap = initMap;

document.querySelector("form").addEventListener("submit", (e) => {
    e.preventDefault();
    const data = [...document.querySelectorAll("input")];
    markerCords = { lat: Number(data[1].value), lng: Number(data[2].value) };
    if (map) {
        const marker = new google.maps.Marker({
            position: markerCords,
            title: data[0].value,
            map: map
        });

        marker.setMap(map);

        fetch('/Home/AddLocation', {
            method: 'POST',
            body: formData = new FormData(document.querySelector("form"))
        });

        document.querySelector("form").reset();

    }
});

function displayMarkers(markers) {
    console.log(markers);
    for (let i = 0; i < markers.length; i++) {
        const marker = new google.maps.Marker({
            position: { lat: markers[i].latitude, lng: markers[i].longitude },
            title: markers[i].name,
            map: map,
        });
    }
}
function getMarkers(){
        fetch("/Home/GetUserMarkers", {
            method: 'GET'
        })
            .then((response) => response.json())
            .then((data) => {
                displayMarkers(data);
            });
}
getMarkers();