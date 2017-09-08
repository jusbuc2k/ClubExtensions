using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PcoApiClient.Models
{
    public class PcoResponseMetadata
    {
        [JsonProperty("parent")]
        public PcoRecord Parent { get; set; }
    }
}
