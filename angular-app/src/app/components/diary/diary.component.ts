import { DatePipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatTabChangeEvent } from '@angular/material/tabs';
import { MatchInfoComponent } from 'src/app/dialogs/match-info/match-info.component';
import { ClubEvent } from 'src/app/models/club-event';
import { EventType } from 'src/app/models/event-enum';
import { MatchType } from 'src/app/models/match-enum';
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

  public events!: ClubEvent[];
  public displayedColumns: string[];
  public matchType: number = EventType.Match;
  
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
    this.loadEvents(0 as EventType);

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
    this.events = this.clubEventService.GetEvents(type);

    this.globalService.log("Matches loaded, portrait: " + this.screenService.IsHandsetPortrait);

    this.setDisplayedColumns(this.screenService.IsHandsetPortrait);

  }

  private setDisplayedColumns(handsetPortrait: boolean): void {
    this.globalService.log("Columns set, portrait: " + handsetPortrait);

    this.screenService.IsHandsetPortrait;

    if (handsetPortrait) {
      this.displayedColumns = ['date', 'time', 'description', 'number', 'more'];
    } else {
      // If no club given then hide that column
      if (this.events && this.events.filter(m => m.cup === undefined).length == this.events.length)
      {
        this.displayedColumns = [ 'day', 'date', 'time','description', 'number', 'more'];
      }
      else 
      {
        this.displayedColumns = ['day', 'date', 'time', 'description', 'cup', 'number', 'more'];
      }

    }
  }

}
