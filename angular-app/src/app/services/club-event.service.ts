import { Injectable } from '@angular/core';
import { ClubEvent, ClubEventDto, } from '../models/club-event';
import { EventType } from '../models/event-enum';
import { MatchType } from '../models/match-enum';
import { ScreenService } from './screen.service';
import { plainToClass } from 'class-transformer';

const ELEMENT_DATA: ClubEventDto[] = [
  {id: 1, date: new Date('2021-03-27 13:00'), eventType: EventType.Work, description: 'Pond'},
  {id: 2, date: new Date('2021-04-11'), eventType: EventType.Match, matchType: MatchType.Spring, matchDraw: new Date('2021-04-11:08:30'), matchStart: new Date('2021-04-11:10:00'), matchEnd: new Date('2021-04-11:16:00'), number: 1, description: 'Roecliffe Pond'},
  {id: 3, date: new Date('2021-04-13 20:00'), eventType: EventType.Meeting, description: 'Committee Meeting'},
  {id: 4, date: new Date('2021-04-25'), eventType: EventType.Match, matchType: MatchType.Spring, matchDraw: new Date('2021-04-25:08:30'), matchStart: new Date('2021-04-25:09:30'), matchEnd: new Date('2021-04-25:15:30'), number: 2, description: 'Roecliffe Pond'},
  {id: 5, date: new Date('2021-05-01 13:00'), eventType: EventType.Work, description: 'Pond'},
  {id: 6, date: new Date('2021-05-09'), eventType: EventType.Match, matchType: MatchType.Spring, matchDraw: new Date('2021-05-09:08:30'), matchStart: new Date('2021-05-09:09:30'), matchEnd: new Date('2021-05-09:15:30'), number: 3, description: 'Roecliffe Pond'},
  {id: 7, date: new Date('2021-05-11 20:00'), eventType: EventType.Meeting, description: 'Committee Meeting'},
  {id: 8, date: new Date('2021-05-15 13:00'), eventType: EventType.Work, description: 'Cricket Field'},
  {id: 9, date: new Date('2021-05-23'), eventType: EventType.Match, matchType: MatchType.Spring, matchDraw: new Date('2021-05-23:08:30'), matchStart: new Date('2021-05-23:09:30'), matchEnd: new Date('2021-05-23:15:30'), number: 4, description: 'Roecliffe Pond'},
  {id: 10, date: new Date('2021-06-05'), eventType: EventType.Match, matchType: MatchType.Junior, number: 1, description: 'Roecliffe Pond', cup: 'Heeley'},
  {id: 11, date: new Date('2021-06-06'), eventType: EventType.Match, matchType: MatchType.Spring, matchDraw: new Date('2021-06-06:08:30'), matchStart: new Date('2021-06-06:09:30'), matchEnd: new Date('2021-06-06:15:30'), number: 5, description: 'Roecliffe Pond'},
  {id: 12, date: new Date('2021-06-08 20:00'), eventType: EventType.Meeting, description: 'Committee Meeting'},
  {id: 13, date: new Date('2021-06-12 13:00'), eventType: EventType.Work, description: 'O.S.U'},
  {id: 14, date: new Date('2021-06-19'), eventType: EventType.Match, matchType: MatchType.Junior, number: 2, description: 'Roecliffe Pond', cup: 'Leeming'},
  {id: 15, date: new Date('2021-06-20'), eventType: EventType.Match, matchType: MatchType.Club, number: 1, description: 'Cricket Field', cup: 'R.Potter'},
  {id: 16, date: new Date('2021-06-27'), eventType: EventType.Match, matchType: MatchType.OSU, number: 1, description: 'Boroughbridge'},
  {id: 17, date: new Date('2021-07-03'), eventType: EventType.Match, matchType: MatchType.Junior, number: 3, description: 'Roecliffe Pond', cup: 'York House'},
  {id: 18, date: new Date('2021-07-11'), eventType: EventType.Match, matchType: MatchType.Club, number: 2, description: 'Away Match Helberby', cup: 'F.Ljudovitch'},
  {id: 19, date: new Date('2021-07-13 20:00'), eventType: EventType.Meeting, description: 'Committee Meeting'},
  {id: 20, date: new Date('2021-07-17'), eventType: EventType.Match, matchType: MatchType.Junior, number: 4, description: 'Roecliffe Pond', cup: 'Brown / Potter'},
  {id: 21, date: new Date('2021-07-25'), eventType: EventType.Match, matchType: MatchType.Club, number: 3, description: 'Roecliffe Pond', cup: 'Lawson Tancred'},
  {id: 22, date: new Date('2021-07-31'), eventType: EventType.Match, matchType: MatchType.Junior, number: 5, description: 'Roecliffe Pond', cup: 'Newboult'},
  {id: 23, date: new Date('2021-08-01'), eventType: EventType.Match, matchType: MatchType.OSU, number: 2, description: 'Helberby'},
  {id: 24, date: new Date('2021-08-08'), eventType: EventType.Match, matchType: MatchType.Officials, description: 'Officials Match', cup: 'Officials'},
  {id: 25, date: new Date('2021-08-14'), eventType: EventType.Match, matchType: MatchType.Junior, number: 6, description: 'Roecliffe Pond', cup: 'Hey Trophy'},
  {id: 26, date: new Date('2021-08-22'), eventType: EventType.Match, matchType: MatchType.Club, number: 4, description: 'Cricket Field', cup: 'Nixon Waite'},
  {id: 27, date: new Date('2021-09-05'), eventType: EventType.Match, matchType: MatchType.OSU, number: 3, description: 'Newton'},
  {id: 28, date: new Date('2021-09-12'), eventType: EventType.Match, matchType: MatchType.Club, number: 5, description: 'Away Match Newton', cup: 'Whitbread'},
  {id: 29, date: new Date('2021-09-14 20:00'), eventType: EventType.Meeting, description: 'Committee Meeting'},
  {id: 30, date: new Date('2021-09-19'), eventType: EventType.Match, matchType: MatchType.OSU, description: 'OSU Spare Date'},
  {id: 31, date: new Date('2021-09-26'), eventType: EventType.Match, matchType: MatchType.Club, number: 6, description: 'Roecliffe Pond', cup: 'Autumn Cup'},
  {id: 32, date: new Date('2021-10-10'), eventType: EventType.Match, matchType: MatchType.Club, number: 7, description: 'Ellenthorpe Middle Lane', cup: 'Worth'},
  {id: 33, date: new Date('2021-10-12 20:00'), eventType: EventType.Meeting, description: 'Committee Meeting'},
  {id: 34, date: new Date('2021-10-20 20:00'), eventType: EventType.Meeting, description: 'OSU Meeting Golden Lion Helperby'},
  {id: 35, date: new Date('2021-10-24'), eventType: EventType.Match, matchType: MatchType.Club, number: 8, description: 'Ellenthorpe Hall', cup: 'Bradley'},
  {id: 36, date: new Date('2021-11-07'), eventType: EventType.Match, matchType: MatchType.Club, number: 9, description: 'Halls Arms Lane', cup: 'Y.E.P'},
  {id: 37, date: new Date('2021-11-09 20:00'), eventType: EventType.Meeting, description: 'Committee Meeting'},
  {id: 38, date: new Date('2021-11-21'), eventType: EventType.Match, matchType: MatchType.Club, number: 10, description: 'Ings Lane', cup: 'G.Wilford'},
  {id: 39, date: new Date('2021-12-05'), eventType: EventType.Match, matchType: MatchType.Club, number: 11, description: 'Ellenthorpe Hall', cup: 'Coates'},
  {id: 40, date: new Date('2021-12-14 20:00'), eventType: EventType.Meeting, description: 'Committee Meeting'},
  {id: 41, date: new Date('2021-12-19'), eventType: EventType.Match, matchType: MatchType.Club, number: 12, description: 'Fur & Feather Ellenthorpe Hall', cup: 'J.Tilburn'},
  {id: 42, date: new Date('2022-01-02'), eventType: EventType.Match, matchType: MatchType.Club, number: 13, description: 'Ings Lane Top', cup: 'B & P Trophy'},
  {id: 43, date: new Date('2022-01-11 20:00'), eventType: EventType.Meeting, description: 'A.G.M Three Horseshoes'},
  {id: 44, date: new Date('2022-01-16'), eventType: EventType.Match, matchType: MatchType.Club, number: 14, description: 'Ellenthorpe Hall', cup: 'R.B Vase'},
  {id: 45, date: new Date('2022-01-30'), eventType: EventType.Match, matchType: MatchType.Club, number: 15, description: 'Ings Lane Bottom', cup: 'Gill'},
  {id: 46, date: new Date('2022-02-08 20:00'), eventType: EventType.Meeting, description: 'Committee Meeting'},
  {id: 47, date: new Date('2022-02-13'), eventType: EventType.Match, matchType: MatchType.Club, number: 16, description: 'Ings Lane Top', cup: 'B & P Rosebowl'},
  {id: 48, date: new Date('2022-02-27'), eventType: EventType.Match, matchType: MatchType.Club, number: 17, description: 'Cricket Field', cup: 'Season End'},
  {id: 49, date: new Date('2022-03-08 20:00'), eventType: EventType.Meeting, description: 'Committee Meeting'},
  {id: 50, date: new Date('2022-05-31 20:00'), eventType: EventType.Meeting, description: 'Presentation Night - Date T.B.A'},
  ];

@Injectable({
  providedIn: 'root'
})
export class ClubEventService {

  constructor(private screenService: ScreenService) { }

  public GetEvents(type: EventType): ClubEvent[]
  {
    if (type == EventType.All) {
      return this.readEvents();
    } else {
      return this.readEvents().filter(m => m.eventType === type);
    }
  }

  public get AllTabName() : string { return this.getTabName(EventType.All); }
  public get MatchTabName() : string { return this.getTabName(EventType.Match); }
  public get WorkPartyTabName() : string { return this.getTabName(EventType.Work); }
  public get MeetingTabName() : string { return this.getTabName(EventType.Meeting); }

  private getTabName(type: EventType): string {
    return this.screenService.IsHandsetPortrait? EventType.CompactName(type) : EventType.FullName(type)
  }

  private readEvents(): ClubEvent[] {
    var events = plainToClass(ClubEvent, ELEMENT_DATA);
    
    return events;
  }
}
