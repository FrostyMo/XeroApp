
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Models;
using Xero.NetStandard.OAuth2.Token;



public class AuthenticationController : Controller
{
    
    XeroConfiguration XeroConfig = new XeroConfiguration
    {
        
        ClientId = "87E725915FC3482588723C390800810D",
        ClientSecret = "UCkVqSWzToFv-_VP59fo-od1Ze3vOeeJUpvcCCGQP7p9GP7F",
        CallbackUri = new Uri("https://localhost:5001/oauth/callback"),
        Scope = "openid profile email accounting.transactions offline_access",
        State = "123",
    };

    
    private readonly ILogger<AuthenticationController> _logger;
    private readonly HttpClient _clientFactory;

    public AuthenticationController(ILogger<AuthenticationController> logger)
    {
        _logger = logger;
        _clientFactory = new HttpClient();
    }
    

    public IActionResult Login()
    {

        // Redirect user to Xero for authentication
        var client = new XeroClient(XeroConfig, _clientFactory);
        var loginUrl = client.BuildLoginUri();
        
        Console.WriteLine(loginUrl.ToString());
        return Redirect(loginUrl.ToString());
    }

    [Route("oauth/callback")]
    public async Task<IActionResult> Callback(string code, string state)
    {
        
        var client = new XeroClient(XeroConfig, _clientFactory);
        var xeroToken = (XeroOAuth2Token)await client.RequestAccessTokenAsync(code);
        
        Console.WriteLine(xeroToken);
        List<Tenant> tenants = await client.GetConnectionsAsync(xeroToken);

        //if (xeroToken == null)
        //{
        //    return RedirectToAction("Index", "Home");
        //}

        Tenant firstTenant = tenants[0];
        var tokenJson = JsonConvert.SerializeObject(xeroToken);
        HttpContext.Session.SetString("token", tokenJson);
        if (firstTenant != null) {
            HttpContext.Session.SetString("XeroTenantId", firstTenant.TenantId.ToString());
            
            Console.WriteLine(firstTenant.TenantId);

            // Create the claims identity
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, xeroToken.IdToken),
                new Claim(ClaimTypes.Name, firstTenant.TenantName), // Adjust based on actual user info
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties { IsPersistent = true });

            return RedirectToAction("Index", "Invoices");
        }
        
        return RedirectToAction("Index", "Home");
    }


    /*

    CAN USE THE FOLLOWING LOGIC TO GET SECRET FROM CONFIG.

     */

    //public class XeroOAuthSettings
    //{
    //    public string? ClientId { get; set; }
    //    public string? ClientSecret { get; set; }
    //    public string? CallbackUri { get; set; }
    //    public string? Scope { get; set; }
    //}
    //// Load Xero settings from appsettings.json
    //var xeroSettings = Configuration.GetSection("XeroSettings").Get<XeroOAuthSettings>();


    public async Task<ActionResult> Disconnect()
    {
        var client = new XeroClient(XeroConfig, _clientFactory);

        var tokenJson = HttpContext.Session.GetString("token");

        if (tokenJson == null)
        {
            return RedirectToAction("Index", "Home");
        }
        var xeroToken = JsonConvert.DeserializeObject<XeroOAuth2Token>(tokenJson);

        var utcTimeNow = DateTime.UtcNow;

        if (utcTimeNow > xeroToken.ExpiresAtUtc)
        {
            Console.WriteLine("TOKEN HAS EXPIRED -------");

            xeroToken = (XeroOAuth2Token)await client.RefreshAccessTokenAsync(xeroToken);

        }
        
        Tenant xeroTenant = xeroToken.Tenants[0];
        await client.DeleteConnectionAsync(xeroToken, xeroTenant);

        return RedirectToAction("Index", "Home");
    }

}