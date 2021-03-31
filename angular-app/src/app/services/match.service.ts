import { Injectable } from '@angular/core';
import { Match } from 'src/app/models/match';
import { MatchType } from 'src/app/models/matchTypeEnum';

const ELEMENT_DATA: Match[] = [
  {type: MatchType.Spring, id: 90, number: 1, date: new  Date('2021-06-30'), day: '', venue: 'Roecliffe Lake', cup: ''},

  {type: MatchType.Club, id: 100, number: 1, date: new  Date('2021-04-21'), day: '', venue: 'Cricket Field', cup: 'R. Potter'},
  {type: MatchType.Club, id: 102, number: 2, date: new  Date('2021-05-02'), day: '', venue: 'Ellenthorpe Middle Lane', cup: 'Lawson Tancred'},
  {type: MatchType.Club, id: 104, number: 3, date: new  Date('2021-05-16'), day: '', venue: 'Ings Lane Bottom', cup: 'Bradley'},
  {type: MatchType.Club, id: 108, number: 4, date: new  Date('2021-06-30'), day: '', venue: 'Roecliffe Lake', cup: 'Whitbread'},

  {type: MatchType.Junior, id: 200, number: 1, date: new  Date('2021-06-30'), day: '', venue: 'Roecliffe Lake', cup: 'Heeley'},

  {type: MatchType.OSU, id: 400, number: 1, date: new  Date('2021-06-30'), day: '', venue: 'Boroughbridge', cup: ''},
];

@Injectable({
  providedIn: 'root'
})
export class MatchService {

  constructor() { }

  public GetMatches(type: MatchType): Match[]
  {
    return ELEMENT_DATA.filter(m => m.type === type); ;
  }

}
