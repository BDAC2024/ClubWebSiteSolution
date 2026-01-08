import { Type } from "class-transformer";
import { ClubEvent } from "./club-event";

export class RefData {
    currentSeason!: number;
    seasons!: Season[];
    seasonsForMembershipPurchase!: Season[];
    dayTicketMatches!: ClubEvent[];
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
    dayTicketsEnabled!: boolean;
    guestTicketsEnabled!: boolean;
    membershipsEnabled!: boolean;
    pondGateKeysEnabled!: boolean;
}
