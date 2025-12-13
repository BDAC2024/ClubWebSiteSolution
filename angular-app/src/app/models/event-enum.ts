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
                return "Match";
            case EventType.Work:
                return "Working Party";
            case EventType.Meeting:
                return "Meeting";
            default:
                return EventType[type];
        }
    }
}
