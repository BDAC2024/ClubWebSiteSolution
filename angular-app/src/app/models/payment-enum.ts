export enum PaymentType {
  Membership = 0,
  GuestTicket,
  DayTicket
}

export namespace PaymentType {
  export function CompactName(type: PaymentType): string {
    switch (type) {
      case PaymentType.Membership:
        return "Memberships";
      case PaymentType.GuestTicket:
        return "Guest Tickets";
      case PaymentType.DayTicket:
        return "Day Tickets";

      default:
        return PaymentType[type];
    }
  }
  export function FullName(type: PaymentType): string {
    switch (type) {
      case PaymentType.Membership:
        return "Memberships";
      case PaymentType.GuestTicket:
        return "Guest Tickets";
      case PaymentType.DayTicket:
        return "Day Tickets";
      default:
        return PaymentType[type];
    }
  }
}

