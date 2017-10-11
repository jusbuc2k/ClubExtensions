using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using System.Net.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using PcoApiClient;
using PcoApiClient.Models;
using Website;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;

namespace WebApplicationBasic.Controllers
{
    public class HomeController : Controller
    {
        private readonly PcoApiClient.PcoAppRegistration _pcoApp;
        private readonly PcoApiClient.PcoAuthenticationOptions _pcoAuthOptions;

        public HomeController(IOptions<PcoApiClient.PcoAppRegistration> pcoApp, IOptions<PcoApiClient.PcoAuthenticationOptions> pcoAuthOptions)
        {
            _pcoApp = pcoApp.Value;
            _pcoAuthOptions = pcoAuthOptions.Value;
        }
   
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        public IActionResult PcoLogin()
        {
            var returnUrl = this.Url.Action("PcoLoginCallback", "Home", null, "https");

            var redirectUrl = _pcoAuthOptions.GenerateLoginUrl(_pcoApp.ClientID, returnUrl, new string[] { "people", "check_ins" });

            return this.Redirect(redirectUrl);
        }

        public async Task<IActionResult> PcoLoginCallback(string code)
        {
            var redirectUrl = this.Url.Action("PcoLoginCallback", "Home", null, "https");

            var client = new System.Net.Http.HttpClient();

            var tokenRequest = new
            {
                grant_type = "authorization_code",
                code = code,
                client_id = _pcoApp.ClientID,
                client_secret = _pcoApp.ClientSecret,
                redirect_uri = redirectUrl
            };
            var tokenRequestJson = Newtonsoft.Json.JsonConvert.SerializeObject(tokenRequest);

            var callbackResponse = await client.PostAsync(_pcoAuthOptions.AuthTokenUrl, new StringContent(tokenRequestJson, System.Text.Encoding.UTF8, "application/json"));
            var token = await callbackResponse.Content.ReadJsonAsync<PcoAuthTokenResponse>();
            var pcoClient = new PcoApiClient.PcoApiClient(client, new PcoApiOptions()
            {
                AuthenticationMethod = "Bearer",
                Password = token.AccessToken
            });

            var myInfo = await pcoClient.Get<PcoPeoplePerson>("people/v2/me");
            var ident = new System.Security.Claims.ClaimsIdentity("PCO");

            ident.AddClaim(new Claim(ClaimTypes.NameIdentifier, myInfo.Data.ID));
            ident.AddClaim(new Claim(ClaimTypes.Name, myInfo.Data.Attributes.Name));
            ident.AddClaim(new Claim(ClaimsExtensions.OrganizationID, myInfo.Meta.Parent.ID.ToString()));
            ident.AddClaim(new Claim(ClaimsExtensions.AccessToken, token.AccessToken));
            ident.AddClaim(new Claim(ClaimsExtensions.RefreshToken, token.RefreshToken));

            var principal = new System.Security.Claims.ClaimsPrincipal(ident);

            await this.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties()
            {
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1),
                AllowRefresh = true,
                IsPersistent = true
            });

            return RedirectToAction("Index");
        }

        public IActionResult PcoLoginComplete()
        {
            return RedirectToAction("Index");
        }       

        public async Task<IActionResult> Logout()
        {
            await this.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return Redirect(_pcoAuthOptions.LogoutUrl);
        }

    }
}
