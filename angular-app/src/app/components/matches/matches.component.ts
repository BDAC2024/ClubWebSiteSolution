import { Component, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Observable } from 'rxjs';
import { map, withLatestFrom } from 'rxjs/operators';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { MatchInfoComponent } from 'src/app/dialogs/match-info/match-info.component';
import { Match } from 'src/app/models/match';
import { MatchService } from 'src/app/services/match.service';
import { MatchType } from 'src/app/models/matchTypeEnum';

@Component({
  selector: 'app-matches',
  templateUrl: './matches.component.html',
  styleUrls: ['./matches.component.css']
})
export class MatchesComponent implements OnInit {

  isHandsetPortrait$: Observable<boolean> = this.breakpointObserver.observe(Breakpoints.HandsetPortrait)
  .pipe(
    map(result => result.matches)
  );

  // Change column settings if now portrait handset
  layoutChanges = this.breakpointObserver.observe(Breakpoints.HandsetPortrait)
  .subscribe(result => {
    this.setDisplayedColumns(result.matches);
  });

  public displayedColumns: string[];
  springMatches: Match[];
  clubMatches: Match[];
  jrMatches: Match[];
  osuMatches: Match[];

  constructor(
    private matchService: MatchService,
    private breakpointObserver: BreakpointObserver,
    public dialog: MatDialog,
    router: Router) {

    const datepipe: DatePipe = new DatePipe('en-GB');

    this.springMatches = matchService.GetMatches(MatchType.Spring);
    this.springMatches.forEach(element => {
      var formatted = datepipe.transform(element.date, 'E');
      element.day = formatted == undefined ? '' : formatted;
    });

    this.clubMatches = matchService.GetMatches(MatchType.Club);
    this.clubMatches.forEach(element => {
      var formatted = datepipe.transform(element.date, 'E');
      element.day = formatted == undefined ? '' : formatted;
    });

    this.jrMatches = matchService.GetMatches(MatchType.Junior);
    this.jrMatches.forEach(element => {
      var formatted = datepipe.transform(element.date, 'E');
      element.day = formatted == undefined ? '' : formatted;
    });

    this.osuMatches = matchService.GetMatches(MatchType.OSU);
    this.osuMatches.forEach(element => {
      var formatted = datepipe.transform(element.date, 'E');
      element.day = formatted == undefined ? '' : formatted;
    });

    this.displayedColumns = [];

    // Initial column settings
    router.events.pipe(
      withLatestFrom(this.isHandsetPortrait$)
    ).subscribe(result => {
      this.setDisplayedColumns(result[1]);
    });

  }

  ngOnInit(): void {
  }

  public showMore(match: Match)
  {
    const dialogRef = this.dialog.open(MatchInfoComponent, {maxHeight: "100vh", data: {match: match}});

    dialogRef.afterClosed().subscribe(result => {
    });
  }

  private setDisplayedColumns(handsetPortrait: boolean): void {
    if (handsetPortrait) {
      this.displayedColumns = ['date', 'venue', 'number', 'more'];
    } else {
      this.displayedColumns = ['date', 'day', 'venue', 'cup', 'number', 'more'];
    }
  }


}
