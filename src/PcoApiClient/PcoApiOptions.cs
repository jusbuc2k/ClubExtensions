using System;
using System.Collections.Generic;
using System.Text;

namespace PcoApiClient
{
    public class PcoApiOptions
    {
        public string Url { get; set; } = "https://api.planningcenteronline.com/";

        public string AuthenticationMethod { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }
    }
}
