import { Type } from "class-transformer";

export class RefData {
    currentSeason!: number;
    seasons!: Season[];
    appSettings!: AppSettings;
}

export class Season {
    season!: number;
    name!: string;
    @Type(() => Date)
    starts!: Date;
    @Type(() => Date)
    ends!: Date;
}

export class AppSettings {
    guestTicketCost!: number;
    previewers!: number[];
}