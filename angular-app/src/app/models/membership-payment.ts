import { MembershipType } from "./product-membership-enum";

export class ProductMembership {
  dbKey!: string;
  type!: MembershipType;
  description!: string;
  term!: string;
  runs!: string;
  cost!: number;
  priceId!: string;
}

export class MembershipPaymentRequest {
  dbKey!: string;
  name!: string;
  dob!: Date;
  phoneNumber!: string;
  allowNameToBeUsed!: boolean;
  acceptPolicies!: boolean;
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
}



