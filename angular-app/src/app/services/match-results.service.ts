import { Injectable } from '@angular/core';
import { plainToClass } from 'class-transformer';
import { MatchResult, MatchResultDto } from '../models/match';

const ELEMENT_DATA: MatchResultDto[] = [
  { matchId: 2, name: 'A.Scaife', weightDecimal: 10.9375, peg: '2',points: 7},
  { matchId: 2, name: 'S.Townend', weightDecimal: 8.125, peg: '11',points: 6},
  { matchId: 2, name: 'S.Slater', weightDecimal: 3.1875, peg: '12',points: 5},
  { matchId: 2, name: 'P.Frankland', weightDecimal: 2.8125, peg: '10',points: 4},
  { matchId: 2, name: 'G.Tilburn', weightDecimal: 2.75, peg: '9',points: 3},
  { matchId: 2, name: 'P.Ingledew', weightDecimal: 2.5625, peg: '20',points: 1.5},
  { matchId: 2, name: 'M.Ledgeway', weightDecimal: 2.5625, peg: '24',points: 1.5},
  { matchId: 2, name: 'L.Caygill', weightDecimal: 2, peg: '17',points: 0},
  { matchId: 2, name: 'R.Frankland', weightDecimal: 1.1875, peg: '26',points: 0},
  { matchId: 2, name: 'P.Atkinson', weightDecimal: 0.375, peg: '18',points: 0},
  { matchId: 2, name: 'L.Frankland', weightDecimal: 0.25, peg: '5',points: 0},
  { matchId: 2, name: 'A.Shearer', weightDecimal: 0.125, peg: '4',points: 0},
  { matchId: 2, name: 'K.Adcock', weightDecimal: 0, peg: '3',points: 0},
  { matchId: 2, name: 'G.Lumsden', weightDecimal: 0, peg: '16',points: 0},
  { matchId: 2, name: 'S.Scaife', weightDecimal: 0, peg: '14',points: 0},
  { matchId: 2, name: 'D.Cotton', weightDecimal: 0, peg: '21',points: 0},
        
];


@Injectable({
  providedIn: 'root'
})
export class MatchResultsService {

  constructor() { }

  public Results(): MatchResult[] {
    var results = plainToClass(MatchResult, ELEMENT_DATA);

    return results;
  }

}
