import { DatePipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatTabChangeEvent } from '@angular/material/tabs';
import { CalendarExportDialogComponent } from 'src/app/dialogs/calendar-export-dialog/calendar-export-dialog.component';
import { MatchInfoComponent } from 'src/app/dialogs/match-info/match-info.component';
import { CalendarExport, CalendarExportDialogData } from 'src/app/models/calendar-export';
import { CalendarExportTypes } from 'src/app/models/calendar-export-types';
import { ClubEvent } from 'src/app/models/club-event';
import { DisplayedColumnsForEvents } from 'src/app/models/displayed-columns-events';
import { EventType } from 'src/app/models/event-enum';
import { RefData } from 'src/app/models/refData';
import { AuthenticationService } from 'src/app/services/auth/authentication.service';
import { ClubEventService } from 'src/app/services/club-event.service';
import { GlobalService } from 'src/app/services/global.service';
import { MembersService } from 'src/app/services/members.service';
import { RefDataService } from 'src/app/services/ref-data.service';
import { ScreenService } from 'src/app/services/screen.service';

const datepipe: DatePipe = new DatePipe('en-GB');

@Component({
  selector: 'app-diary',
  templateUrl: './diary.component.html',
  styleUrls: ['./diary.component.css']
})
export class DiaryComponent implements OnInit {

  private allEvents!: ClubEvent[];

  public events: ClubEvent[] = [];
  public displayedColumns: string[];
  public matchType: number = EventType.Match;
  public isLoading: boolean = false;
  public selectedSeason!: number;
  public refData!: RefData;
  public selectedEventType: EventType = EventType.All;

  constructor(
    public screenService: ScreenService,
    public clubEventService: ClubEventService,
    public refDataService: RefDataService,
    public dialog: MatDialog,
    public globalService: GlobalService,
    public authenticationService: AuthenticationService,
    public membersService: MembersService
  ) { 
    this.displayedColumns = [];

    screenService.OrientationChange.on(() => {
      globalService.log("diary - orientation has changed IsHandsetPortrait = " + screenService.IsHandsetPortrait);
      this.setDisplayedColumns(screenService.IsHandsetPortrait);
    });

  }

  ngOnInit(): void {
    this.isLoading = true;
    this.getRefData();

  }

  public tabChanged(tabChangeEvent: MatTabChangeEvent): void {
    this.selectedEventType = tabChangeEvent.index as EventType;
     this.loadEvents();
  }

  public showMore(match: ClubEvent)
  {
    const dialogRef = this.dialog.open(MatchInfoComponent, {maxHeight: "100vh", data: {match: match}});

    dialogRef.afterClosed().subscribe(result => {
    });
  }

  private loadEvents(): void
  {
    if (this.selectedEventType == EventType.All) {
      this.events = this.allEvents;
    } else {
      this.events = this.allEvents.filter(m => m.eventType === this.selectedEventType);
    }

    this.globalService.log("Matches loaded, portrait: " + this.screenService.IsHandsetPortrait);

    this.setDisplayedColumns(this.screenService.IsHandsetPortrait);

  }

  private setDisplayedColumns(handsetPortrait: boolean): void {
    this.globalService.log("Columns set, portrait: " + handsetPortrait);

    this.screenService.IsHandsetPortrait;

    var dc = new DisplayedColumnsForEvents();
    dc.displayedColumns;

    if (handsetPortrait) {
      dc.day[0] = false;
      dc.cup[0] = false;
      this.displayedColumns = dc.displayedColumns;
    } else {
      // If no club given then hide that column
      if (this.events && this.events.filter(m => m.cup === undefined).length == this.events.length)
      {
        dc.cup[0] = false;
        this.displayedColumns = dc.displayedColumns;
      }
      else 
      {
        this.displayedColumns = dc.displayedColumns;
      }

    }
  }

  public getRefData() {
    this.refDataService.getRefData()
    .subscribe(data => {
      this.refData = data;
      this.selectedSeason = this.globalService.getStoredSeason(data.currentSeason);
      this.getEvents();
    });
  }

  public getEvents() {
    this.globalService.setStoredSeason(this.selectedSeason);
    this.clubEventService.readEvents(this.selectedSeason)
    .subscribe(data => {
      this.isLoading = false;
      this.allEvents = data;
      this.loadEvents();
    });

  }

  public exportCalendar() {
    //var item: NewsItem = { dbKey: "", title: "", body: "", date: new Date() };
    var item: CalendarExportDialogData = { seasons: this.refData.seasons, season: this.selectedSeason, email: this.membersService.CurrentMember.email, calendarExportTypes: new CalendarExportTypes() };

    const dialogRef = this.dialog.open(CalendarExportDialogComponent, {
      width: this.screenService.IsHandset ? '90vw' : '60vw',
      data: item
    });

    dialogRef.afterClosed().subscribe(result => {
      console.log(`The dialog was closed : `);

      // if (result) {
      //   console.log("Exporting...");

      //   var dto : CalendarExport = {
      //     season: result.season,
      //     email: result.email,
      //     selectedCalendarExportTypes: []
      //   }

      //   if (result.calendarExportTypes.all) {
      //     dto.selectedCalendarExportTypes.push(0);
      //   }
      //   if (result.calendarExportTypes.allMatches) {
      //     dto.selectedCalendarExportTypes.push(1);
      //   }
      //   if (result.calendarExportTypes.meetings) {
      //     dto.selectedCalendarExportTypes.push(2);
      //   }
      //   if (result.calendarExportTypes.pondMatches) {
      //     dto.selectedCalendarExportTypes.push(3);
      //   }
      //   if (result.calendarExportTypes.riverMatches) {
      //     dto.selectedCalendarExportTypes.push(4);
      //   }

      //   this.clubEventService.exportCalendar(dto).subscribe();
      // }
    });
  }

  public isPreviewer(): boolean {
    if (this.refData != null) {
      return this.authenticationService.isPreviewer();
    } else {
      return false;
    }
  }
}
