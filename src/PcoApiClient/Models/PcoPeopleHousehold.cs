using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PcoApiClient.Models
{
    public class PcoPeopleHousehold
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("member_count")]
        public int MemberCount { get; set; }

        [JsonProperty("primary_contact_id")]
        public string PrimaryContactID { get; set; }

        [JsonProperty("primary_contact_name")]
        public string PrimaryContactName { get; set; }
    }
}
