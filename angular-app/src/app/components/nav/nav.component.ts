import { Component, OnInit, ViewChild } from '@angular/core';
import { Observable } from 'rxjs';
import { filter, map, withLatestFrom } from 'rxjs/operators';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { MatSidenav } from '@angular/material/sidenav';
import { Router, NavigationEnd } from '@angular/router';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {

  @ViewChild('drawer', { static: false })
  drawer!: MatSidenav;

  isHandset$: Observable<boolean> = this.breakpointObserver.observe(Breakpoints.Handset)
    .pipe(
      map(result => result.matches)
    );

    constructor(
      private breakpointObserver: BreakpointObserver, 
      //private titleService: Title, 
      //private msalService: MsalService,
      //private logger: LogService,
      //private authService: AuthService,
      router: Router) {
  
      router.events.pipe(
        withLatestFrom(this.isHandset$),
        filter(([a, b]) => b && a instanceof NavigationEnd)
      ).subscribe(_ => this.drawer.close());
      this.title = "Boroughbridge & District Angling Club"// this.titleService.getTitle();
  
      this.previewCodeValid = true;
    }
  
      // Properties

  /** The title of the app as obtained frm the titleService
   *
   * @defaultValue ''
   */  
  public title: string;

  public get loggedIn(): boolean {
    return false;
  }

  public previewCodeValid: boolean;

  public userName!: string;

  ngOnInit(): void {
  }

    // Methods

  /** Logs out the user by calling the auth service
   * @param inp - The best arg ever
   * @param outp - The resultant number
   */
   public logout(): void {
  }

}
