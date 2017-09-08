using System;
using System.Collections.Generic;
using System.Text;

namespace PcoApiClient
{
    public class PcoAuthenticationOptions
    {
        public string AuthLoginUrl { get; set; } = "https://api.planningcenteronline.com/oauth/";

        public string AuthTokenUrl { get; set; } = "https://api.planningcenteronline.com/oauth/token";

        public string MeUrl { get; set; } = "https://api.planningcenteronline.com/people/v2/me";

        public string GenerateLoginUrl(string clientID, string returnUrl, IEnumerable<string> scopes)
        {
            var url = new StringBuilder(this.AuthLoginUrl);

            url.Append("authorize?")
                .Append("client_id=").Append(clientID)
                .Append("&redirect_uri=").Append(returnUrl)
                .Append("&scope=").Append(string.Join(" ", scopes))
                .Append("&response_type=code");

            return url.ToString();
        }
    }
}
