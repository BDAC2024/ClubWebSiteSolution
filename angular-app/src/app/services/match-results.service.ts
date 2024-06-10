import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { plainToClass } from 'class-transformer';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { AggregateWeight } from '../models/aggregate-weight';
import { LeagueStanding } from '../models/league-standing';
import { MatchResult } from '../models/match';
import { AggregateType, TrophyType } from '../models/match-enum';
import { Season } from '../models/refData';
import { GlobalService } from './global.service';
import { TrophyWinner } from '../models/trophy-winner';

@Injectable({
  providedIn: 'root'
})
export class MatchResultsService {

  constructor(
    private http: HttpClient, 
    private globalService: GlobalService) {

 }

 public readResults(matchId: string): Observable<MatchResult[]> {
  return this.http.get<MatchResult[]>(`${this.globalService.ApiUrl}/api/matchResults/${matchId}`)
            .pipe(map(res => 
              plainToClass(MatchResult, res)
          ));
}

public readLeagueStandings(type: AggregateType, season: number): Observable<LeagueStanding[]> {
  return this.http.get<MatchResult[]>(`${this.globalService.ApiUrl}/api/matchResults/standings/${type}/${season}`)
            .pipe(map(res => 
              plainToClass(LeagueStanding, res)
          ));
}

public readAggregateWeights(type: AggregateType, season: number): Observable<AggregateWeight[]> {
  return this.http.get<MatchResult[]>(`${this.globalService.ApiUrl}/api/matchResults/aggregateWeights/${type}/${season}`)
            .pipe(map(res => 
              plainToClass(AggregateWeight, res)
          ));
}

public readTrophyWinners(type: TrophyType, season: number): Observable<TrophyWinner[]> {
  return this.http.get<TrophyWinner[]>(`${this.globalService.ApiUrl}/api/trophywinners/${type}/${season}`)
            .pipe(map(res => 
              plainToClass(TrophyWinner, res)
          ));
}

}
