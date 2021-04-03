import { EventType } from 'src/app/models/event-enum';
import { MatchType } from 'src/app/models/match-enum';

export interface ClubEvent {
    id: number;
    date: Date;
    time?: Date;
    eventType: EventType;  
    matchType?: MatchType;  
    number?: number;
    description: string;
    cup?: string;
    day?: string;
  }