using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PcoApiClient.Models
{
    public class PcoRecord
    {
        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
