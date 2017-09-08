using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PcoApiClient.Models
{
    public class PcoAuthTokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("created_at")]
        public long CreatedAt { get; set; }

        public DateTimeOffset GetExpirationDate()
        {
            return DateTimeOffset.Parse("1970-01-01").AddSeconds(this.CreatedAt).AddSeconds(this.ExpiresIn);
        }
    }
}
