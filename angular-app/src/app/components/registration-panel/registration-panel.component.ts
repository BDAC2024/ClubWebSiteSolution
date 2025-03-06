import { HttpErrorResponse } from '@angular/common/http';
import { AfterViewInit, Component, Input, OnInit, ViewChild } from '@angular/core';
import { UntypedFormControl } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { ConfirmDialogComponent } from 'src/app/dialogs/confirm-dialog/confirm-dialog.component';
import { OpenMatch } from 'src/app/models/open-match';
import { JuniorAgeGroup } from 'src/app/models/open-match-enum';
import { OpenMatchRegistration } from 'src/app/models/open-match-registration';
import { OpenMatchService } from 'src/app/services/open-match.service';
import { ScreenService } from 'src/app/services/screen.service';

@Component({
  selector: 'app-registration-panel',
  templateUrl: './registration-panel.component.html',
  styleUrls: ['./registration-panel.component.css']
})
export class RegistrationPanelComponent implements OnInit, AfterViewInit {

  @Input() isAdmin = false;
  @ViewChild(MatSort) sort!: MatSort;

  public errorMessage: string = "";
  emailControl: UntypedFormControl = new UntypedFormControl();

  public matches: OpenMatch[] = [];
  public isLoading: boolean = false;
  public isListing: boolean = false;
  public isRegistering: boolean = false;
  public isSubmitting: boolean = false;
  public registrationSuccessful: boolean = false;
  
  public displayedMatchColumns: string[];
  public displayedRegistrationColumns: string[];

  public registrationMatch!: OpenMatch;
  public registration!: OpenMatchRegistration;
  public selectedSeason!: number;
  public successfulRegistration!: OpenMatchRegistration;
  
  public registrations = new MatTableDataSource<OpenMatchRegistration>();
  public isLoadingRegistrations: boolean = false;

  constructor(
        public screenService: ScreenService,
        public openMatchService: OpenMatchService,
        private dialog: MatDialog
      ) 
  { 
    this.displayedMatchColumns = [];
    this.displayedRegistrationColumns = [];

    this.selectedSeason = 25;

    this.setDisplayedColumns(this.screenService.IsHandsetPortrait);
    
    screenService.OrientationChange.on(() => {
      this.setDisplayedColumns(screenService.IsHandsetPortrait);
    });

  }

  ngOnInit(): void {
    this.getMatches();
  }

  ngAfterViewInit(): void {
    this.registrations.sort = this.sort;
  }

  private getMatches() : void {
    this.isLoading = true;
    this.isRegistering = false;
    this.registrationSuccessful = false;
    this.openMatchService.readMatches(this.selectedSeason)
    .subscribe(data => {
      this.matches = data;
      // this.matches[2].inThePast = true;
      this.isLoading = false;
    });

  }

  private getRegistrations(matchId: string): void {
    this.isLoadingRegistrations = true;
    this.openMatchService.readRegistrations(this.selectedSeason)
    .subscribe(data => {
      this.isLoadingRegistrations = false;
      this.registrations.data = data.filter(m => m.openMatchId === matchId) as OpenMatchRegistration[];
    });

  }

  public register(match: OpenMatch) : void {
    this.isRegistering = true;
    this.registrationMatch = match;
    this.registration = new OpenMatchRegistration();
    this.registration.openMatchId = match.dbKey;
    this.registrationSuccessful = false;
    this.isSubmitting = false;

  }

  public viewRegistrations(match: OpenMatch) : void {
    this.isListing = true;
    this.isRegistering = false;
    this.registrationMatch = match;
    this.registrationSuccessful = false;
    this.isSubmitting = false;

    this.getRegistrations(match.dbKey);
  }

  public unregister(registration: OpenMatchRegistration) : void {

    const confirmDialog = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Please Confirm',
        message: `Are you sure you want to delete the registration for : <br/><b>${registration.name}</b>`
      }
    });
    confirmDialog.afterClosed().subscribe(result => {
      if (result === true) {
        this.openMatchService.deleteRegistration(registration.dbKey)
        .subscribe(data => {
          this.getMatches();
          this.getRegistrations(registration.openMatchId);
        });
          }
    });



  }


  public formComplete(): boolean {
    var valid = this.registration.name != null &&
      this.registration.name.trim() != "" && 
      this.registration.ageGroup != null &&
      this.registration.address != null &&
      this.registration.address.trim() != "" && 
      this.registration.parentName != null &&
      this.registration.parentName.trim() != "" && 
      this.registration.emergencyContactPhone != null &&
      this.registration.emergencyContactPhone.trim() != "";

    return valid;
  }

  public submit() : void {
    if (!this.registration.name || this.registration.name === '') {
      this.errorMessage = "Please enter the Angler's name"
      return;
    }
    this.isSubmitting = true;

    this.openMatchService.submitRegistration(this.registration)
      .pipe(map(res => res),
        catchError((error: HttpErrorResponse) => {
          this.isSubmitting = false;
          return throwError(error);
        }))
      .subscribe(data => {
        this.successfulRegistration = data;
        this.getMatches();
        this.registrationSuccessful = true;
        if (this.isListing) {
          this.getRegistrations(this.registration.openMatchId);
        }
    });

  }

  private setDisplayedColumns(handsetPortrait: boolean): void {
    
    var dcMatch = [
      "date",
      "draw",
      "start",
      "end",
      "pegsRemaining",
      "register"
    ];

    var dcReg = [
      "name",
      "ageGroupAsString",
      "contact"
    ];


    if (handsetPortrait) {
      this.displayedMatchColumns = dcMatch;
    } else {
      this.displayedMatchColumns = dcMatch;
      dcReg.push("address");
      dcReg.push("parent");
      dcReg.push("email");
    }

    dcReg.push("delete");

    this.displayedRegistrationColumns = dcReg;

  }

  public getDisplayedMatchColumns(): string[] {

    var dc = this.displayedMatchColumns;

    if (this.isAdmin && dc.indexOf("registrations") == -1 ) {
      dc.push("registrations");

      if (this.screenService.IsHandsetPortrait) {
        dc.forEach( (item, index) => {
          if(item === "draw") dc.splice(index,1);
        });

        dc.forEach( (item, index) => {
          if(item === "start") dc.splice(index,1);
        });

        dc.forEach( (item, index) => {
          if(item === "end") dc.splice(index,1);
        });
      }
    }

    this.displayedMatchColumns = dc;

    return this.displayedMatchColumns;
  }

  public getDisplayedRegistrationColumns(): string[] {
    return this.displayedRegistrationColumns;
  }

  public matchRegistrationSummary(): string {
    var upTo12 = this.registrations.data.filter(function (element) {
      return element.ageGroup == 0;
    }).length

    var thirteenTo18 = this.registrations.data.filter(function (element) {
      return element.ageGroup == 1;
    }).length

    var upTo12IsAre = upTo12 == 1 ? "is" : "are";
    var thirteenTo18IsAre = thirteenTo18 == 1 ? "is" : "are";

    return `Currently <b>${this.registrations.data.length}</b> registered; <b>${upTo12}</b> ${upTo12IsAre} up to 12 and <b>${thirteenTo18}</b> ${thirteenTo18IsAre} 13 to 18`;
  }
}
