using JPProject.Admin.Domain.Commands.Clients;

namespace JPProject.Admin.Domain.Validations.Client
{
    public class UpdateClientCommandValidation : ClientValidation<UpdateClientCommand>
    {
        public UpdateClientCommandValidation()
        {
            ValidateGrantType();
            ValidateOldClientId();
            ValidateIdentityTokenLifetime();
            ValidateAccessTokenLifetime();
            ValidateAuthorizationCodeLifetime();
            ValidateSlidingRefreshTokenLifetime();
            ValidateDeviceCodeLifetime();
            ValidateAbsoluteRefreshTokenLifetime();
            ValidatePostLogoutTrailingSlash();
            ValidateClientUriTrailingSlash();
        }
    }
}