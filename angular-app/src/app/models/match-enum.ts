export enum MatchType {
  Spring = 0,
  Club,
  Junior,
  OSU,
  Specials
}

export namespace MatchType {
  export function CompactName(type: MatchType): string {
    switch(type) {
      case MatchType.Spring:
        return "Spring";
      case MatchType.Club:
        return "Club";
      case MatchType.Junior:
        return "Junior";
      case MatchType.OSU:
        return "OSU";
      case MatchType.Specials:
        return "Specials";
      default:
        return MatchType[type];
    }
  }
  export function FullName(type: MatchType): string {
    switch(type) {
      case MatchType.Spring:
        return "Spring League";
      case MatchType.Club:
        return "Club Match";
      case MatchType.Junior:
        return "Junior Match";
      case MatchType.OSU:
        return "Ouse/Swale/Ure";
      case MatchType.Specials:
        return "Specials";
      default:
        return MatchType[type];
    }
  }
}

export enum AggregateWeightType {
  Spring = 0,
  ClubRiver,
  ClubPond,
  None
}

export namespace AggregateWeightType {
  export function CompactName(type: AggregateWeightType): string {
    switch(type) {
      case AggregateWeightType.Spring:
        return "Spring";
      case AggregateWeightType.ClubRiver:
        return "Club/River";
      case AggregateWeightType.ClubPond:
        return "Club/Pond";
      default:
        return AggregateWeightType[type];
    }
  }
  export function FullName(type: AggregateWeightType): string {
    switch(type) {
      case AggregateWeightType.Spring:
        return "Spring Leage";
      case AggregateWeightType.ClubRiver:
        return "Club Match - River";
      case AggregateWeightType.ClubPond:
        return "Club Match - Pond";
      default:
        return AggregateWeightType[type];
    }
  }
}
