import { Component, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Observable } from 'rxjs';
import { map, withLatestFrom } from 'rxjs/operators';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { MatchInfoComponent } from 'src/app/dialogs/match-info/match-info.component';
import { ClubEvent } from 'src/app/models/club-event';
import { MatchService } from 'src/app/services/match.service';
import { MatchType } from 'src/app/models/enums';
import { MatTabChangeEvent } from '@angular/material/tabs';
import { ScreenService } from 'src/app/services/screen.service';

const datepipe: DatePipe = new DatePipe('en-GB');

@Component({
  selector: 'app-matches',
  templateUrl: './matches.component.html',
  styleUrls: ['./matches.component.css']
})
export class MatchesComponent implements OnInit {

  // Change column settings if portrait occurs
  portraitLayoutChanges = this.breakpointObserver.observe(Breakpoints.HandsetPortrait)
  .subscribe(result => {
    this.setDisplayedColumns(result.matches);
  });

  // Change column settings if landscape occurs
  landscapeLayoutChanges = this.breakpointObserver.observe(Breakpoints.HandsetLandscape)
  .subscribe(result => {
    this.screenService.IsHandsetLandscape = result.matches;
  });

  public displayedColumns: string[];
  matches!: ClubEvent[];
  
  constructor(
    private matchService: MatchService,
    public screenService: ScreenService,
    private breakpointObserver: BreakpointObserver,
    public dialog: MatDialog,
    router: Router) {

    this.displayedColumns = [];

    console.log("1.1 constructor running");
  }

  ngOnInit(): void {
    console.log("ngOnInit running");

    this.loadMatches(0 as MatchType);
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
    var typeMatches = this.matchService.GetMatches(type);
    typeMatches.forEach(element => {
      var formatted = datepipe.transform(element.date, 'E');
      element.day = formatted == undefined ? '' : formatted;
    });

    this.matches = typeMatches;

    console.log("Matches loaded, portrait: " + this.screenService.IsHandsetPortrait);

    this.setDisplayedColumns(this.screenService.IsHandsetPortrait);

  }

  private setDisplayedColumns(handsetPortrait: boolean): void {
    console.log("Columns set, portrait: " + handsetPortrait);

    this.screenService.IsHandsetPortrait;

    if (handsetPortrait) {
      this.displayedColumns = ['date', 'description', 'number', 'more'];
    } else {
      // If no club given then hide that column
      if (this.matches && this.matches.filter(m => m.cup === undefined).length == this.matches.length)
      {
        this.displayedColumns = ['date', 'day', 'description', 'number', 'more'];
      }
      else 
      {
        this.displayedColumns = ['date', 'day', 'description', 'cup', 'number', 'more'];
      }

    }
  }

}
