import { Injectable } from '@angular/core';
import { ClubEvent } from 'src/app/models/club-event';
import { EventType, MatchType } from 'src/app/models/enums';
import { ClubEventService } from './club-event.service';


@Injectable({
  providedIn: 'root'
})
export class MatchService {

  constructor(
    private clubEventService: ClubEventService
  ) { }

  public GetMatches(type: MatchType): ClubEvent[]
  {
    return this.clubEventService.GetMatches().filter(m => m.matchType === type); ;
  }

}
