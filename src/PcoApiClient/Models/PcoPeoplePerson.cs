using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PcoApiClient.Models
{
    public class PcoPeoplePerson
    {
        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or set the gender (M or F)
        /// </summary>
        [JsonProperty("gender")]
        public string Gender { get; set; }
        
        [JsonProperty("birthdate")]
        public DateTime? BirthDate { get; set; }

        [JsonProperty("child")]
        public bool Child { get; set; }

        [JsonProperty("grade")]
        public int? Grade { get; set; }

        [JsonProperty("medical_notes")]
        public string MedicalNotes { get; set; }
    }
}
