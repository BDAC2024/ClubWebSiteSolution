import { DatePipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatTabChangeEvent } from '@angular/material/tabs';
import { MatchInfoComponent } from 'src/app/dialogs/match-info/match-info.component';
import { ClubEvent } from 'src/app/models/club-event';
import { DisplayedColumnsForEvents } from 'src/app/models/displayed-columns-events';
import { EventType } from 'src/app/models/event-enum';
import { ClubEventService } from 'src/app/services/club-event.service';
import { GlobalService } from 'src/app/services/global.service';
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

  constructor(
    public screenService: ScreenService,
    public clubEventService: ClubEventService,
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

    this.clubEventService.readEvents()
    .subscribe(data => {
      this.isLoading = false;
      this.allEvents = data;
      this.loadEvents(0 as EventType);
    });

  }

  public tabChanged(tabChangeEvent: MatTabChangeEvent): void {
     this.loadEvents(tabChangeEvent.index as EventType);
  }

  public showMore(match: ClubEvent)
  {
    const dialogRef = this.dialog.open(MatchInfoComponent, {maxHeight: "100vh", data: {match: match}});

    dialogRef.afterClosed().subscribe(result => {
    });
  }

  private loadEvents(type: EventType): void
  {
    if (type == EventType.All) {
      this.events = this.allEvents;
    } else {
      this.events = this.allEvents.filter(m => m.eventType === type);
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

}
