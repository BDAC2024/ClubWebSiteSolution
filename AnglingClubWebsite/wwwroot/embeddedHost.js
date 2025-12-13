/* TODO Ang to Blazor Migration - whole file only needed whilst migrating */
window.blazorHost = {
  start: function (dotnetObjRef) {

    const allowedOrigins = [
      'http://localhost:4200',      // Angular dev
      window.location.origin        // prod (same origin, /new)
    ];

    window.addEventListener('message', function (e) {

      //console.log('[Blazor iframe] message received', e.origin, e.data);

      if (!allowedOrigins.includes(e.origin)) {
        console.log('[Blazor iframe] origin rejected');
        return;
      }

      const msg = e.data;
      if (!msg || typeof msg !== 'object') return;

      if (msg.type === 'navigate') {
        dotnetObjRef.invokeMethodAsync('HandleNavigate', msg.path || '');
      } else if (msg.type === 'auth') {
        dotnetObjRef.invokeMethodAsync('HandleAuth', msg.token || null, msg.rememberMe || false);
      }
    }, false);

    // Let parent know Blazor is ready for messages
    // parent origin differs in dev, so we must NOT use window.location.origin here
    console.log("Blazor sending ready message to parent");
    window.parent.postMessage({ type: 'blazor-ready' }, '*');

  }
};
