import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { ActivatedRoute, Router } from '@angular/router';
import { BlazorBridgeService } from 'src/app/services/blazor-bridge.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-blazor-host',
  templateUrl: './blazor-host.component.html',
  styleUrls: ['./blazor-host.component.css']
})
export class BlazorHostComponent implements OnInit, OnDestroy {

  @ViewChild('blazorFrame', { static: true })
  frameRef!: ElementRef<HTMLIFrameElement>;
  
  iframeSrc!: SafeResourceUrl;

  blazorUrl!: SafeResourceUrl;
  private readonly baseBlazorUrl = environment.blazorBaseUrl;

  private readyListener?: (e: MessageEvent) => void;

  constructor(
    private sanitizer: DomSanitizer,
    private bridge: BlazorBridgeService,
    private router: Router,
    private route: ActivatedRoute,
  ) {}

  ngOnInit(): void {

    // Which page did Angular ask for when navigating to /blazor?
    // Prefer router state when available, otherwise fall back to history.state
    // 1. Read desired Blazor page from router state or history.state
    const nav = this.router.getCurrentNavigation();
    const state = (nav && nav.extras && nav.extras.state)
      ? nav.extras.state
      : window.history.state;
    const page = state && state['blazorPage'] as string | undefined;

    // 2. Decide base URL for Blazor
    //    - Dev: use localhost Blazor dev server
    //    - Prod: force /new on the current origin (ignore any weird env config)
    const base = environment.production
      ? `${window.location.origin}/new`
      : environment.blazorBaseUrl;   // e.g. 'http://localhost:7099'

    // 3. Build the final iframe URL
    //    page = 'news'  ->  https://site/new/news?embedded=true
    //    page = undefined -> https://site/new/?embedded=true
    const pathSegment = page ? `/${page}` : '/';
    const rawUrl = `${base}${pathSegment}?embedded=true`;

    console.log('[BlazorHost] iframeSrc set to', rawUrl);
    this.iframeSrc = this.sanitizer.bypassSecurityTrustResourceUrl(rawUrl);

    // Listen for "blazor-ready" so we know when we can send messages
    this.readyListener = (e: MessageEvent) => {
      console.log('[BlazorHost] message received', e);

      const allowedOrigins = [
        'https://localhost:7099',          // Blazor dev
        window.location.origin            // prod: same origin under /new
      ];

      if (!allowedOrigins.includes(e.origin)) {
        console.warn('[BlazorHost] ignoring message from unallowed origin', e.origin);
        return;
      }

      if (e.data?.type === 'blazor-ready') {
        this.bridge.setReady(true);
      }
    };
    window.addEventListener('message', this.readyListener);

    window.addEventListener('message', this.messageHandler);

    const state2 = window.history.state as { blazorPage?: string };
    const page2 = state2.blazorPage ?? '';

    this.openBlazor(page2);
  }

  openBlazor(page: string) {
    // page might be "matches?embedded=true"
    const fullUrl = page
      ? `${this.baseBlazorUrl}/${page}`
      : this.baseBlazorUrl;

    console.log('[BlazorHost] iframe URL:', fullUrl);
    this.blazorUrl = this.sanitizer.bypassSecurityTrustResourceUrl(fullUrl);
  }

  ngAfterViewInit(): void {
    console.log('[BlazorHost] setting frame');
    this.bridge.setFrame(this.frameRef.nativeElement);
  }

  ngOnDestroy(): void {
    this.bridge.reset();
    if (this.readyListener) {
      window.removeEventListener('message', this.readyListener);
    }

    if (this.messageHandler) {
      window.removeEventListener('message', this.messageHandler);
    }

  }

  private messageHandler = (event: MessageEvent) => {
    const allowedOrigins = [
      'https://localhost:7099',      // Blazor dev
      window.location.origin         // prod (same origin, /new)
    ];

    if (!allowedOrigins.includes(event.origin)) {
      console.log('[Ang messageHandler] origin rejected');
      return;
    }

    const data = event.data;
    if (!data || data.source !== 'BLAZOR') {
      return;
    }

    switch (data.type) {
      case 'REQUEST_LOGIN':
        this.handleRequestLogin(data.blazorPage);
        break;
      default:
        console.warn('Unknown message from Blazor:', data);
    }
  };

  private handleRequestLogin(blazorPage?: string) {
    this.router.navigate(['/login'], {
      state: blazorPage ? { blazorPage } : undefined
    });
  }
}
