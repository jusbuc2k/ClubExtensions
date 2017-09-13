using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PcoApiClient.Models
{
    public class PcoEmailAddress
    {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("primary")]
        public bool Primary { get; set; }
    }
}
