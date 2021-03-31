export interface Match {
    id: number;
    number: number;
    date: Date;
    day: string;
    venue: string;
    cup: string;
}

export interface MatchParam {
    match: Match;
}  

export interface MatchResult {
  matchId: number;
  name: string;
  weight: string;
  points?: number;
}
