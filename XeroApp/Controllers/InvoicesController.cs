using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Model.Accounting;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Net.Http;
using Xero.NetStandard.OAuth2.Token;
using Xero.NetStandard.OAuth2.Config;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace XeroApp.Controllers
{
    [Authorize]
    public class InvoicesController : Controller
    {
        XeroConfiguration XeroConfig = new XeroConfiguration
        {

            ClientId = "87E725915FC3482588723C390800810D",
            ClientSecret = "UCkVqSWzToFv-_VP59fo-od1Ze3vOeeJUpvcCCGQP7p9GP7F",
            CallbackUri = new Uri("https://localhost:5001/oauth/callback"),
            Scope = "openid profile email accounting.transactions offline_access",
            State = "123",
        };

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            Console.WriteLine("INSIDE HERE FINALLY");
            //var accessToken = await HttpContext.GetTokenAsync("access_token");
            //// Retrieve the Xero tenant ID from session
            var xeroTenantId = HttpContext.Session.GetString("XeroTenantId");
            var tokenJson = HttpContext.Session.GetString("token");

            if (tokenJson == null || xeroTenantId == null){
                // Log out the user by clearing the authentication cookie and session
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                HttpContext.Session.Clear();

                // Optionally, redirect the user to a login page or show a message
                return RedirectToAction("Login", "Authentication");
                
            }

            var xeroToken = JsonConvert.DeserializeObject<XeroOAuth2Token>(tokenJson);

            var utcTimeNow = DateTime.UtcNow;

            if (utcTimeNow > xeroToken.ExpiresAtUtc)
            {
                Console.WriteLine("TOKEN HAS EXPIRED -------");
                var client = new XeroClient(XeroConfig, new HttpClient());
                xeroToken = (XeroOAuth2Token)await client.RefreshAccessTokenAsync(xeroToken);
                var tokenJsonn = JsonConvert.SerializeObject(xeroToken);
                HttpContext.Session.SetString("token", tokenJsonn);
                
            }

            if (string.IsNullOrEmpty(xeroTenantId))
            {
                Console.WriteLine("TENANT ID IS NULL IN INVOICES INDEX -------");
                // Handle the case where the Xero tenant ID is not available
                return RedirectToAction("Index", "Home");
            }
            string accessToken = xeroToken.AccessToken;

            var accountingApi = new AccountingApi();
            var response = await accountingApi.GetInvoicesAsync(accessToken, xeroTenantId);

            Console.WriteLine("ALL IS WELLL");
            return View(response._Invoices);
        }
    }
}