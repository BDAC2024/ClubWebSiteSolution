import { Component, OnInit, Output, EventEmitter, Inject } from '@angular/core';
import { MatchParam, MatchResult } from 'src/app/models/match';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ClubEvent } from 'src/app/models/club-event';
import { MatchType } from 'src/app/models/match-enum';
import { ScreenService } from 'src/app/services/screen.service';

const ELEMENT_DATA: MatchResult[] = [
  {matchId: 2, name: "G.Tilburn", weight: "3lb 7oz", peg: "5", points: 12},
  {matchId: 2, name: "G.Lumsden", weight: "3lb 2oz", peg: "9", points: 10},
  {matchId: 2, name: "K.Adcock", weight: "2lb 6oz", peg: "1", points: 8},
  {matchId: 2, name: "S.Townend", weight: "1lb 5oz", peg: "6", points: 6},
  {matchId: 2, name: "P.Ingledew", weight: "1lb 2oz", peg: "2", points: 4},

  {matchId: 15, name: "G.Tilburn", weight: "3lb 7oz", peg: "4", points: 12},
  {matchId: 15, name: "G.Lumsden", weight: "3lb 2oz", peg: "11", points: 10},
  {matchId: 15, name: "K.Adcock", weight: "2lb 6oz", peg: "10", points: 8},
  {matchId: 15, name: "S.Townend", weight: "1lb 5oz", peg: "8", points: 6},
  {matchId: 15, name: "P.Ingledew", weight: "1lb 2oz", peg: "3", points: 4},
  {matchId: 15, name: "L.Caygill", weight: "1lb 1oz", peg: "5", points: 2},
  {matchId: 15, name: "A.Scaife", weight: "6oz", peg: "2", points: 1},

  {matchId: 18, name: "G.Tilburn", weight: "3lb 7oz", peg: "12", points: 12},
  {matchId: 18, name: "G.Lumsden", weight: "3lb 2oz", peg: "22", points: 10},
  {matchId: 18, name: "K.Adcock", weight: "2lb 6oz", peg: "2", points: 8},

  {matchId: 21, name: "G.Lumsden", weight: "7lb 7oz", peg: "8", points: 12},
  {matchId: 21, name: "G.Tilburn", weight: "2lb 2oz", peg: "6", points: 10},
  {matchId: 21, name: "K.Adcock", weight: "4lb 9oz", peg: "2", points: 8},

  {matchId: 10, name: "A.Kid", weight: "3lb 7oz", peg: "1", points: 12},
  {matchId: 10, name: "L.Rascal", weight: "3lb 2oz", peg: "9", points: 10},
  {matchId: 10, name: "T.Iny", weight: "2lb 6oz", peg: "5", points: 8},

  {matchId: 16, name: "K.Adcock", weight: "4lb 0oz", peg: "22"},
  {matchId: 16, name: "S.Townend", weight: "2lb 2oz", peg: "10"},
  {matchId: 16, name: "G.Tilburn", weight: "7oz", peg: "4"},
  
];

@Component({
  selector: 'app-match-info',
  templateUrl: './match-info.component.html',
  styleUrls: ['./match-info.component.css']
})
export class MatchInfoComponent implements OnInit {

  public matchType: string = "";
  public match!: ClubEvent;
  results = ELEMENT_DATA;
  public displayedColumns: string[] = ["name", "peg", "weight", "points"];

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: MatchParam,
    public screenService: ScreenService
  ) {

   }

  ngOnInit(): void {
    this.match = this.data.match;
    this.matchType = MatchType.FullName(this.match.matchType as MatchType);

    this.results = this.results.filter(m => m.matchId === this.match.id);

    // If no points are used then hide that column
    if (this.results.filter(m => m.points === undefined).length == this.results.length)
    {
      this.displayedColumns = ["name", "peg", "weight"];
    }
      
  }

}
