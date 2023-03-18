export class GuestTicket {
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


