using PcoApiClient.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PcoApiClient
{
    public static class ExtensionMethods
    {
        public static T GetData<T>(this IDictionary<string, PcoPeopleRelationship> relationships, string key)
        {
            return relationships[key].Data.ToObject<T>();
        }
    }
}
