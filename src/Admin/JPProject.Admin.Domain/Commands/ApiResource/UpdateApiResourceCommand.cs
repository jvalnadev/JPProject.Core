using JPProject.Admin.Domain.Validations.ApiResource;

namespace JPProject.Admin.Domain.Commands.ApiResource
{
    public class UpdateApiResourceCommand : ApiResourceCommand
    {

        public UpdateApiResourceCommand(IdentityServer4.Models.ApiResource apiResource, string oldResourceName)
        {
            OldResourceName = oldResourceName;
            ApiResource = apiResource;
        }

        public override bool IsValid()
        {
            ValidationResult = new UpdateApiResourceCommandValidation().Validate(this);
            return ValidationResult.IsValid;
        }
    }
}