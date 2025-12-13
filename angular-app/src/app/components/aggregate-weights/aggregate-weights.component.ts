import { Component, OnInit } from '@angular/core';
import { MatTabChangeEvent } from '@angular/material/tabs';
import { AggregateWeight } from 'src/app/models/aggregate-weight';
import { ClubEvent } from 'src/app/models/club-event';
import { AggregateType, MatchType } from 'src/app/models/match-enum';
import { RefData } from 'src/app/models/refData';
import { ClubEventService } from 'src/app/services/club-event.service';
import { GlobalService } from 'src/app/services/global.service';
import { MatchResultsService } from 'src/app/services/match-results.service';
import { MatchService } from 'src/app/services/match.service';
import { RefDataService } from 'src/app/services/ref-data.service';
import { ScreenService } from 'src/app/services/screen.service';

@Component({
  selector: 'app-aggregate-weights',
  templateUrl: './aggregate-weights.component.html',
  styleUrls: ['./aggregate-weights.component.css']
})
export class AggregateWeightsComponent implements OnInit {

  public allMatches: ClubEvent[] = [];
  public displayedColumns: string[];
  public isLoading: boolean = false;
  public weights: AggregateWeight[] = [];
  public refData!: RefData;
  public selectedSeason!: number;
  public selectedAggregateType: AggregateType = AggregateType.Spring;

  constructor(
    public matchResultsService: MatchResultsService,
    public matchService: MatchService,
    private refDataService: RefDataService,
    private globalService: GlobalService,
    public screenService: ScreenService,
    public clubEventService: ClubEventService

  ) { 
    this.displayedColumns = ["position", "name", "weight"];
  }

  ngOnInit(): void {
    this.isLoading = true;
    this.getRefData();

  }

  public anyOfType(aggType: number): boolean {
    return this.allMatches.filter(m => m.aggregateType === aggType).length > 0;
  }

  public tabChanged(tabChangeEvent: MatTabChangeEvent): void {

    this.selectedAggregateType = parseInt(tabChangeEvent.tab.ariaLabel) as AggregateType;
    
    this.loadWeights();
  }

  public loadWeights(): void
  {
    this.isLoading = true;
    this.weights = [];
    
    this.globalService.setStoredSeason(this.selectedSeason);
    
    this.matchResultsService.readAggregateWeights(this.selectedAggregateType, this.selectedSeason)
    .subscribe(data => {
      this.isLoading = false;
      this.weights = data;
    });
  }

  private getRefData() {
    this.refDataService.getRefData()
    .subscribe(data => {
      this.refData = data;
      this.selectedSeason = this.globalService.getStoredSeason(data.currentSeason);
      this.getMatches();
    });
  }

  public getMatches() {
    this.isLoading = true;
    this.globalService.setStoredSeason(this.selectedSeason);
    this.clubEventService.readEvents(this.selectedSeason)
    .subscribe(data => {
      this.isLoading = false;
      this.allMatches = data;
      this.loadWeights();
    });
  }  
}
