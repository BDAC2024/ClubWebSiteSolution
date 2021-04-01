import { EventType, MatchType } from 'src/app/models/enums';

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