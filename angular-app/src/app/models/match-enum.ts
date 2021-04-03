export enum MatchType {
  Spring = 0,
  Club,
  Junior,
  OSU,
  Officials
}

export namespace MatchType {
  export function CompactName(type: MatchType): string {
    switch(type) {
      case MatchType.Spring:
        return "Spring";
      case MatchType.Club:
        return "Club";
      case MatchType.Junior:
        return "Jnr";
      case MatchType.OSU:
        return "O.S.U";
      default:
        return MatchType[type];
    }
  }
  export function FullName(type: MatchType): string {
    switch(type) {
      case MatchType.Spring:
        return "Spring League";
      case MatchType.Club:
        return "Club Matches";
      case MatchType.Junior:
        return "Junior Matches";
      case MatchType.OSU:
        return "Ouse/Swale/Ure";
      default:
        return MatchType[type];
    }
  }
}

