using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PcoApiClient.Models
{
    public class PcoCheckInsLocation
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("position")]
        public int Position { get; set; }

        [JsonProperty("grade_min")]
        public int? GradeMin { get; set; }

        [JsonProperty("grade_max")]
        public int? GradeMax { get; set; }

        [JsonProperty("age_min_in_months")]
        public int? MinAgeInMonths { get; set; }

        [JsonProperty("age_max_in_months")]
        public int? MaxAgeInMonths { get; set; }

        [JsonProperty("age_on")]
        public DateTime? AgeOn { get; set; }

        [JsonProperty("age_range_by")]
        public string AgeRangeBy { get; set; }

        [JsonProperty("opened")]
        public bool Opened { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("gender")]
        public string Gender { get; set; }

        [JsonProperty("child")]
        public bool? Child { get; set; }


    }
}
