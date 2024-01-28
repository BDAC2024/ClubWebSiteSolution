using System;

namespace AnglingClubWebServices.Interfaces
{
    public interface ITicketService
    {
        void IssueDayTicket(int ticketNumber, DateTime validOn, string holdersName, string emailAddress, string paymentId);
        void IssueGuestTicket(int ticketNumber, DateTime validOn, DateTime issuedOn, string membersName, string guestsName, int membershipNumber, string emailAddress, string paymentId);
    }
}