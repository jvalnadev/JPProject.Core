using JPProject.Admin.Domain.Commands.ApiScope;
using JPProject.Admin.Domain.Events.ApiResource;
using JPProject.Admin.Domain.Interfaces;
using JPProject.Domain.Core.Bus;
using JPProject.Domain.Core.Commands;
using JPProject.Domain.Core.Interfaces;
using JPProject.Domain.Core.Notifications;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace JPProject.Admin.Domain.CommandHandlers
{
    public class ApiScopeCommandHandler : CommandHandler,
        IRequestHandler<RemoveApiScopeCommand, bool>,
        IRequestHandler<SaveApiScopeCommand, bool>
    {
        private readonly IApiScopeRepository _apiScopeRepository;

        public ApiScopeCommandHandler(
            IUnitOfWork uow,
            IMediatorHandler bus,
            INotificationHandler<DomainNotification> notifications,
            IApiScopeRepository apiScopeRepository) : base(uow, bus, notifications)
        {
            _apiScopeRepository = apiScopeRepository;
        }



        public async Task<bool> Handle(RemoveApiScopeCommand request, CancellationToken cancellationToken)
        {
            if (!request.IsValid())
            {
                NotifyValidationErrors(request);
                return false;
            }

            var savedClient = await _apiScopeRepository.Get(request.Name);
            if (savedClient == null)
            {
                await Bus.RaiseEvent(new DomainNotification("Api Scope", "Scope not found"));
                return false;
            }

            _apiScopeRepository.RemoveScope(request.Name);

            if (await Commit())
            {
                await Bus.RaiseEvent(new ApiScopeRemovedEvent(request.Name));
                return true;
            }
            return false;
        }

        public async Task<bool> Handle(SaveApiScopeCommand request, CancellationToken cancellationToken)
        {
            if (!request.IsValid())
            {
                NotifyValidationErrors(request);
                return false;
            }

            var savedApi = await _apiScopeRepository.Get(request.Name);
            if (savedApi == null)
            {
                await Bus.RaiseEvent(new DomainNotification("Api", "Api not found"));
                return false;
            }

            _apiScopeRepository.Add(request.ToModel());

            if (await Commit())
            {
                await Bus.RaiseEvent(new ApiScopeSavedEvent(request.Name, request));
                return true;
            }
            return false;
        }
    }
}