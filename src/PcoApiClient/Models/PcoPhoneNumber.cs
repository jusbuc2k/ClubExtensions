using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PcoApiClient.Models
{
    public class PcoPhoneNumber
    {
        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("primary")]
        public bool Primary { get; set; }
    }
}
