import { Type } from 'class-transformer';
import jwt_decode from 'jwt-decode';

export class Member {
  id!: string;
  dbKey!: string;
  token?: string;
  membershipNumber!: number;
  name!: string;
  surname!: string;
  email!: string;
  admin: boolean = false;
  treasurer: boolean = false;
  committeeMember: boolean = false;
  secretary: boolean = false;
  membershipSecretary: boolean = false;
  previewer: boolean = false;
  allowNameToBeUsed: boolean = false;
  @Type(() => Date)
  preferencesLastUpdated!: Date;
  pinResetRequired: boolean = false;
  seasonsActive!: number[];
  reLoginRequired: boolean = false;
  initialPin!: number;
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
      this.treasurer = Member.getBoolClaim(tokenDecoded.Treasurer);
      this.committeeMember = Member.getBoolClaim(tokenDecoded.CommitteeMember);
      this.secretary = Member.getBoolClaim(tokenDecoded.Secretary);
      this.membershipSecretary = Member.getBoolClaim(tokenDecoded.MembershipSecretary);
      this.previewer = Member.getBoolClaim(tokenDecoded.Previewer);
      this.allowNameToBeUsed = Member.getBoolClaim(tokenDecoded.AllowNameToBeUsed);
      this.preferencesLastUpdated = new Date(tokenDecoded.PreferencesLastUpdated);
      this.name = tokenDecoded.Name;
      this.email = tokenDecoded.Email;
      this.pinResetRequired = Member.getBoolClaim(tokenDecoded.PinResetRequired);
      this.reLoginRequired = Member.getBoolClaim(tokenDecoded.ReLoginRequired);

    } else {
      // Do nothing, probably a logout
    }
  }

  private static getBoolClaim(tokenClaim: any): boolean {

    if (tokenClaim) {
      return JSON.parse(tokenClaim.toLowerCase());
    } else {
      return false;
    }
  }


}

