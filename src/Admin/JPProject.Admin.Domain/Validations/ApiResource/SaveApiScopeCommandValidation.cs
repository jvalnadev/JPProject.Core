using JPProject.Admin.Domain.Commands.ApiResource;
using JPProject.Admin.Domain.Commands.ApiScope;

namespace JPProject.Admin.Domain.Validations.ApiResource
{
    public class SaveApiScopeCommandValidation : ApiScopeValidation<SaveApiScopeCommand>
    {
        public SaveApiScopeCommandValidation()
        {
            ValidateScopeName();
        }
    }
}