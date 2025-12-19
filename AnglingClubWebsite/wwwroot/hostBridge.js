// wwwroot/hostBridge.js
// TODO Ang to Blazor Migration - only required whilst migrating
window.blazorHostBridge = {
    requestLogin: function (blazorPage) {
        // Only if we're inside an iframe
        if (window.parent && window.parent !== window) {
            const message = {
                source: 'BLAZOR',
                type: 'REQUEST_LOGIN',
                // full URL, fall back to current if not provided
                //blazorPage: blazorPage || (window.location.pathname + window.location.search)
                blazorPage: blazorPage
            };

            console.warn('blazorHostBridge.requestLogin message being sent to parent frame', blazorPage);
            // TODO: replace '*' with your Angular origin in prod
            window.parent.postMessage(message, '*');
        } else {
            console.warn('blazorHostBridge.requestLogin: no parent frame');
        }
    },
    requestAngPage: function (angPage) {
        // Only if we're inside an iframe
        if (window.parent && window.parent !== window) {
            const message = {
                source: 'BLAZOR',
                type: 'REQUEST_PAGE',
                // full URL, fall back to current if not provided
                //blazorPage: blazorPage || (window.location.pathname + window.location.search)
                angPage: angPage
            };

            console.warn('blazorHostBridge.requestAngPage message being sent to parent frame', angPage);
            // TODO: replace '*' with your Angular origin in prod
            window.parent.postMessage(message, '*');
        } else {
            console.warn('blazorHostBridge.requestAngPage: no parent frame');
        }
    },
    isInIFrame: function () {
        try {
            return window.self !== window.top;
        } catch {
            // Cross-origin iframe – still means "embedded"
            return true;
        }
    }
};
