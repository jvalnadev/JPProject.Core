using System;
using System.Threading.Tasks;
using JPProject.Admin.Application.ViewModels.ApiScopeViewModels;

namespace JPProject.Admin.Application.Interfaces
{
    public interface IApiScopeAppService : IDisposable
    {
        Task<bool> RemoveScope(RemoveApiScopeViewModel model);
        Task<bool> SaveScope(SaveApiScopeViewModel model);
    }
}