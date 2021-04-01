import { ClubEvent } from "./club-event";

export interface MatchParam {
    match: ClubEvent;
}  

export interface MatchResult {
  matchId: number;
  name: string;
  weight: string;
  points?: number;
}
