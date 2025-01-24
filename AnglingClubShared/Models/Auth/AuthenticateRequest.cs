using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AnglingClubShared.Models.Auth
{
    public class AuthenticateRequest : ObservableValidator
    {
        private int _membershipNumber;

        [Required]
        public int MembershipNumber
        {
            get => _membershipNumber;
            set => SetProperty(ref _membershipNumber, value, true);
        }

        private int _pin;

        [Required]
        public int Pin
        {
            get => _pin;
            set => SetProperty(ref _pin, value, true);
        }


        public void Validate() => ValidateAllProperties();
    }
}
