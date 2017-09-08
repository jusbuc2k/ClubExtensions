using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PcoApiClient.Models
{
    public class PcoOrganization
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("date_format")]
        public string DateFormat { get; set; }
    }
}
