import { Component, OnInit } from '@angular/core';
import { MatTabChangeEvent } from '@angular/material/tabs';
import { AggregateWeight } from 'src/app/models/aggregate-weight';
import { MatchType } from 'src/app/models/match-enum';
import { MatchResultsService } from 'src/app/services/match-results.service';
import { MatchService } from 'src/app/services/match.service';
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

  constructor(
    public matchResultsService: MatchResultsService,
    public matchService: MatchService,
    public screenService: ScreenService

  ) { 
    this.displayedColumns = ["position", "name", "weight"];
  }

  ngOnInit(): void {
    this.loadWeights(MatchType.Spring);

  }

  public tabChanged(tabChangeEvent: MatTabChangeEvent): void {
    this.loadWeights(tabChangeEvent.index as MatchType);
  }

  private loadWeights(type: MatchType): void
  {
    this.isLoading = true;
    this.weights = [];
    
    this.matchResultsService.readAggregateWeights(type)
    .subscribe(data => {
      this.isLoading = false;
      this.weights = data;
    });
  }

}
