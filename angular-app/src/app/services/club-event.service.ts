import { Injectable } from '@angular/core';
import { ClubEvent } from '../models/club-event';
import { EventType, MatchType } from '../models/enums';

const ELEMENT_DATA: ClubEvent[] = [
  {id: 1, date: new Date('27 Mar 21'), time: new Date('13:00'), eventType: EventType.Work, description: 'Pond'},
  {id: 2, date: new Date('11 Apr 21'), eventType: EventType.Match, matchType: MatchType.Spring, number: 1, description: 'Roecliffe Pond'},
  {id: 3, date: new Date('13 Apr 21'), eventType: EventType.Meeting, description: 'Committee Meeting'},
  {id: 4, date: new Date('25 Apr 21'), eventType: EventType.Match, matchType: MatchType.Spring, number: 2, description: 'Roecliffe Pond'},
  {id: 5, date: new Date('01 May 21'), time: new Date('13:00'), eventType: EventType.Work, description: 'Pond'},
  {id: 6, date: new Date('09 May 21'), eventType: EventType.Match, matchType: MatchType.Spring, number: 3, description: 'Roecliffe Pond'},
  {id: 7, date: new Date('11 May 21'), eventType: EventType.Meeting, description: 'Committee Meeting'},
  {id: 8, date: new Date('15 May 21'), time: new Date('13:00'), eventType: EventType.Work, description: 'Cricket Field'},
  {id: 9, date: new Date('23 May 21'), eventType: EventType.Match, matchType: MatchType.Spring, number: 4, description: 'Roecliffe Pond'},
  {id: 10, date: new Date('05 Jun 21'), eventType: EventType.Match, matchType: MatchType.Junior, number: 1, description: 'Roecliffe Pond', cup: 'Heeley'},
  {id: 11, date: new Date('06 Jun 21'), eventType: EventType.Match, matchType: MatchType.Spring, number: 5, description: 'Roecliffe Pond'},
  {id: 12, date: new Date('08 Jun 21'), time: new Date('20:00'), eventType: EventType.Meeting, description: 'Committee Meeting'},
  {id: 13, date: new Date('12 Jun 21'), time: new Date('13:00'), eventType: EventType.Work, description: 'O.S.U'},
  {id: 14, date: new Date('19 Jun 21'), eventType: EventType.Match, matchType: MatchType.Junior, number: 2, description: 'Roecliffe Pond', cup: 'Leeming'},
  {id: 15, date: new Date('20 Jun 21'), eventType: EventType.Match, matchType: MatchType.Club, number: 1, description: 'Cricket Field', cup: 'R.Potter'},
  {id: 16, date: new Date('27 Jun 21'), eventType: EventType.Match, matchType: MatchType.OSU, number: 1, description: 'Boroughbridge'},
  {id: 17, date: new Date('03 Jul 21'), eventType: EventType.Match, matchType: MatchType.Junior, number: 3, description: 'Roecliffe Pond', cup: 'York House'},
  {id: 18, date: new Date('11 Jul 21'), eventType: EventType.Match, matchType: MatchType.Club, number: 2, description: 'Away Match Helberby', cup: 'F.Ljudovitch'},
  {id: 19, date: new Date('13 Jul 21'), time: new Date('20:00'), eventType: EventType.Meeting, description: 'Committee Meeting'},
  {id: 20, date: new Date('17 Jul 21'), eventType: EventType.Match, matchType: MatchType.Junior, number: 4, description: 'Roecliffe Pond', cup: 'Brown / Potter'},
  {id: 21, date: new Date('25 Jul 21'), eventType: EventType.Match, matchType: MatchType.Club, number: 3, description: 'Roecliffe Pond', cup: 'Lawson Tancred'},
  {id: 22, date: new Date('31 Jul 21'), eventType: EventType.Match, matchType: MatchType.Junior, number: 5, description: 'Roecliffe Pond', cup: 'Newboult'},
  {id: 23, date: new Date('01 Aug 21'), eventType: EventType.Match, matchType: MatchType.OSU, number: 2, description: 'Helberby'},
  {id: 24, date: new Date('08 Aug 21'), eventType: EventType.Match, matchType: MatchType.Officials, description: 'Officials Match', cup: 'Officials'},
  {id: 25, date: new Date('14 Aug 21'), eventType: EventType.Match, matchType: MatchType.Junior, number: 6, description: 'Roecliffe Pond', cup: 'Hey Trophy'},
  {id: 26, date: new Date('22 Aug 21'), eventType: EventType.Match, matchType: MatchType.Club, number: 4, description: 'Cricket Field', cup: 'Nixon Waite'},
  {id: 27, date: new Date('05 Sep 21'), eventType: EventType.Match, matchType: MatchType.OSU, number: 3, description: 'Netwon'},
  {id: 28, date: new Date('12 Sep 21'), eventType: EventType.Match, matchType: MatchType.Club, number: 5, description: 'Away Match Newton', cup: 'Whitbread'},
  {id: 29, date: new Date('14 Sep 21'), time: new Date('20:00'), eventType: EventType.Meeting, description: 'Committee Meeting'},
  {id: 30, date: new Date('19 Sep 21'), eventType: EventType.Match, matchType: MatchType.OSU, description: 'OSU Spare Date'},
  {id: 31, date: new Date('26 Sep 21'), eventType: EventType.Match, matchType: MatchType.Club, number: 6, description: 'Roecliffe Pond', cup: 'Autumn Cup'},
  {id: 32, date: new Date('10 Oct 21'), eventType: EventType.Match, matchType: MatchType.Club, number: 7, description: 'Ellenthorpe Middle Lane', cup: 'Worth'},
  {id: 33, date: new Date('12 Oct 21'), time: new Date('20:00'), eventType: EventType.Meeting, description: 'Committee Meeting'},
  {id: 34, date: new Date('20 Oct 21'), time: new Date('20:00'), eventType: EventType.Meeting, description: 'OSU Meeting Golden Lion Helperby'},
  {id: 35, date: new Date('24 Oct 21'), eventType: EventType.Match, matchType: MatchType.Club, number: 8, description: 'Ellenthorpe Hall', cup: 'Bradley'},
  {id: 36, date: new Date('07 Nov 21'), eventType: EventType.Match, matchType: MatchType.Club, number: 9, description: 'Halls Arms Lane', cup: 'Y.E.P'},
  {id: 37, date: new Date('09 Nov 21'), time: new Date('20:00'), eventType: EventType.Meeting, description: 'Committee Meeting'},
  {id: 38, date: new Date('21 Nov 21'), eventType: EventType.Match, matchType: MatchType.Club, number: 10, description: 'Ings Lane', cup: 'G.Wilford'},
  {id: 39, date: new Date('05 Dec 21'), eventType: EventType.Match, matchType: MatchType.Club, number: 11, description: 'Ellenthorpe Hall', cup: 'Coates'},
  {id: 40, date: new Date('14 Dec 21'), time: new Date('20:00'), eventType: EventType.Meeting, description: 'Committee Meeting'},
  {id: 41, date: new Date('19 Dec 21'), eventType: EventType.Match, matchType: MatchType.Club, number: 12, description: 'Fur & Feather Ellenthorpe Hall', cup: 'J.Tilburn'},
  {id: 42, date: new Date('02 Jan 22'), eventType: EventType.Match, matchType: MatchType.Club, number: 13, description: 'Ings Lane Top', cup: 'B & P Trophy'},
  {id: 43, date: new Date('11 Jan 22'), time: new Date('20:00'), eventType: EventType.Meeting, description: 'A.G.M Three Horseshoes'},
  {id: 44, date: new Date('16 Jan 22'), eventType: EventType.Match, matchType: MatchType.Club, number: 14, description: 'Ellenthorpe Hall', cup: 'R.B Vase'},
  {id: 45, date: new Date('30 Jan 22'), eventType: EventType.Match, matchType: MatchType.Club, number: 15, description: 'Ings Lane Bottom', cup: 'Gill'},
  {id: 46, date: new Date('08 Feb 22'), time: new Date('20:00'), eventType: EventType.Meeting, description: 'Committee Meeting'},
  {id: 47, date: new Date('13 Feb 22'), eventType: EventType.Match, matchType: MatchType.Club, number: 16, description: 'Ings Lane Top', cup: 'B & P Rosebowl'},
  {id: 48, date: new Date('27 Feb 22'), eventType: EventType.Match, matchType: MatchType.Club, number: 17, description: 'Cricket Field', cup: 'Season End'},
  {id: 49, date: new Date('08 Mar 22'), time: new Date('20:00'), eventType: EventType.Meeting, description: 'Committee Meeting'},
  {id: 50, date: new Date('31 May 22'), eventType: EventType.Meeting, description: 'Presentation Night - Date T.B.A'},
  
];

@Injectable({
  providedIn: 'root'
})
export class ClubEventService {

  constructor() { }

  public GetMatches(): ClubEvent[]
  {
    return ELEMENT_DATA.filter(m => m.eventType === EventType.Match);
  }

  public GetEvents(type: EventType): ClubEvent[]
  {
    if (type == EventType.All) {
      return ELEMENT_DATA;
    } else {
      return ELEMENT_DATA.filter(m => m.eventType === type);
    }
  }
}
