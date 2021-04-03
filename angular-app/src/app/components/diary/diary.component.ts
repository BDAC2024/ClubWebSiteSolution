import { DatePipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatTabChangeEvent } from '@angular/material/tabs';
import { MatchInfoComponent } from 'src/app/dialogs/match-info/match-info.component';
import { ClubEvent } from 'src/app/models/club-event';
import { EventType, MatchType } from 'src/app/models/enums';
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
    var typeEvents: ClubEvent[] = [];

    this.clubEventService.GetEvents(type).forEach(val => typeEvents.push(Object.assign({}, val)));

    typeEvents.forEach(element => {
      var formatted = datepipe.transform(element.date, 'E');
      element.day = formatted == undefined ? '' : formatted;

      if (type == EventType.All || type == EventType.Match) {

        if (element.eventType == EventType.Work) {
          element.description = "Work Party - " + element.description;
        }

        if (element.eventType == EventType.Match) {
          var matchType: string = "";

          switch(element.matchType)
          {
            case MatchType.Spring:
              matchType = "Spring";
              break;
            case MatchType.Officials:
              matchType = "Officials";
              break;
            case MatchType.OSU:
              matchType = "O.S.U";
              break;
            case MatchType.Junior:
              matchType = "Junior";
              break;
            case MatchType.Club:
              matchType = "Club";
              break;
          }

          element.description = matchType + " - " + element.description;
        }
      }
    });

    this.events = typeEvents;

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
        this.displayedColumns = ['date', 'time', 'day', 'description', 'number', 'more'];
      }
      else 
      {
        this.displayedColumns = ['date', 'time', 'day', 'description', 'cup', 'number', 'more'];
      }

    }
  }

}
