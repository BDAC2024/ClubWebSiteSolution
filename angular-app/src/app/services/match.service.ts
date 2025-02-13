import { Injectable } from '@angular/core';
import { ClubEvent } from 'src/app/models/club-event';
import { AggregateType, MatchType, TrophyType } from 'src/app/models/match-enum';
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
  public get PairsTabName() : string { return this.getTabName(MatchType.Pairs); }
  public get EveningTabName() : string { return this.getTabName(MatchType.Evening); }
  public get VisitorsTabName() : string { return this.getTabName(MatchType.Visitors); }
  public get QualifierTabName() : string { return this.getTabName(MatchType.Qualifier); }

  private getTabName(type: MatchType): string {
    return this.screenService.IsHandsetPortrait? MatchType.CompactName(type) : MatchType.FullName(type)
  }

  public get AggSpringTabName() : string { return this.getAggTabName(AggregateType.Spring); }
  public get AggClubRiverTabName() : string { return this.getAggTabName(AggregateType.ClubRiver); }
  public get AggClubPondTabName() : string { return this.getAggTabName(AggregateType.ClubPond); }
  public get AggPairsTabName() : string { return this.getAggTabName(AggregateType.Pairs); }
  public get AggEveningTabName() : string { return this.getAggTabName(AggregateType.Evening); }
  public get AggJuniorTabName() : string { return this.getAggTabName(AggregateType.Junior); }
  public get AggOsuTabName() : string { return this.getAggTabName(AggregateType.OSU); }

  private getAggTabName(type: AggregateType): string {
    return this.screenService.IsHandsetPortrait? AggregateType.CompactName(type) : AggregateType.FullName(type)
  }

  public get TrophySeniorTabName() : string { return this.getTrophyTabName(TrophyType.Senior); }
  public get TrophyJuniorTabName() : string { return this.getTrophyTabName(TrophyType.Junior); }

  private getTrophyTabName(type: TrophyType): string {
    return this.screenService.IsHandsetPortrait? TrophyType.CompactName(type) : TrophyType.FullName(type)
  }

}
