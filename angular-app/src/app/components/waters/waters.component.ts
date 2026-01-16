import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { AddEditWaterDialogComponent } from 'src/app/dialogs/add-edit-water-dialog/add-edit-water-dialog.component';
import { Water } from 'src/app/models/water';
import { AuthenticationService } from 'src/app/services/auth/authentication.service';
import { MembersService } from 'src/app/services/members.service';
import { WatersService } from 'src/app/services/waters.service';
import { DomSanitizer, SafeResourceUrl, SafeUrl } from '@angular/platform-browser';
import { ScreenService } from 'src/app/services/screen.service';
import { RefDataService } from 'src/app/services/ref-data.service';
import { RefData } from 'src/app/models/refData';
import {ActivatedRoute} from '@angular/router'; 
import { Inject } from '@angular/core';
import { DOCUMENT } from '@angular/common';
@Component({
  selector: 'app-waters',
  templateUrl: './waters.component.html',
  styleUrls: ['./waters.component.css']
})
export class WatersComponent implements OnInit {

  waters!: Water[];
  pathColour: string = "lightgreen";
  mapType: string = 'satellite';

  youTubeEmbedRoot: string = "https://www.youtube-nocookie.com/embed/";
  videoWidth: number = 500;
  videoHeight: number = 315;
  
  public isLoading: boolean = false;
  public isPageAdmin: boolean = false;
  public refData!: RefData;
  public fragment: string = "";
  public bookmark!: string;
  public goneToBookmark: boolean = false;

  constructor(
    // @Inject(DOCUMENT) private document: Document,
    public refDataService: RefDataService,
    public watersService: WatersService,
    public membersService: MembersService,
    public authenticationService: AuthenticationService,
    public screenService: ScreenService,
    private dialog: MatDialog,
    private router: Router,
    private sanitizer: DomSanitizer,
    private route: ActivatedRoute) { 

      this.videoHeight = this.videoWidth / (16 / 9);

      screenService.OrientationChange.on(() => {
        this.videoWidth = screenService.IsHandsetPortrait ? 260 : 500;
        this.videoHeight = this.videoWidth / (16 / 9);
      });
  
  }

  ngAfterViewChecked() {
    // contentChild is updated after the content has been checked

    if (this.waters != undefined && this.waters.length > 0)
    {
      if (!this.goneToBookmark) {
        this.goneToBookmark = true;
        // console.log("ngAfterViewChecked: " + this.bookmark);
        var anchor = document.querySelector("#" + this.bookmark);
        if (anchor != undefined && anchor != null) {
          anchor.scrollIntoView();
        }
      }
    }
  }

  ngOnInit(): void {
    this.getWaters();
    this.getRefData();
    // this.path = this.watersService.PathOld(this.waters[2]);

    // this.path.forEach((p) => {console.log(`lat: ${p.lat}, long: ${p.long}`) });

    // this.path.push({ lat: 54.098626321067286, long: -1.3566482946865484});
    // this.path.push({ lat: 54.09867070551979, long: -1.357407887485396});
    // this.path.push({ lat: 54.098134031424564, long: -1.3576232247914872});

    this.route.fragment.subscribe(fragment => { this.fragment = fragment!; });
  }


  ngAfterViewInit(): void {
    try {
      this.bookmark = this.fragment;
    } catch (e) { }
  }

  private videoUrlCache = new Map<string, SafeResourceUrl>();

  public videoURL(videoShortCode: string): SafeResourceUrl {
    let cached = this.videoUrlCache.get(videoShortCode);
    if (!cached) {
      cached = this.sanitizer.bypassSecurityTrustResourceUrl(
        this.youTubeEmbedRoot + videoShortCode
      );
      this.videoUrlCache.set(videoShortCode, cached);
    }
    return cached;
  }

  trackByWaterId(_index: number, item: Water) {
    return item.id;
  }

  public enableAdmin(set: boolean): void {
    this.isPageAdmin = set && this.membersService.CurrentMember.admin;
  }

  public editWater(originalItem: Water): void {

    var item: Water = JSON.parse(JSON.stringify(originalItem));

    const dialogRef = this.dialog.open(AddEditWaterDialogComponent, {
      width: '90vw',
      data: item
    });

    dialogRef.afterClosed().subscribe(result => {
      console.log(`The dialog was closed : `);
      console.log(result);

      if (result) {
        console.log("Editing: " + item.name);

        this.watersService.addOrUpdateWater(result)
        .subscribe(data => {
          this.getWaters();
        });
      }
    });

  }

  public getRefData() {
    this.isLoading = true;
    this.refDataService.getRefData()
    .subscribe(data => {
      this.refData = data;
      this.isLoading = false;
    });
  }

  private getWaters(): void
  {
    this.isLoading = true;

    this.watersService.readWaters()
    .subscribe(data => {
      this.isLoading = false;
      this.waters = data;

      this.waters.forEach((w) => {
        w.markers.forEach((m) => {
          m.icon = `assets/${m.icon}.png`;
        });
      });
      //console.log("getWaters finished");
  
    });

  }

  public memberLogin(): void {
    this.router.navigate(['/login'], { queryParams: { returnUrl: "/waters" } });
  }
}


