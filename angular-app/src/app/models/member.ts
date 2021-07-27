import { Type } from 'class-transformer';
import jwt_decode from 'jwt-decode';

export class Member {
    id!: string;
    token?: string;
    membershipNumber!: number;
    name!: string;
    admin: boolean = false;
    allowNameToBeUsed: boolean = false;
    enabled: boolean = false;
    @Type(() => Date)
    lastPaid!: Date;
    @Type(() => Date)
    preferencesLastUpdated!: Date;
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
            this.lastPaid = new Date(tokenDecoded.LastPaid);
        } else {
            // Do nothing, probably a logout
        }
    }

}

