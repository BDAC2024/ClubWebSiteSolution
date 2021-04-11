import { ClubEvent } from "./club-event";

export interface MatchParam {
    match: ClubEvent;
}  

class MatchResultBase {
  matchId!: number;
  name!: string;
  peg!: string;
  weightDecimal!: number;
  points?: number;
}

export class MatchResultDto extends MatchResultBase {

}

export class MatchResult extends MatchResultBase {
  public get weight(): string {

    var wt = "D.N.W";

    if (this.weightDecimal > 0) {
      var wtLb = Math.floor(this.weightDecimal);
      var wtOz = Math.round((this.weightDecimal - wtLb) * 16);
      wt = `${wtLb}lb ${wtOz}oz`;
    } 

    return wt;
  }
}
