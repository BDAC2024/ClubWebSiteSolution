import { Type } from 'class-transformer';
import jwt_decode from 'jwt-decode';

export class Member {
    id!: string;
    dbKey!: string;
    token?: string;
    membershipNumber!: number;
    name!: string;
    admin: boolean = false;
    allowNameToBeUsed: boolean = false;
    @Type(() => Date)
    preferencesLastUpdated!: Date;
    pinResetRequired: boolean = false;
    seasonsActive!: number[];
    /**
     *
     */
    constructor(token: string | undefined = undefined) {
        if (token) {
            var tokenDecoded: any = jwt_decode(token || "");
            this.token = token;
            this.id = tokenDecoded.Key;
            this.membershipNumber = JSON.parse(tokenDecoded.MembershipNumber.toLowerCase());
            this.admin = JSON.parse(tokenDecoded.Admin.toLowerCase());
            this.allowNameToBeUsed = JSON.parse(tokenDecoded.AllowNameToBeUsed.toLowerCase());
            this.preferencesLastUpdated = new Date(tokenDecoded.PreferencesLastUpdated);
            this.name = tokenDecoded.Name;
            this.pinResetRequired = JSON.parse(tokenDecoded.PinResetRequired.toLowerCase());
        } else {
            // Do nothing, probably a logout
        }
    }

}

