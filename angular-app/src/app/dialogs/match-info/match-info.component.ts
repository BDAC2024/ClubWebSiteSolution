import { Component, OnInit, Output, EventEmitter, Inject } from '@angular/core';
import { MatchParam, MatchResult } from 'src/app/models/match';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ClubEvent } from 'src/app/models/club-event';
import { MatchType } from 'src/app/models/match-enum';
import { ScreenService } from 'src/app/services/screen.service';
import { MatchResultsService } from 'src/app/services/match-results.service';


@Component({
  selector: 'app-match-info',
  templateUrl: './match-info.component.html',
  styleUrls: ['./match-info.component.css']
})
export class MatchInfoComponent implements OnInit {

  public matchType: string = "";
  public match!: ClubEvent;
  public displayedColumns: string[] = ["pos", "name", "peg", "weight", "pts"];
  public results: MatchResult[] = [];
  public isLoading: boolean = false;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: MatchParam,
    public screenService: ScreenService,
    private matchResultsService: MatchResultsService
  ) {

   }

  ngOnInit(): void {
    this.match = this.data.match;
    this.matchType = MatchType.FullName(this.match.matchType as MatchType);
    this.isLoading = true;

    this.matchResultsService.readResults(this.match.id)
    .subscribe(data => {
      this.results = data;
      this.isLoading = false;
      // If no points are used then hide that column
      if (this.results.filter(m => m.points === undefined).length == this.results.length)
      {
        this.displayedColumns = ["pos", "name", "peg", "weight"];
      }
    });

  }

}
