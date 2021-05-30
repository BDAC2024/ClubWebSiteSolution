import jwt_decode from 'jwt-decode';

export class Member {
    id!: string;
    token?: string;
    membershipNumber!: number;
    name!: string;
    isAdmin: boolean = false;
    allowNameToBeUsed: boolean = false;

    /**
     *
     */
    constructor(token: string | undefined = undefined) {
        if (token) {
            var tokenDecoded: any = jwt_decode(token || "");
            this.token = token;
            this.id = tokenDecoded.Key;
            this.membershipNumber = JSON.parse(tokenDecoded.MembershipNumber.toLowerCase());
            this.isAdmin = JSON.parse(tokenDecoded.IsAdmin.toLowerCase());
            this.allowNameToBeUsed = JSON.parse(tokenDecoded.AllowNameToBeUsed.toLowerCase());
        } else {
            // Do nothing, probably a logout
        }
    }

}

