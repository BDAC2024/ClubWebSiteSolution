using AnglingClubWebServices.Helpers;
using AnglingClubWebServices.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;

namespace AnglingClubWebServices.Models
{
    public class OpenMatchRegistration : TableBase
    {
        [Required]
        public string OpenMatchId { get; set; }

        /// <summary>
        /// This is per match and starts at 1
        /// </summary>
        [Required]
        public int RegistrationNumber { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public JuniorAgeGroup AgeGroup { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string ParentName { get; set; }

        [Required]
        public string EmergencyContactPhone { get; set; }

        /// <summary>
        /// Optional
        /// </summary>
        public string ContactEmail { get; set; }

        public string AgeGroupAsString 
        { 
            get
            {
                return AgeGroup.EnumDescription();
            }
        }
    }
}
