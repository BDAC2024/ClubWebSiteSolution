// blazor-bridge.service.ts
import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class BlazorBridgeService {
  private frameWindow: Window | null = null;
  private isReady = false;
  private queuedPath: string | null = null;
  private latestAuth: { token: any | null; rememberMe: boolean | null } | null = null;


  setFrame(frame: HTMLIFrameElement) {
    this.frameWindow = frame.contentWindow;
    this.tryFlushQueue();
    this.trySendAuth();    
  }

  setReady(ready: boolean) {
    console.log('[BlazorBridge] setReady', ready);
    this.isReady = ready;

    if (ready) {
      this.tryFlushQueue();
       this.trySendAuth();   
    }

    // If we become ready and have a queued request, send it now
    if (ready && this.queuedPath && this.frameWindow) {
      this.sendNavigate(this.queuedPath);
      this.queuedPath = null;
    }
  }

  navigate(path: string) {
    console.log(`[BlazorBridge] navigate called [${path}] `, this.frameWindow);

    // Always remember the latest requested path
    this.queuedPath = path;

    // If we're already ready and have a frame, send right now
    this.tryFlushQueue();
  }

  private tryFlushQueue() {
    if (!this.isReady)
    {
      console.log('[BlazorBridge] not ready yet');
      return; 
    }
    if (!this.frameWindow)
    {
      console.log('[BlazorBridge] frameWindow not set yet');
      return;
    }
    if (!this.queuedPath && this.queuedPath !== '')
    {
      console.log('[BlazorBridge] no queued path to send');
      return;
    }

    this.sendNavigate(this.queuedPath);
    this.queuedPath = null;
  }

  private sendNavigate(path: string) {
    console.log(`[BlazorBridge] sending navigate to [${path}]`);

    if (!this.frameWindow) {
      console.warn('[BlazorBridge] frameWindow is not set!');
      return;
    }
    
    this.frameWindow.postMessage(
      { type: 'navigate', path: path },
      '*'  // Allow all origins
    );
  }

  sendAuth(token: any | null, rememberMe: boolean | null) {

    //console.log('[BlazorBridge] sendAuth called', { token, rememberMe });

    // Remember the latest auth payload
    this.latestAuth = { token, rememberMe };

    // Try to send immediately if possible
    this.trySendAuth();
    // console.log('[BlazorBridge] sending auth message');
    // if (!this.frameWindow) return;
    // this.frameWindow.postMessage(
    //   { type: 'auth', JSON.stringify(token), rememberMe },
    //   '*'  // Allow all origins
    // );
  }

  private trySendAuth() {
    if (!this.latestAuth) return;
    if (!this.frameWindow) return;
    if (!this.isReady) return;   // wait until Blazor sends "blazor-ready"

    console.log('[BlazorBridge] sending auth to iframe');
    this.frameWindow.postMessage(
      {
        type: 'auth',
        token: JSON.stringify(this.latestAuth.token),
        rememberMe: this.latestAuth.rememberMe
      }      ,
      '*'  // Allow all origins
    );
  }

  reset() {
    // Call this when the host component is destroyed
    console.log('[BlazorBridge] reset');
    this.isReady = false;
    this.frameWindow = null;
    this.queuedPath = null;
    // keep latestAuth so a new iframe can get it after a route back to /blazor
  }
}
