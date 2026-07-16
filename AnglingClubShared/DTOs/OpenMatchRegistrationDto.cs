using AnglingClubShared.Enums;
using System.ComponentModel.DataAnnotations;

namespace AnglingClubShared.DTOs
{
    public class OpenMatchRegistrationDto
    {
        public string DbKey { get; set; } = string.Empty;

        [Required]
        public string OpenMatchId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the angler's name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select an age group")]
        public JuniorAgeGroup? AgeGroup { get; set; }

        public string AgeGroupAsString { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the address")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the parent or guardian name")]
        public string ParentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the emergency contact phone number")]
        public string EmergencyContactPhone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string? ContactEmail { get; set; }
    }
}
