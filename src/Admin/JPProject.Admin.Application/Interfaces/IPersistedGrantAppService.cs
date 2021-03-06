using JPProject.Admin.Application.ViewModels;
using JPProject.Domain.Core.ViewModels;
using System;
using System.Threading.Tasks;

namespace JPProject.Admin.Application.Interfaces
{
    public interface IPersistedGrantAppService : IDisposable
    {
        Task<ListOf<PersistedGrantViewModel>> GetPersistedGrants(IPersistedGrantCustomSearch search);
        Task Remove(RemovePersistedGrantViewModel model);
    }
}