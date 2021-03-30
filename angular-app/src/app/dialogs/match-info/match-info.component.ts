import { Component, OnInit, Output, EventEmitter, Inject } from '@angular/core';
import { Match, MatchParam } from 'src/app/models/match';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';

@Component({
  selector: 'app-match-info',
  templateUrl: './match-info.component.html',
  styleUrls: ['./match-info.component.css']
})
export class MatchInfoComponent implements OnInit {

  public match!: Match;

  constructor(@Inject(MAT_DIALOG_DATA) public data: MatchParam) { }

  ngOnInit(): void {
    this.match = this.data.match;

    console.log(this.match.venue);
  }

}
