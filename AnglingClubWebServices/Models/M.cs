namespace AnglingClubWebServices.Models
{
    public class MemberDto : Member
    {

        public int PinInput { 
            set
            {
                base.NewPin(value);
            }
        }
    }
}
