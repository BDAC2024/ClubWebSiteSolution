import { Component, OnInit, ViewChild } from '@angular/core';
import { Observable } from 'rxjs';
import { filter, map, withLatestFrom } from 'rxjs/operators';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { MatSidenav } from '@angular/material/sidenav';
import { Router, NavigationEnd } from '@angular/router';
import { PreviewService } from 'src/app/services/preview.service';
import { ScreenService } from 'src/app/services/screen.service';
import { GlobalService } from 'src/app/services/global.service';
import { AuthenticationService } from 'src/app/services/auth/authentication.service';
import { RefDataService } from 'src/app/services/ref-data.service';
import { RefData } from 'src/app/models/refData';
import { NewsService } from 'src/app/services/news.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {

  public refData!: RefData;
  
  // isExpanded = true;
  showStandingsSubMenu: boolean = false;
  showAdminSubMenu: boolean = false;
  showRulesSubMenu: boolean = false;
  showClubInfoSubMenu: boolean = false;
  showPurchaseSubMenu: boolean = false;
  showJnrSectionSubMenu: boolean = false;
  // isShowing = false;

  @ViewChild('drawer', { static: false })
  drawer!: MatSidenav;

  isHandset$: Observable<boolean> = this.breakpointObserver.observe(Breakpoints.Handset)
    .pipe(
      map(result => result.matches)
    );

  isHandsetPortrait$: Observable<boolean> = this.breakpointObserver.observe(Breakpoints.HandsetPortrait)
    .pipe(
      map(result => result.matches)
    );
  
    isHandsetLandscape$: Observable<boolean> = this.breakpointObserver.observe(Breakpoints.HandsetLandscape)
    .pipe(
      map(result => result.matches)
    );

  // Change column settings if portrait occurs
  portraitLayoutChanges = this.breakpointObserver.observe(Breakpoints.HandsetPortrait)
  .subscribe(result => {
    this.screenService.IsHandsetPortrait = result.matches;
  });

  // portraitTabletLayoutChanges = this.breakpointObserver.observe(Breakpoints.TabletPortrait)
  // .subscribe(result => {
  //   this.screenService.IsTabletPortrait = result.matches;
  // });

  // portraitWebLayoutChanges = this.breakpointObserver.observe(Breakpoints.WebPortrait)
  // .subscribe(result => {
  //   this.screenService.IsWebPortrait = result.matches;
  // });

  // Change column settings if landscape occurs
  landscapeLayoutChanges = this.breakpointObserver.observe(Breakpoints.HandsetLandscape)
  .subscribe(result => {
    this.screenService.IsHandsetLandscape = result.matches;
  });

  // landscapeTabletLayoutChanges = this.breakpointObserver.observe(Breakpoints.TabletLandscape)
  // .subscribe(result => {
  //   this.screenService.IsTabletLandscape = result.matches;
  // });
  
  // landscapeWebLayoutChanges = this.breakpointObserver.observe(Breakpoints.WebLandscape)
  // .subscribe(result => {
  //   this.screenService.IsWebLandscape = result.matches;
  // });
  
    constructor(
      private breakpointObserver: BreakpointObserver, 
      public previewService: PreviewService,
      private screenService: ScreenService,
      private globalService: GlobalService,
      public authenticationService: AuthenticationService,
      public refDataService: RefDataService,
      public newsService: NewsService,
      router: Router) {

        this.title = "Boroughbridge & District Angling Club"// this.titleService.getTitle();

        // Initial screen settings
        router.events.pipe(
          withLatestFrom(this.isHandset$),
          filter(([a, b]) => b && a instanceof NavigationEnd)
        ).subscribe(_ => this.drawer.close());
  
        router.events.pipe(
          withLatestFrom(this.isHandsetPortrait$)
        ).subscribe(result => {
          screenService.IsHandsetPortrait = result[1];
          this.globalService.log("Nav - Orientation done - portrait: " + screenService.IsHandsetPortrait );
        });

        router.events.pipe(
          withLatestFrom(this.isHandsetLandscape$)
        ).subscribe(result => {
          screenService.IsHandsetLandscape = result[1];
          this.globalService.log("Nav - Orientation done - landscape: " + screenService.IsHandsetLandscape );
        });

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

  public userName!: string;

  ngOnInit(): void {
    this.getRefData();
    this.newsService.isThereNewNews();
  }

  public getRefData() {
    this.refDataService.getRefData()
    .subscribe(data => {
      this.refData = data;

      this.refData.seasons = this.refData.seasons
      .filter((s) => {
        return s.season <= this.refData.currentSeason;
      })
      .sort((a, b) => {
        return a.season < b.season && 1 || -1;
      });

    });
  }

  public isPreviewer(): boolean {
    if (this.refData != null) {
      return this.authenticationService.isPreviewer(this.refData.appSettings.previewers);
    } else {
      return false;
    }
  }


}
