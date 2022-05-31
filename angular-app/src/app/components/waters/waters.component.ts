import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { AddEditWaterDialogComponent } from 'src/app/dialogs/add-edit-water-dialog/add-edit-water-dialog.component';
import { Water } from 'src/app/models/water';
import { AuthenticationService } from 'src/app/services/auth/authentication.service';
import { MembersService } from 'src/app/services/members.service';
import { WatersService } from 'src/app/services/waters.service';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { ScreenService } from 'src/app/services/screen.service';

@Component({
  selector: 'app-waters',
  templateUrl: './waters.component.html',
  styleUrls: ['./waters.component.css']
})
export class WatersComponent implements OnInit {

  waters!: Water[];
  pathColour: string = "lightgreen";
  mapType: string = 'satellite';

  youTubeEmbedRoot: string = "https://www.youtube.com/embed/";
  videoWidth: number = 560;
  videoHeight: number = 315;
  
  public isLoading: boolean = false;
  public isPageAdmin: boolean = false;

  constructor(
    public watersService: WatersService,
    public membersService: MembersService,
    public authenticationService: AuthenticationService,
    public screenService: ScreenService,
    private dialog: MatDialog,
    private router: Router,
    private sanitizer: DomSanitizer) { 

      screenService.OrientationChange.on(() => {
        this.videoWidth = screenService.IsHandsetPortrait ? 300 : 560;
        this.videoHeight = this.videoWidth / (16 / 9);
      });
  
  }

  ngOnInit(): void {
    this.getWaters();

    // this.path = this.watersService.PathOld(this.waters[2]);

    // this.path.forEach((p) => {console.log(`lat: ${p.lat}, long: ${p.long}`) });

    // this.path.push({ lat: 54.098626321067286, long: -1.3566482946865484});
    // this.path.push({ lat: 54.09867070551979, long: -1.357407887485396});
    // this.path.push({ lat: 54.098134031424564, long: -1.3576232247914872});
  }

  public videoURL(videoShortCode: string): SafeUrl {
    return this.sanitizer.bypassSecurityTrustResourceUrl(this.youTubeEmbedRoot + videoShortCode);
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
    });

  }

  public memberLogin(): void {
    this.router.navigate(['/login'], { queryParams: { returnUrl: "/waters" } });
  }
}
