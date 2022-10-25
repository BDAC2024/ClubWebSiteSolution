import { Component, OnInit } from '@angular/core';
import { MatTabChangeEvent } from '@angular/material/tabs';
import { AggregateWeight } from 'src/app/models/aggregate-weight';
import { AggregateWeightType, MatchType } from 'src/app/models/match-enum';
import { RefData } from 'src/app/models/refData';
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

  public displayedColumns: string[];
  public isLoading: boolean = false;
  public weights: AggregateWeight[] = [];
  public refData!: RefData;
  public selectedSeason!: number;
  public selectedAggregateWeightType: AggregateWeightType = AggregateWeightType.Spring;

  constructor(
    public matchResultsService: MatchResultsService,
    public matchService: MatchService,
    private refDataService: RefDataService,
    private globalService: GlobalService,
    public screenService: ScreenService

  ) { 
    this.displayedColumns = ["position", "name", "weight"];
  }

  ngOnInit(): void {
    this.isLoading = true;
    this.getRefData();

  }

  public tabChanged(tabChangeEvent: MatTabChangeEvent): void {

    switch (tabChangeEvent.index) {
      case 0:
        this.selectedAggregateWeightType = AggregateWeightType.Spring;
        break;
        
      case 1:
        this.selectedAggregateWeightType = AggregateWeightType.ClubRiver;
        break;
        
      case 2:
        this.selectedAggregateWeightType = AggregateWeightType.ClubPond;
        break;

      case 3:
        this.selectedAggregateWeightType = AggregateWeightType.Pairs;
        break;
    }

    this.loadWeights();
  }

  public loadWeights(): void
  {
    this.isLoading = true;
    this.weights = [];
    
    this.globalService.setStoredSeason(this.selectedSeason);
    
    this.matchResultsService.readAggregateWeights(this.selectedAggregateWeightType, this.selectedSeason)
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
      this.loadWeights();
    });
  }

}
