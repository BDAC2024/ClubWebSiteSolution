export enum MatchType {
  Spring = 0,
  Club,
  Junior,
  OSU,
  Specials,
  Pairs,
  Evening
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
      case MatchType.Pairs:
        return "Pairs";
      case MatchType.Evening:
        return "Evening";
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
      case MatchType.Pairs:
        return "Pairs";
      case MatchType.Evening:
        return "Evening League";
      default:
        return MatchType[type];
    }
  }
}

export enum AggregateType {
  Spring = 0,
  ClubRiver,
  ClubPond,
  None,
  Pairs,
  Evening,
  Junior,
  OSU,
}

export namespace AggregateType {
  export function CompactName(type: AggregateType): string {
    switch(type) {
      case AggregateType.Spring:
        return "Spring";
      case AggregateType.ClubRiver:
        return "Club/River";
      case AggregateType.ClubPond:
        return "Club/Pond";
        case AggregateType.Pairs:
          return "Pairs";
        case AggregateType.Evening:
          return "Evening";
        case AggregateType.Junior:
          return "Junior";
        case AggregateType.OSU:
          return "OSU";
        default:
          return AggregateType[type];
    }
  }
  export function FullName(type: AggregateType): string {
    switch(type) {
      case AggregateType.Spring:
        return "Spring Leage";
      case AggregateType.ClubRiver:
        return "Club Match - River";
      case AggregateType.ClubPond:
        return "Club Match - Pond";
      case AggregateType.Pairs:
        return "Pairs";
      case AggregateType.Evening:
        return "Evening";
      case AggregateType.Junior:
        return "Junior";
      case AggregateType.OSU:
        return "Ouse/Swale/Ure";
      default:
        return AggregateType[type];
    }
  }
}
