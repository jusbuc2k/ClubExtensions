﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PcoApiClient.Models
{
    public class PcoSingleResponse<T> : PcoResponse
    {
        [JsonProperty("data")]
        public PcoDataRecord<T> Data { get; set; }

        [JsonProperty("meta")]
        public PcoResponseMetadata Meta { get; set; }

        public IEnumerable<PcoDataRecord<R>> GetRelated<R>(string relationshipName)
        {
            var relationships = this.Data.Relationships[relationshipName];

            if (relationships == null || relationships.Data == null)
            {
                return Enumerable.Empty<PcoDataRecord<R>>();
            }

            return relationships.GetDataAsList()
                .SelectMany(rel => this.Included.Where(x => x.Type == rel.Type && x.ID == rel.ID)
                .Select(s => new PcoDataRecord<R>(){
                    ID = s.ID,
                    Type = s.Type,
                    Relationships = s.Relationships,
                    Attributes = s.Attributes.ToObject<R>()
                }));
        }
    }
}
