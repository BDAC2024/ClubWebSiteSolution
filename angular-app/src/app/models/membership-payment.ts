import { MembershipType } from "./product-membership-enum";
import { Season } from "./refData";

export class ProductMembership {
  dbKey!: string;
  type!: MembershipType;
  description!: string;
  term!: string;
  runs!: string;
  cost!: number;
}

export class MembershipPaymentRequest {
  dbKey!: string;
  seasonName!: string;
  name!: string;
  dob!: Date;
  phoneNumber!: string;
  allowNameToBeUsed!: boolean;
  acceptPolicies!: boolean;
  paidForKey: boolean = false;
  underAge!: boolean;
  parentalConsent!: boolean;
  childCanSwim!: string;
  responsible1st!: string;
  responsible2nd!: string;
  responsible3rd!: string;
  responsible4th!: string;
  emergencyContact!: string;
  emergencyContactPhoneHome!: string;
  emergencyContactPhoneWork!: string;
  emergencyContactPhoneMobile!: string;
  successUrl!: string;
  cancelUrl!: string;
  disabilityCertificateFile?: File | null;
}



