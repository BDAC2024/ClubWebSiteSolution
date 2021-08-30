export class Rules {
    dbKey!: string;
    title!: string;
    body!: string;
    ruleType!: RuleType;
}

export enum RuleType {
    General = 0,
    Match,
    JuniorGeneral,
    JuniorMatch
  }
  
  export namespace RuleType {
    export function CompactName(type: RuleType): string {
      switch(type) {
        case RuleType.General:
          return "General Rules";
        case RuleType.Match:
          return "Match Rules";
        case RuleType.JuniorGeneral:
          return "Jnr/Int General Rules";
        case RuleType.JuniorMatch:
          return "Jnr/Int Match Rules";
        default:
          return RuleType[type];
      }
    }
    export function FullName(type: RuleType): string {
      switch(type) {
        case RuleType.General:
          return "General Rules";
        case RuleType.Match:
          return "Match Rules";
        case RuleType.JuniorGeneral:
          return "Junior / Intermediate - General Rules";
        case RuleType.JuniorMatch:
          return "Junior / Intermediate - Match Rules";
        default:
          return RuleType[type];
      }
    }
  }
    