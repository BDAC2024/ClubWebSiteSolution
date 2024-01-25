using System;

namespace AnglingClubWebServices.Interfaces
{
    public interface ITicketService
    {
        void IssueDayTicket(DateTime validOn, string holdersName, string emailAddress, string paymentId);
        void IssueGuestTicket(DateTime validOn, string membersName, string guestsName, int membershipNumber, string emailAddress, string paymentId);
    }
}