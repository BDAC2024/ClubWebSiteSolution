import { Component, OnInit } from '@angular/core';
import { MatTabChangeEvent } from '@angular/material/tabs';
import { TrophyType } from 'src/app/models/match-enum';
import { RefData } from 'src/app/models/refData';
import { TrophyWinner } from 'src/app/models/trophy-winner';
import { GlobalService } from 'src/app/services/global.service';
import { MatchResultsService } from 'src/app/services/match-results.service';
import { MatchService } from 'src/app/services/match.service';
import { RefDataService } from 'src/app/services/ref-data.service';
import { ScreenService } from 'src/app/services/screen.service';

@Component({
  selector: 'app-trophy-winners',
  templateUrl: './trophy-winners.component.html',
  styleUrls: ['./trophy-winners.component.css']
})
export class TrophyWinnersComponent implements OnInit {

  public displayedColumns: string[];
  public isLoading: boolean = false;
  public trophyWinners: TrophyWinner[] = [];
  public refData!: RefData;
  public selectedSeason!: number;
  public selectedTrophyType: TrophyType = TrophyType.Senior;

  constructor(
    public matchResultsService: MatchResultsService,
    public matchService: MatchService,
    private refDataService: RefDataService,
    private globalService: GlobalService,
    public screenService: ScreenService
  ) { 
    this.displayedColumns = ["trophy", "winner", "weightpoints", "venue", "date"];

    screenService.OrientationChange.on(() => {
      this.setDisplayedColumns(screenService.IsHandsetPortrait);
    });
  }


  ngOnInit(): void {
    this.getRefData();
  }

  public tabChanged(tabChangeEvent: MatTabChangeEvent): void {

    switch (tabChangeEvent.index) {
      case 0:
        this.selectedTrophyType = TrophyType.Senior;
        break;
        
      case 1:
        this.selectedTrophyType = TrophyType.Junior;
        break;
        
    }

    this.loadTrophyWinners();
  }

  public loadTrophyWinners(): void
  {
    this.isLoading = true;
    this.trophyWinners = [];
    
    this.globalService.setStoredSeason(this.selectedSeason);
    this.matchResultsService.readTrophyWinners(this.selectedTrophyType, this.selectedSeason)
    .subscribe(data => {
      this.isLoading = false;
      this.trophyWinners = data;

    });
  }

  private getRefData() {
    this.refDataService.getRefData()
    .subscribe(data => {
      this.refData = data;
      this.selectedSeason = this.globalService.getStoredSeason(data.currentSeason);
      this.loadTrophyWinners();
    });
  }

  private setDisplayedColumns(isHandsetPortait: boolean) {

    if (isHandsetPortait) {
      this.displayedColumns = ["trophy", "winner", "weightpoints"];
    } else {
      this.displayedColumns = ["trophy", "winner", "weightpoints", "venue", "date"];
    }
  }
}
