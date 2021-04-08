import { DatePipe } from '@angular/common';
import { EventType } from 'src/app/models/event-enum';
import { MatchType } from 'src/app/models/match-enum';

const datepipe: DatePipe = new DatePipe('en-GB');

class ClubEventBase {
  id!: number;
  date!: Date;
  eventType!: EventType;
  matchType?: MatchType;
  matchDraw?: Date;
  matchStart?: Date;
  matchEnd?: Date;
  number?: number;
  description!: string;
  cup?: string;
}

export class ClubEventDto extends ClubEventBase {
}

export class ClubEvent extends ClubEventBase {

  public get day(): string {
    var formatted = datepipe.transform(this.date, 'E');
    return formatted == undefined ? '' : formatted;
  }

  public get time(): string {
    if (this.eventType != EventType.Match) {
      var formatted = datepipe.transform(this.date, 'HH:mm');
      return formatted == undefined ? '' : formatted;
    } else {
      return '';
    }
  }

  public get descriptionForTable(): string {
    if (this.eventType == EventType.Work) {
      return EventType.FullName(this.eventType) + " at " + this.description;
    } else if (this.eventType == EventType.Match) {
      return `${MatchType.FullName(this.matchType as MatchType)} ${this.number? ` no. ${this.number}` : ``} at ${this.description}`;
    } else {
      return this.description;
    }
  }
}

