import { Component, OnInit, Output, EventEmitter, Inject } from '@angular/core';
import { Match, MatchParam, MatchResult } from 'src/app/models/match';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';

const ELEMENT_DATA: MatchResult[] = [
  {matchId: 90, name: "G.Tilburn", weight: "3lb 7oz", points: 12},
  {matchId: 90, name: "G.Lumsden", weight: "3lb 2oz", points: 10},
  {matchId: 90, name: "K.Adcock", weight: "2lb 6oz", points: 8},
  {matchId: 90, name: "S.Townend", weight: "1lb 5oz", points: 6},
  {matchId: 90, name: "P.Ingledew", weight: "1lb 2oz", points: 4},

  {matchId: 100, name: "G.Tilburn", weight: "3lb 7oz", points: 12},
  {matchId: 100, name: "G.Lumsden", weight: "3lb 2oz", points: 10},
  {matchId: 100, name: "K.Adcock", weight: "2lb 6oz", points: 8},
  {matchId: 100, name: "S.Townend", weight: "1lb 5oz", points: 6},
  {matchId: 100, name: "P.Ingledew", weight: "1lb 2oz", points: 4},
  {matchId: 100, name: "L.Caygill", weight: "1lb 1oz", points: 2},
  {matchId: 100, name: "A.Scaife", weight: "6oz", points: 1},

  {matchId: 102, name: "G.Tilburn", weight: "3lb 7oz", points: 12},
  {matchId: 102, name: "G.Lumsden", weight: "3lb 2oz", points: 10},
  {matchId: 102, name: "K.Adcock", weight: "2lb 6oz", points: 8},

  {matchId: 104, name: "G.Lumsden", weight: "7lb 7oz", points: 12},
  {matchId: 104, name: "G.Tilburn", weight: "2lb 2oz", points: 10},
  {matchId: 104, name: "K.Adcock", weight: "4lb 9oz", points: 8},

  {matchId: 200, name: "A.Kid", weight: "3lb 7oz", points: 12},
  {matchId: 200, name: "L.Rascal", weight: "3lb 2oz", points: 10},
  {matchId: 200, name: "T.Iny", weight: "2lb 6oz", points: 8},

  {matchId: 400, name: "K.Adcock", weight: "4lb 0oz"},
  {matchId: 400, name: "S.Townend", weight: "2lb 2oz"},
  {matchId: 400, name: "G.Tilburn", weight: "7oz"},
  
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

    this.results = this.results.filter(m => m.matchId === this.match.id);

    // If no points are used then hide that column
    if (this.results.filter(m => m.points === undefined).length == this.results.length)
    {
      this.displayedColumns = ["name", "weight"];
    }
      
  }

}
