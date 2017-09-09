using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Website.Models
{
    public class ClubAssignment
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int? Grade { get; set; } 

        public string Gender { get; set; }

        public DateTime? BirthDate { get; set; }

        public string OldClubName { get; set; }

        public string NewClubName { get; set; }

        public string PersonID { get; set; }

        public string ClubDatumID { get; set; }

        public bool IsChange
        {
            get
            {
                return !string.Equals(this.NewClubName, this.OldClubName);
            }
        }
    }
}
