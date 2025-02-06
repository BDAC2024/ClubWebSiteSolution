async function initializeMaps(mapId, siteLat, siteLng) {

    // See https://developers.google.com/maps/documentation/javascript/style-reference
    const styles = {
        default: [],
        hide: [
            //{
            //    featureType: "all",
            //    elementType: "all",
            //    stylers: [{ visibility: "off" }],
            //},
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

    var latlng = new google.maps.LatLng(siteLat, siteLng);
    var options = {
        zoom: 14, center: latlng,
        mapTypeId: google.maps.MapTypeId.SATELLITE
    };
    var map = new google.maps.Map(document.getElementById(mapId), options);
    map.setOptions({ styles: styles["hide"] });

    new google.maps.Marker({
        position: latlng,
        map,
        title: "Measurement Station",
    });

} 