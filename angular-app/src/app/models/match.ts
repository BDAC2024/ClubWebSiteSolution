import { ClubEvent } from "./club-event";

export interface MatchParam {
    match: ClubEvent;
}  

class MatchResultBase {
  matchId!: string;
  name!: string;
  peg!: string;
  points?: number;
}

export class MatchResult extends MatchResultBase {
  id!: string;
  WeightDecimal!: number;
  Weight!: string;
  PositionOrdinal!: string;
}
