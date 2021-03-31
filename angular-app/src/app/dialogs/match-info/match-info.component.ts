import { Component, OnInit, Output, EventEmitter, Inject } from '@angular/core';
import { Match, MatchParam, MatchResult } from 'src/app/models/match';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';

const ELEMENT_DATA: MatchResult[] = [
  {matchId: 100, name: "G.Tilburn", weight: "3lb 7oz", points: 12},
  {matchId: 100, name: "G.Lumsden", weight: "3lb 2oz", points: 10},
  {matchId: 100, name: "K.Adcock", weight: "2lb 6oz", points: 8},
  {matchId: 100, name: "S.Townend", weight: "1lb 5oz", points: 6},
  {matchId: 100, name: "P.Ingledew", weight: "1lb 2oz", points: 4},
  {matchId: 100, name: "L.Caygill", weight: "1lb 1oz", points: 2},
  {matchId: 100, name: "A.Scaife", weight: "6oz", points: 1},

  {matchId: 102, name: "K.Adcock", weight: "4lb 0oz", points: 12},
  {matchId: 102, name: "S.Townend", weight: "2lb 2oz", points: 10},
  {matchId: 102, name: "G.Tilburn", weight: "7oz", points: 8},

  {matchId: 104, name: "G.Lumsden", weight: "7lb 7oz", points: 12},
  {matchId: 104, name: "G.Tilburn", weight: "2lb 2oz", points: 10},
  {matchId: 104, name: "K.Adcock", weight: "4lb 9oz", points: 8},
];

@Component({
  selector: 'app-match-info',
  templateUrl: './match-info.component.html',
  styleUrls: ['./match-info.component.css']
})
export class MatchInfoComponent implements OnInit {

  public match!: Match;
  results = ELEMENT_DATA;
  public displayedColumns: string[] = ["name", "weight", "points"];

  constructor(@Inject(MAT_DIALOG_DATA) public data: MatchParam) { }

  ngOnInit(): void {
    this.match = this.data.match;

    this.results = this.results.filter(
      m => m.matchId === this.match.id);

    console.log(this.match.venue);
  }

}
