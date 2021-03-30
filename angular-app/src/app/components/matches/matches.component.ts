import { Component, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Observable } from 'rxjs';
import { map, withLatestFrom } from 'rxjs/operators';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { MatchInfoComponent } from 'src/app/dialogs/match-info/match-info.component';
import { Match } from 'src/app/models/match';


const ELEMENT_DATA: Match[] = [
  {id: 100, number: 1, date: new  Date('2021-04-21'), day: '', venue: 'Cricket Field', cup: 'R. Potter'},
  {id: 102, number: 2, date: new  Date('2021-05-02'), day: '', venue: 'Ellenthorpe Middle Lane', cup: 'Lawson Tancred'},
  {id: 104, number: 3, date: new  Date('2021-05-16'), day: '', venue: 'Ings Lane Bottom', cup: 'Bradley'},
  {id: 108, number: 4, date: new  Date('2021-06-30'), day: '', venue: 'Roecliffe Lake', cup: 'Whitbread'},
];

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

  constructor(
    private breakpointObserver: BreakpointObserver,
    public dialog: MatDialog,
    router: Router) {

    const datepipe: DatePipe = new DatePipe('en-GB');

    this.matches.forEach(element => {
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

  public displayedColumns: string[];
  matches = ELEMENT_DATA;

  public showMore(match: Match)
  {
    console.log("You chose match: " + match.id);
    const dialogRef = this.dialog.open(MatchInfoComponent, {data: {match: match}});

    dialogRef.afterClosed().subscribe(result => {
      console.log(`Dialog result: ${result}`);
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
