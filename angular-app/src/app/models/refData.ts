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
    dayTicketCost!: number;
    previewers!: number[];
    treasurers!: number[];
    memberSecretaries!: number[];
    dayTicketsEnabled!: boolean;
    guestTicketsEnabled!: boolean;
    membershipsEnabled!: boolean;
}