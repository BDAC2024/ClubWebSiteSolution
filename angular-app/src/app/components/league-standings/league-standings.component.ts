import { Component, OnInit } from '@angular/core';
import { MatchResultsService } from 'src/app/services/match-results.service';
import { ScreenService } from 'src/app/services/screen.service';
import { MatTabChangeEvent } from '@angular/material/tabs';
import { MatchType } from 'src/app/models/match-enum';
import { LeagueStanding } from 'src/app/models/league-standing';
import { MatchService } from 'src/app/services/match.service';
import { GlobalService } from 'src/app/services/global.service';

@Component({
  selector: 'app-league-standings',
  templateUrl: './league-standings.component.html',
  styleUrls: ['./league-standings.component.css']
})
export class LeagueStandingsComponent implements OnInit {

  public displayedColumns: string[];
  public isLoading: boolean = false;
  public standings: LeagueStanding[] = [];

  constructor(
    public matchResultsService: MatchResultsService,
    public matchService: MatchService,
    public screenService: ScreenService
  ) { 
    this.displayedColumns = ["position", "name", "points", "weight"];
  }

  ngOnInit(): void {
    this.loadLeague(MatchType.Spring);

  }

  public tabChanged(tabChangeEvent: MatTabChangeEvent): void {
    this.loadLeague(tabChangeEvent.index as MatchType);
  }

  private loadLeague(type: MatchType): void
  {
    this.isLoading = true;
    this.standings = [];
    
    this.matchResultsService.readLeagueStandings(type)
    .subscribe(data => {
      this.isLoading = false;
      this.standings = data;
    });
  }

}
