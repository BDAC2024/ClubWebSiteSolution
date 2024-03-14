import { Component, OnInit } from '@angular/core';
import { MatchResultsService } from 'src/app/services/match-results.service';
import { ScreenService } from 'src/app/services/screen.service';
import { MatTabChangeEvent } from '@angular/material/tabs';
import { AggregateType, MatchType } from 'src/app/models/match-enum';
import { LeagueStanding } from 'src/app/models/league-standing';
import { MatchService } from 'src/app/services/match.service';
import { GlobalService } from 'src/app/services/global.service';
import { RefDataService } from 'src/app/services/ref-data.service';
import { RefData, Season } from 'src/app/models/refData';

@Component({
  selector: 'app-league-standings',
  templateUrl: './league-standings.component.html',
  styleUrls: ['./league-standings.component.css']
})
export class LeagueStandingsComponent implements OnInit {

  public displayedColumns: string[];
  public isLoading: boolean = false;
  public standings: LeagueStanding[] = [];
  public refData!: RefData;
  public selectedSeason!: number;
  public selectedAggregateType: AggregateType = AggregateType.Spring;

  constructor(
    public matchResultsService: MatchResultsService,
    public matchService: MatchService,
    private refDataService: RefDataService,
    private globalService: GlobalService,
    public screenService: ScreenService
  ) { 
    this.displayedColumns = ["position", "name", "points"];
  }

  ngOnInit(): void {
    this.getRefData();

  }

  public tabChanged(tabChangeEvent: MatTabChangeEvent): void {

    switch (tabChangeEvent.index) {
      case 0:
        this.selectedAggregateType = AggregateType.Spring;
        break;
        
      case 1:
        this.selectedAggregateType = AggregateType.ClubRiver;
        break;
        
      case 2:
        this.selectedAggregateType = AggregateType.ClubPond;
        break;
        
      case 3:
        this.selectedAggregateType = AggregateType.Junior;
        break;

      case 4:
        this.selectedAggregateType = AggregateType.OSU;
        break;

      case 5:
        this.selectedAggregateType = AggregateType.Evening;
        break;
    }

    this.loadLeague();
  }

  public loadLeague(): void
  {
    this.isLoading = true;
    this.standings = [];
    
    this.globalService.setStoredSeason(this.selectedSeason);
    this.matchResultsService.readLeagueStandings(this.selectedAggregateType, this.selectedSeason)
    .subscribe(data => {
      this.isLoading = false;
      this.standings = data;

    });
  }

  private getRefData() {
    this.refDataService.getRefData()
    .subscribe(data => {
      this.refData = data;
      this.selectedSeason = this.globalService.getStoredSeason(data.currentSeason);
      this.loadLeague();
    });
  }

}
