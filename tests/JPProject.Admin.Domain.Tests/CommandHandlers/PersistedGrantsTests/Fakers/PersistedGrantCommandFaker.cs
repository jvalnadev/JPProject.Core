using Bogus;
using IdentityServer4.Models;
using JPProject.Admin.Application.ViewModels;
using JPProject.Admin.Domain.Commands.PersistedGrant;

namespace JPProject.Admin.Domain.Tests.CommandHandlers.PersistedGrantsTests.Fakers
{
    public class PersistedGrantCommandFaker
    {
        public static Faker<RemovePersistedGrantCommand> GenerateRemoveCommand(string name = null)
        {
            return new Faker<RemovePersistedGrantCommand>().CustomInstantiator(c => new RemovePersistedGrantCommand(c.Company.CompanyName()));
        }
    }
    public class PersistedGrantFaker
    {
        public static Faker<PersistedGrantViewModel> GeneratePersstedGrantViewModel(string key = null)
        {
            return new Faker<PersistedGrantViewModel>().CustomInstantiator(
                    f => new PersistedGrantViewModel(
                        key ?? f.Random.String(10, 20),
                        f.Lorem.Word(),
                        f.Random.Guid().ToString(),
                        f.Internet.DomainName(),
                        f.Date.Recent(),
                        f.Date.Future(),
                        f.Lorem.Word()
                        )
                    );
        }
        public static Faker<PersistedGrant> GeneratePersstedGrant(string key = null)
        {
            return new Faker<PersistedGrant>()
                .RuleFor(p => p.Key, f => f.Lorem.Word())
                .RuleFor(p => p.Type, f => f.Lorem.Word())
                .RuleFor(p => p.SubjectId, f => f.Lorem.Word())
                .RuleFor(p => p.ClientId, f => f.Lorem.Word())
                .RuleFor(p => p.CreationTime, f => f.Date.Past())
                .RuleFor(p => p.Expiration, f => default)
                .RuleFor(p => p.Data, f => f.Lorem.Word());
        }
    }
}
