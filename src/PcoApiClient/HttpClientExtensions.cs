using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PcoApiClient
{
    public static class HttpClientExtensions
    {
        public static async Task<T> ReadJsonAsync<T>(this System.Net.Http.HttpContent content)
        {
            var json = await content.ReadAsStringAsync();

            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }
    }
}
