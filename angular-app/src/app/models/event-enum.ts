export enum EventType {
    All = 0,
    Match,
    Work,
    Meeting,
}

export namespace EventType {
    export function CompactName(type: EventType): string {
        switch (type) {
            case EventType.All:
                return "All";
            case EventType.Match:
                return "Match";
            case EventType.Work:
                return "Wk Pty";
            case EventType.Meeting:
                return "Mtg";
            default:
                return EventType[type];
        }
    }
    export function FullName(type: EventType): string {
        switch (type) {
            case EventType.All:
                return "All";
            case EventType.Match:
                return "Matches";
            case EventType.Work:
                return "Working Parties";
            case EventType.Meeting:
                return "Meetings";
            default:
                return EventType[type];
        }
    }
}
