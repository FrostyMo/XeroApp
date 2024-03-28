using System.Collections.Generic;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Models;

namespace XeroApp.Services
{
    public interface IXeroService
    {
        Task<List<Tenant>> GetXeroTenants(string accessToken);
    }
}