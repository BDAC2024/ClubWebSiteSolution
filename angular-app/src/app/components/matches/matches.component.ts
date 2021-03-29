import { Component, OnInit } from '@angular/core';

export interface Match {
  date: Date;
  venue: string;
  // position: number;
  // weight: number;
  // symbol: string;
}

const ELEMENT_DATA: Match[] = [
  {date: new  Date('2021-04-21') , venue: 'Cricket Field'},
  {date: new  Date('2021-05-02') , venue: 'Ellenthorpe Hall'},
  {date: new  Date('2021-05-16') , venue: 'Ings Lane Bottom'},
  {date: new  Date('2021-06-30') , venue: 'Roecliffe Lake'},
];

@Component({
  selector: 'app-matches',
  templateUrl: './matches.component.html',
  styleUrls: ['./matches.component.css']
})
export class MatchesComponent implements OnInit {

  constructor() { }

  ngOnInit(): void {
  }

  displayedColumns: string[] = ['date', 'venue'];
  matches = ELEMENT_DATA;
}
