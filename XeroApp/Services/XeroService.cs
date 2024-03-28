using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xero.NetStandard.OAuth2.Models;


namespace XeroApp.Services
{
    public class TenantResponse
    {
        public List<Tenant> Tenants { get; set; }
    }

    public class XeroService : IXeroService
    {
        public async Task<List<Tenant>> GetXeroTenants(string accessToken)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await client.GetAsync("https://api.xero.com/connections");
            Console.WriteLine("Hi---------");
            Console.WriteLine(response);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            // Adjusting deserialization to use the wrapper class
            var tenantResponse = JsonConvert.DeserializeObject<TenantResponse>(jsonResponse);
            return tenantResponse.Tenants; // Extracting the list from the wrapper
        }

        
    }
}