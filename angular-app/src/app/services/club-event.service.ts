import { Injectable } from '@angular/core';

import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';

import { ClubEvent } from '../models/club-event';
import { EventType } from '../models/event-enum';
import { ScreenService } from './screen.service';
import { plainToClass } from 'class-transformer';
import { GlobalService } from './global.service';
import { map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class ClubEventService {

  private events: ClubEvent[] = [];

  constructor(
    private http: HttpClient, 
    private screenService: ScreenService,
    private globalService: GlobalService) {

  }

  public get AllTabName() : string { return this.getTabName(EventType.All); }
  public get MatchTabName() : string { return this.getTabName(EventType.Match); }
  public get WorkPartyTabName() : string { return this.getTabName(EventType.Work); }
  public get MeetingTabName() : string { return this.getTabName(EventType.Meeting); }

  private getTabName(type: EventType): string {
    return this.screenService.IsHandsetPortrait? EventType.CompactName(type) : EventType.FullName(type)
  }

  public readEvents(): Observable<ClubEvent[]> {
    return this.http.get<ClubEvent[]>(`${this.globalService.ApiUrl}/api/events`)
              .pipe(map(res => 
                  plainToClass(ClubEvent, res)
              ));
  }
}
