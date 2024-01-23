import { Type } from "class-transformer";
import { PaymentType } from "./payment-enum";

export class Payment {
    sessionId!: string;
    @Type(() => Date)
    paidOn!: Date;
    validOn!: Date;
    category!: PaymentType;
    purchase!: string;
    membersName!: string;
    holdersName!: string;
    email!: string;
    cardHoldersName!: string;
    shippingAddress!: string;
    amount!: number;
    status!: string;
}