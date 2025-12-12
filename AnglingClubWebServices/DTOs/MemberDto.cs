using AnglingClubShared.Entities;
using AnglingClubWebServices.Models;

namespace AnglingClubWebServices.DTOs
{
    public class MemberDto : Member
    {

        public int PinInput
        {
            set
            {
                NewPin(value);
            }
        }
    }
}
