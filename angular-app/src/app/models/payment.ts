import { Type } from "class-transformer";
import { PaymentType } from "./payment-enum";

export class Payment {
    dbKey!: string;
    orderId!: number;
    orderType!: PaymentType;
    description!: string;
    ticketNumber!: number;
    membersName!: string;
    guestsName!: string;
    ticketHoldersName!: string;
    @Type(() => Date)
    paidOn!: Date;
    validOn!: Date;
    amount!: number;
    status!: string;
}

export class PaymentDetail {
    orderId!: number;
    orderType!: PaymentType;
    description!: string;
    address!: string;
    @Type(() => Date)
    paidOn!: Date;
    amount!: number;
    status!: string;
    validOn!: Date;
    membershipNumber!: number;
    ticketNumber!: number;
    email!: string;
    ticketHoldersName!: string;
    membersName!: string;
    guestsName!: string;
    name!: string;
    doB!: Date;
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
    emergencyContactPhoneMobile!: string
}