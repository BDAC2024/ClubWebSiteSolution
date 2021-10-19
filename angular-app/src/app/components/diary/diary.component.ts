import { DatePipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatTabChangeEvent } from '@angular/material/tabs';
import { MatchInfoComponent } from 'src/app/dialogs/match-info/match-info.component';
import { ClubEvent } from 'src/app/models/club-event';
import { DisplayedColumnsForEvents } from 'src/app/models/displayed-columns-events';
import { EventType } from 'src/app/models/event-enum';
import { RefData } from 'src/app/models/refData';
import { ClubEventService } from 'src/app/services/club-event.service';
import { GlobalService } from 'src/app/services/global.service';
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
    public globalService: GlobalService
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
}
