export class GuestTicket {

  cost!: number;
  validOn!: Date | null;
  membersName!: string;
  membershipNumber!: number;
  guestsName!: string;
  successUrl!: string;
  cancelUrl!: string;
}

export class OLDGuestTicket {
  dbKey!: string;
  ticketNumber!: number;
  cost!: number;
  issuedBy!: string;
  issuedByMembershipNumber!: number;
  issuedOn!: Date;
  ticketValidOn!: Date | null;
  membersName!: string;
  membershipNumber!: number;
  emailTo!: string;
  guestsName!: string;
  season!: number;
  imageData!: string;
}

