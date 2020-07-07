using IdentityServer4.Models;
using JPProject.Domain.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JPProject.Admin.Domain.Interfaces
{
    public interface IApiScopeRepository : IRepository<ApiScope>
    {
        void RemoveScope(string name);
        Task<IEnumerable<ApiScope>> GetScopesByResource(string name);
        Task<List<string>> ListScopes();
        Task<ApiScope> Get(string scopeName);
    }
}