export class LoginDetails {
    membershipNumber!: number;
    pin!: number

    /**
     *
     */
    constructor(membNo: number, pinNo: number) {
        this.membershipNumber = membNo;
        this.pin = pinNo;
    }
}