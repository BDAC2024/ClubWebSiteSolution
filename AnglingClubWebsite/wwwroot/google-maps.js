async function initializeMaps(mapId, center, markers, pathWaypoints) {

    // See https://developers.google.com/maps/documentation/javascript/style-reference
    const styles = {
        default: [],
        hide: [
            {
                featureType: "poi",
                stylers: [{ visibility: "off" }],
            },
            {
                featureType: "transit",
                stylers: [{ visibility: "off" }],
            },
        ],
    };

    var mapCenter = new google.maps.LatLng(center.lat, center.long);
    var options = {
        zoom: 14, center: mapCenter,
        mapTypeId: google.maps.MapTypeId.SATELLITE,
        mapId: "DEMO_MAP_ID", // Map ID is required for advanced markers.
    };
    var map = new google.maps.Map(document.getElementById(mapId), options);
    //map.setOptions({ styles: styles["hide"] });

    //console.log("DB markers");
    //console.log(markers.length);
    //console.log(markers);

    for (var i = 0; i < markers.length; i++) {

        const iconImg = document.createElement("img");

        iconImg.src =
            "../images/" + markers[i].icon + ".png";

        new google.maps.marker.AdvancedMarkerElement({
            map,
            position: { lat: markers[i].position.lat, lng: markers[i].position.long },
            content: iconImg,
            title: markers[i].label,
        });
    }

    //console.log("pathWaypoints");
    //console.log(pathWaypoints.length);
    //console.log(pathWaypoints);

    if (pathWaypoints.length > 0) {
        var mapWaypoints = [pathWaypoints.length];
        for (var i = 0; i < pathWaypoints.length; i++)
        {
            mapWaypoints[i] =
            {
                lat: pathWaypoints[i].lat,
                lng: pathWaypoints[i].long
            };
        }

        //console.log("mapWaypoints");
        //console.log(mapWaypoints.length);
        //console.log(mapWaypoints);

        const mapPath = new google.maps.Polyline({
            path: mapWaypoints,
            geodesic: true,
            strokeColor: "lightgreen",
            strokeOpacity: 1.00,
            strokeWeight: 3,
        });

        mapPath.setMap(map);
    }
} 