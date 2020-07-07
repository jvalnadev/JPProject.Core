using JPProject.Admin.Domain.Commands.ApiResource;
using JPProject.Admin.Domain.Commands.ApiScope;

namespace JPProject.Admin.Domain.Validations.ApiResource
{
    public class RemoveApiScopeCommandValidation : ApiScopeValidation<RemoveApiScopeCommand>
    {
        public RemoveApiScopeCommandValidation()
        {
            ValidateScopeName();
        }
    }
}