using AnglingClubWebServices.Helpers;
using AnglingClubWebServices.Interfaces;
using Microsoft.Extensions.FileSystemGlobbing;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnglingClubWebServices.Models
{
    public class OpenMatch : TableBase
    {
        [Required]
        public DateTime Date { get; set; }

        [Required]
        public DateTime Draw { get; set; }

        [Required]
        public DateTime Starts { get; set; }

        [Required]
        public DateTime Ends { get; set; }

        [Required]
        public string Venue { get; set; }

        [Required]
        public int PegsAvailable { get; set; }

        [Required]
        public OpenMatchType OpenMatchType { get; set; }

        /// <summary>
        /// Set by controller when returning list of matches
        /// </summary>
        [NotMapped]
        public int PegsRemaining { get; set; }

        public Season Season 
        {
            get
            {
                return EnumUtils.SeasonForDate(Date).Value;
            }

        }

        public string DrawTime
        {
            get
            {
                return Draw.ToString("HH:mm");
            }
        }

        public string StartTime
        {
            get
            {
                return Starts.ToString("HH:mm");
            }
        }

        public string EndTime
        {
            get
            {
                return Ends.ToString("HH:mm");
            }
        }

        public bool InThePast
        {
            get
            {
                return Draw < DateTime.Now;
            }
        }

    }
}
