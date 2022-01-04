import { Injectable } from '@angular/core';
import { ClubEvent } from 'src/app/models/club-event';
import { AggregateWeightType, MatchType } from 'src/app/models/match-enum';
import { EventType } from '../models/event-enum';
import { ClubEventService } from './club-event.service';
import { ScreenService } from './screen.service';


@Injectable({
  providedIn: 'root'
})
export class MatchService {

  constructor(
    private clubEventService: ClubEventService,
    private screenService: ScreenService
  ) { }

  // public GetMatches(type: MatchType): ClubEvent[]
  // {
  //   return this.clubEventService.GetEvents(EventType.Match).filter(m => m.matchType === type); ;
  // }

  public get SpringTabName() : string { return this.getTabName(MatchType.Spring); }
  public get ClubTabName() : string { return this.getTabName(MatchType.Club); }
  public get JuniorTabName() : string { return this.getTabName(MatchType.Junior); }
  public get OsuTabName() : string { return this.getTabName(MatchType.OSU); }
  public get SpecialsTabName() : string { return this.getTabName(MatchType.Specials); }

  private getTabName(type: MatchType): string {
    return this.screenService.IsHandsetPortrait? MatchType.CompactName(type) : MatchType.FullName(type)
  }

  public get AggSpringTabName() : string { return this.getAggTabName(AggregateWeightType.Spring); }
  public get AggClubRiverTabName() : string { return this.getAggTabName(AggregateWeightType.ClubRiver); }
  public get AggClubPondTabName() : string { return this.getAggTabName(AggregateWeightType.ClubPond); }

  private getAggTabName(type: AggregateWeightType): string {
    return this.screenService.IsHandsetPortrait? AggregateWeightType.CompactName(type) : AggregateWeightType.FullName(type)
  }
}
