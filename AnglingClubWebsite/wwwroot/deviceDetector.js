function isDevice() {
    //alert(navigator.userAgent);
    return /android|webos|iphone|ipad|ipod|blackberry|iemobile|opera mini|mobile/i.test(navigator.userAgent);
}