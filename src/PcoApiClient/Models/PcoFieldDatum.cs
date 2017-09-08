using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PcoApiClient.Models
{
    public class PcoFieldDatum
    {
        [JsonProperty("field_definition_id")]
        public int FieldDefinitionID { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
