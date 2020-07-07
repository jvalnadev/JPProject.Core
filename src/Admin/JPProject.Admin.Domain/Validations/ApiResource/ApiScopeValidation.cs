using FluentValidation;
using JPProject.Admin.Domain.Commands.ApiScope;

namespace JPProject.Admin.Domain.Validations.ApiResource
{
    public abstract class ApiScopeValidation<T> : AbstractValidator<T> where T : ApiScopeCommand
    {

        protected void ValidateScopeName()
        {
            RuleFor(c => c.Name).NotEmpty().WithMessage("Invalid scope");
        }
    }
}