import { Type } from "class-transformer";

export class RefData {
    currentSeason!: number;
    seasons!: Season[];
    seasonsForMembershipPurchase!: Season[];
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
    pondGateKeyCost!: number;
    guestTicketCost!: number;
    dayTicketCost!: number;
    handlingCharge!: number;
    previewers!: number[];
    treasurers!: number[];
    membershipSecretaries!: number[];
    dayTicketsEnabled!: boolean;
    guestTicketsEnabled!: boolean;
    membershipsEnabled!: boolean;
    pondGateKeysEnabled!: boolean;
}