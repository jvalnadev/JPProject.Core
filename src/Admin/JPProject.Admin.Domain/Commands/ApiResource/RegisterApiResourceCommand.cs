using FluentValidation.Results;
using JPProject.Admin.Domain.Validations.ApiResource;
using System.Linq;

namespace JPProject.Admin.Domain.Commands.ApiResource
{
    public class RegisterApiResourceCommand : ApiResourceCommand
    {
        public RegisterApiResourceCommand(IdentityServer4.Models.ApiResource apiApiResource)
        {
            ApiResource = apiApiResource;
        }

        public override bool IsValid()
        {
            ValidationResult = new RegisterApiResourceCommandValidation().Validate(this);
            return ValidationResult.IsValid;
        }

        public IdentityServer4.Models.ApiResource ToModel()
        {
            var scopes = ApiResource.Scopes.ToList();
            scopes.Add(ApiResource.Name);
            ApiResource.Scopes = scopes;
            return ApiResource;
        }
    }
}