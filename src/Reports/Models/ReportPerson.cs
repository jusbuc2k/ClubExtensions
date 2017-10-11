using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClubExtensions.Reports.Models
{
    public class ReportPerson
    {
        public string PersonID { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string ClubName { get; set; }

        public string SubGroup { get; set; }

        public string HouseholdID { get; set; }

        public string HouseholdName { get; set; }

        public string PrimaryContactName { get; set; }

        public string PrimaryContactEmail { get; set; }

        public string PrimaryContactPhone { get; set; }

        public DateTime? BirthDate { get; set; }

        public int? Grade { get; set; }

        public string GradeName
        {
            get
            {
                if (!this.Grade.HasValue)
                {
                    return string.Empty;
                }

                switch (this.Grade.Value)
                {
                    case -1: return "PK";
                    case 0: return "KG";
                    case 1: return "1st";
                    case 2: return "2nd";
                    case 3: return "3rd";
                    case 4: return "4th";
                    case 5: return "5th";
                    case 6: return "6th";
                    case 7: return "7th";
                    case 8: return "8th";
                    case 9: return "9th";
                    case 10: return "10th";
                    case 11: return "11th";
                    case 12: return "12th";
                    default: return "N/A";
                }
            }
        }

        public int? Age
        {
            get
            {
                if (this.BirthDate.HasValue)
                {
                    return (int)Math.Floor(DateTime.Now.Subtract(this.BirthDate.Value).TotalDays / 365D);
                }
                else
                {
                    return null;
                }
            }
        }

        public DateTime? LastAttendance { get; set; }

        public string Gender { get; set; }
    }
}
