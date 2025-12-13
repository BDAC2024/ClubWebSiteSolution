import { Type } from 'class-transformer';
import { EventType } from 'src/app/models/event-enum';
import { AggregateType, MatchType } from 'src/app/models/match-enum';
import { Season } from './season-enum';
import { AggregateWeight } from './aggregate-weight';

export class ClubEvent {
  id!: string;
  day!: string;
  time!: string;
  descriptionForTable!: string;
  season!: Season;
  
  @Type(() => Date)
  date!: Date;
  
  inThePast!: boolean;
  eventType!: EventType;
  matchType?: MatchType;
  matchDraw?: Date;
  matchStart?: Date;
  matchEnd?: Date;
  number?: number;
  description!: string;
  cup?: string;
  aggregateType?: AggregateType
}



