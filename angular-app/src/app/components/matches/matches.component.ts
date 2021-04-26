import { Component, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { MatDialog } from '@angular/material/dialog';
import { MatchInfoComponent } from 'src/app/dialogs/match-info/match-info.component';
import { ClubEvent } from 'src/app/models/club-event';
import { MatchService } from 'src/app/services/match.service';
import { MatchType } from 'src/app/models/match-enum';
import { MatTabChangeEvent } from '@angular/material/tabs';
import { ScreenService } from 'src/app/services/screen.service';
import { GlobalService } from 'src/app/services/global.service';
import { DisplayedColumnsForMatches } from 'src/app/models/displayed-columns-matches';
import { ClubEventService } from 'src/app/services/club-event.service';

const datepipe: DatePipe = new DatePipe('en-GB');

@Component({
  selector: 'app-matches',
  templateUrl: './matches.component.html',
  styleUrls: ['./matches.component.css']
})
export class MatchesComponent implements OnInit {

  public displayedColumns: string[];
  public matches: ClubEvent[] = [];
  public allMatches!: ClubEvent[];
  public isLoading: boolean = false;
  
  constructor(
    public matchService: MatchService,
    public clubEventService: ClubEventService,
    public screenService: ScreenService,
    public dialog: MatDialog,
    public globalService: GlobalService) {

    this.displayedColumns = [];

    screenService.OrientationChange.on(() => {
      this.globalService.log("matches - orientation has changed IsHandsetPortrait = " + screenService.IsHandsetPortrait);
      this.setDisplayedColumns(screenService.IsHandsetPortrait);
    });

    this.globalService.log("1.1 constructor running");
  }

  ngOnInit(): void {
    this.globalService.log("ngOnInit running");
    this.isLoading = true;

    this.clubEventService.readEvents()
    .subscribe(data => {
      this.isLoading = false;
      this.allMatches = data;
      this.loadMatches(0 as MatchType);
    });
  }

  public showMore(match: ClubEvent)
  {
    const dialogRef = this.dialog.open(MatchInfoComponent, {maxHeight: "100vh", data: {match: match}});

    dialogRef.afterClosed().subscribe(result => {
    });
  }

  public tabChanged(tabChangeEvent: MatTabChangeEvent): void {
    this.loadMatches(tabChangeEvent.index as MatchType);
  }

  private loadMatches(type: MatchType): void
  {
      
    this.matches = this.allMatches.filter(m => m.matchType === type);

    this.globalService.log("Matches loaded, portrait: " + this.screenService.IsHandsetPortrait);

    this.setDisplayedColumns(this.screenService.IsHandsetPortrait);
  }

  private setDisplayedColumns(handsetPortrait: boolean): void {
    this.globalService.log("Columns set, portrait: " + handsetPortrait);

    this.screenService.IsHandsetPortrait;
    var dc = new DisplayedColumnsForMatches();

    if (handsetPortrait) {
      dc.day[0] = false;
      dc.cup[0] = false;
      this.displayedColumns = dc.displayedColumns;
    } else {
      // If no club given then hide that column
      if (this.matches && this.matches.filter(m => m.cup === undefined).length == this.matches.length)
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
