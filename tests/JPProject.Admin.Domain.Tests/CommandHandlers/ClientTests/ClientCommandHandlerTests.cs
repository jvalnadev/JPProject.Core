using Bogus;
using FluentAssertions;
using IdentityServer4.Models;
using JPProject.Admin.Domain.CommandHandlers;
using JPProject.Admin.Domain.Interfaces;
using JPProject.Admin.Fakers.Test.ClientFakers;
using JPProject.Domain.Core.Bus;
using JPProject.Domain.Core.Notifications;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace JPProject.Admin.Domain.Tests.CommandHandlers.ClientTests
{
    public class ClientCommandHandlerTests
    {
        private readonly ClientCommandHandler _commandHandler;
        private readonly Mock<DomainNotificationHandler> _notifications;
        private readonly Mock<IMediatorHandler> _mediator;
        private readonly Mock<IAdminUnitOfWork> _uow;
        private readonly Mock<IClientRepository> _clientRepository;
        private readonly CancellationTokenSource _tokenSource;
        private readonly Faker _faker;

        public ClientCommandHandlerTests()
        {
            _faker = new Faker();
            _tokenSource = new CancellationTokenSource();
            _uow = new Mock<IAdminUnitOfWork>();
            _mediator = new Mock<IMediatorHandler>();
            _notifications = new Mock<DomainNotificationHandler>();
            _clientRepository = new Mock<IClientRepository>();
            _commandHandler = new ClientCommandHandler(_uow.Object, _mediator.Object, _notifications.Object, _clientRepository.Object);

        }

        [Fact]
        public async Task ShouldNotAddDuplicatedClientId()
        {
            var command = ClientCommandFaker.GenerateSaveClientCommand().Generate();
            _clientRepository.Setup(s =>
                                    s.GetByClientId(It.Is<string>(clientId => clientId.Equals(command.Client.ClientId))))
                                     .ReturnsAsync(EntityClientFaker.GenerateClient().Generate());

            var result = await _commandHandler.Handle(command, _tokenSource.Token);

            result.Should().BeFalse();
            _uow.Verify(v => v.Commit(), Times.Never);
        }

        [Fact]
        public async Task ShouldNotUpdateClientWithPostLogoutUriWithTrailingSlash()
        {
            var command = ClientCommandFaker.GenerateUpdateClientCommand().Generate();
            command.Client.PostLogoutRedirectUris = new List<string>() { $"{_faker.Internet.Url()}/" };

            var result = await _commandHandler.Handle(command, _tokenSource.Token);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task ShouldClientHaveName()
        {
            var command = ClientCommandFaker.GenerateSaveClientCommand().Generate();
            command.Client.ClientName = string.Empty;

            var result = await _commandHandler.Handle(command, _tokenSource.Token);

            result.Should().BeFalse();
        }


        [Fact]
        public async Task ShouldClientHaveValidId()
        {
            var command = ClientCommandFaker.GenerateSaveClientCommand().Generate();
            command.Client.ClientId = string.Empty;

            var result = await _commandHandler.Handle(command, _tokenSource.Token);

            result.Should().BeFalse();
        }


        [Fact]
        public async Task ShouldNotCopySecrets()
        {
            var command = ClientCommandFaker.GenerateCopyClientCommand().Generate();

            var copyClient = EntityClientFaker.GenerateClient().Generate();

            _clientRepository.Setup(s =>
                    s.GetByClientId(It.Is<string>(clientId => clientId.Equals(command.Client.ClientId))))
                    .ReturnsAsync(copyClient);

            _clientRepository.Setup(s => s.Add(It.Is<Client>(c => !c.ClientSecrets.Any())));

            var result = await _commandHandler.Handle(command, _tokenSource.Token);

            result.Should().BeFalse();
        }


        [Theory]
        [InlineData(GrantType.Implicit, GrantType.AuthorizationCode)]
        [InlineData(GrantType.Implicit, GrantType.Hybrid)]
        [InlineData(GrantType.AuthorizationCode, GrantType.Hybrid)]
        public void ShouldNotAllowCombinationOfGrants(string a, string b)
        {
            var command = ClientCommandFaker.GenerateUpdateClientCommand().Generate();

            Assert.Throws<InvalidOperationException>(() => command.Client.AllowedGrantTypes = new List<string>() { a, b });
        }

        [Fact]
        public async Task ShouldNotAcceptNegativeAbsoluteRefreshTokenLifetime()
        {
            var command = ClientCommandFaker.GenerateUpdateClientCommand(absoluteRefreshTokenLifetime: _faker.Random.Int(max: 0)).Generate();

            var result = await _commandHandler.Handle(command, _tokenSource.Token);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task ShouldNotAcceptNegativeIdentityTokenLifetime()
        {
            var command = ClientCommandFaker.GenerateUpdateClientCommand(identityTokenLifetime: _faker.Random.Int(max: 0)).Generate();

            var result = await _commandHandler.Handle(command, _tokenSource.Token);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task ShouldNotAcceptNegativeAccessTokenLifetime()
        {
            var command = ClientCommandFaker.GenerateUpdateClientCommand(accessTokenLifetime: _faker.Random.Int(max: 0)).Generate();

            var result = await _commandHandler.Handle(command, _tokenSource.Token);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task ShouldNotAcceptNegativeAuthorizationCodeLifetime()
        {
            var command = ClientCommandFaker.GenerateUpdateClientCommand(authorizationCodeLifetime: _faker.Random.Int(max: 0)).Generate();

            var result = await _commandHandler.Handle(command, _tokenSource.Token);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task ShouldNotAcceptNegativeSlidingRefreshTokenLifetime()
        {
            var command = ClientCommandFaker.GenerateUpdateClientCommand(slidingRefreshTokenLifetime: _faker.Random.Int(max: 0)).Generate();
            var result = await _commandHandler.Handle(command, _tokenSource.Token);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task ShouldNotAcceptNegativeDeviceCodeLifetime()
        {
            var command = ClientCommandFaker.GenerateUpdateClientCommand(deviceCodeLifetime: _faker.Random.Int(max: 0)).Generate();

            var result = await _commandHandler.Handle(command, _tokenSource.Token);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task ShouldUpdateClient()
        {
            var oldClientId = "my-old-client-name";
            var command = ClientCommandFaker.GenerateUpdateClientCommand(oldClientId: oldClientId).Generate();
            _clientRepository.Setup(s => s.UpdateWithChildrens(It.Is<string>(s => s == oldClientId), It.Is<Client>(a => a.ClientId == command.Client.ClientId))).Returns(Task.CompletedTask);
            _clientRepository.Setup(s => s.GetClient(It.Is<string>(a => a == oldClientId))).ReturnsAsync(EntityClientFaker.GenerateClient().Generate());
            _uow.Setup(s => s.Commit()).ReturnsAsync(true);

            var result = await _commandHandler.Handle(command, _tokenSource.Token);


            _clientRepository.Verify(s => s.UpdateWithChildrens(It.Is<string>(s => s == oldClientId), It.IsAny<Client>()), Times.Once);
            _clientRepository.Verify(s => s.GetClient(It.Is<string>(q => q == oldClientId)), Times.Once);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldRemoveClient()
        {
            var command = ClientCommandFaker.GenerateRemoveClientCommand().Generate();
            _clientRepository.Setup(s => s.Remove(It.Is<Client>(a => a.ClientId == command.Client.ClientId)));
            _clientRepository.Setup(s => s.GetByClientId(It.Is<string>(a => a == command.Client.ClientId))).ReturnsAsync(EntityClientFaker.GenerateClient().Generate());
            _uow.Setup(s => s.Commit()).ReturnsAsync(true);

            var result = await _commandHandler.Handle(command, _tokenSource.Token);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldNotRemoveSecretWhenClientDoesntExist()
        {
            var command = ClientCommandFaker.GenerateRemoveClientSecretCommand().Generate();

            var result = await _commandHandler.Handle(command, _tokenSource.Token);

            result.Should().BeFalse();
            _uow.Verify(v => v.Commit(), Times.Never);
        }

        [Fact]
        public async Task ShouldNotRemoveSecretWhenSecretIdIsDifferent()
        {
            var command = ClientCommandFaker.GenerateRemoveClientSecretCommand().Generate();
            _clientRepository.Setup(s => s.GetClient(It.Is<string>(a => a == command.ClientId))).ReturnsAsync(EntityClientFaker.GenerateClient().Generate());

            var result = await _commandHandler.Handle(command, _tokenSource.Token);

            result.Should().BeFalse();
            _uow.Verify(v => v.Commit(), Times.Never);
        }

        [Fact]
        public async Task ShouldRemoveClientSecret()
        {
            var clientSecret = EntityClientFaker.GenerateClient(clientSecrets: _faker.Random.Int(1, 3)).Generate();
            var randomClientSecret = _faker.PickRandom(clientSecret.ClientSecrets);
            var command = ClientCommandFaker.GenerateRemoveClientSecretCommand(randomClientSecret.Type, randomClientSecret.Value).Generate();

            _uow.Setup(s => s.Commit()).ReturnsAsync(true);
            _clientRepository.Setup(s => s.GetClient(It.Is<string>(a => a == command.ClientId))).ReturnsAsync(clientSecret);

            var result = await _commandHandler.Handle(command, _tokenSource.Token);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldNotSaveClientSecretWhenClientDoesntExist()
        {
            var command = ClientCommandFaker.GenerateSaveClientSecretCommand().Generate();
            _clientRepository.Setup(s => s.GetClient(It.Is<string>(a => a == command.ClientId))).ReturnsAsync(EntityClientFaker.GenerateClient().Generate());

            var result = await _commandHandler.Handle(command, _tokenSource.Token);

            result.Should().BeFalse();
            _clientRepository.Verify(s => s.GetByClientId(It.Is<string>(a => a == command.ClientId)), Times.Once);
            _uow.Verify(v => v.Commit(), Times.Never);
        }

        [Fact]
        public async Task ShouldEncryptedValueBeCorrect()
        {
            var command = ClientCommandFaker.GenerateSaveClientSecretCommand().Generate();
            var valueEncryptedMustBe = command.GetValue();

            _clientRepository.Setup(s => s.GetByClientId(It.Is<string>(a => a == command.ClientId))).ReturnsAsync(EntityClientFaker.GenerateClient().Generate());
            _clientRepository.Setup(s => s.AddSecret(It.Is<string>(s => s == command.ClientId), It.Is<Secret>(cs => cs.Value == valueEncryptedMustBe)));
            _uow.Setup(s => s.Commit()).ReturnsAsync(true);


            var result = await _commandHandler.Handle(command, _tokenSource.Token);

            result.Should().BeTrue();
            _clientRepository.Verify(s => s.GetByClientId(It.Is<string>(a => a == command.ClientId)), Times.Once);
            _clientRepository.Verify(s => s.AddSecret(It.Is<string>(s => s == command.ClientId), It.Is<Secret>(cs => cs.Value == valueEncryptedMustBe)), Times.Once);
            _uow.Verify(s => s.Commit(), Times.Once);
        }

        [Fact]
        public async Task ShouldNotEncryptedValueBeCorrect()
        {
            var command = ClientCommandFaker.GenerateSaveClientSecretCommand().Generate();
            var valueEncryptedMustBe = command.GetValue();

            _clientRepository.Setup(s => s.GetByClientId(It.Is<string>(a => a == command.ClientId))).ReturnsAsync(EntityClientFaker.GenerateClient().Generate());
            _clientRepository.Setup(s => s.AddSecret(It.Is<string>(s => s == command.ClientId), It.Is<Secret>(cs => cs.Value == valueEncryptedMustBe)));
            _uow.Setup(s => s.Commit()).ReturnsAsync(true);


            var result = await _commandHandler.Handle(command, _tokenSource.Token);

            result.Should().BeTrue();
            _clientRepository.Verify(s => s.GetByClientId(It.Is<string>(a => a == command.ClientId)), Times.Once);
            _clientRepository.Verify(s => s.AddSecret(It.Is<string>(s => s == command.ClientId), It.Is<Secret>(cs => cs.Value == valueEncryptedMustBe)), Times.Once);
            _uow.Verify(s => s.Commit(), Times.Once);
        }

        [Fact]
        public async Task ShouldNotRemovePropertyWhenClientDoesntExist()
        {
            var command = ClientCommandFaker.GenerateRemovePropertyCommand().Generate();


            var result = await _commandHandler.Handle(command, _tokenSource.Token);

            result.Should().BeFalse();
            _clientRepository.Verify(s => s.GetClient(It.Is<string>(a => a == command.ClientId)), Times.Once);
        }


        [Fact]
        public async Task ShouldNotRemovePropertyWhenIdIsDifferent()
        {
            var command = ClientCommandFaker.GenerateRemovePropertyCommand().Generate();
            _clientRepository.Setup(s => s.GetClient(It.Is<string>(a => a == command.ClientId))).ReturnsAsync(EntityClientFaker.GenerateClient().Generate());

            var result = await _commandHandler.Handle(command, _tokenSource.Token);

            result.Should().BeFalse();
            _clientRepository.Verify(s => s.GetClient(It.Is<string>(a => a == command.ClientId)), Times.Once);
        }

        [Fact]
        public async Task ShouldRemoveProperty()
        {
            var properties = EntityClientFaker.GenerateClient(clientProperties: _faker.Random.Int(1, 3)).Generate();
            var command = ClientCommandFaker.GenerateRemovePropertyCommand(_faker.PickRandom(properties.Properties).First().Key).Generate();

            _uow.Setup(s => s.Commit()).ReturnsAsync(true);
            _clientRepository.Setup(s => s.GetClient(It.Is<string>(a => a == command.ClientId))).ReturnsAsync(properties);
            _clientRepository.Setup(s => s.RemoveProperty(It.Is<string>(a => a == command.ClientId), It.IsAny<string>(), It.IsAny<string>()));

            var result = await _commandHandler.Handle(command, _tokenSource.Token);

            result.Should().BeTrue();
            _clientRepository.Verify(s => s.GetClient(It.Is<string>(a => a == command.ClientId)), Times.Once);
            _clientRepository.Verify(s => s.RemoveProperty(It.Is<string>(a => a == command.ClientId), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }


        [Fact]
        public async Task ShouldNotSavePropertyWhenClientDoesntExist()
        {
            var command = ClientCommandFaker.GenerateSavePropertyCommand().Generate();
            _clientRepository.Setup(s => s.GetByClientId(It.Is<string>(q => q == command.ClientId))).ReturnsAsync((Client)null);


            var result = await _commandHandler.Handle(command, _tokenSource.Token);


            result.Should().BeFalse();
            _clientRepository.Verify(s => s.GetByClientId(It.Is<string>(q => q == command.ClientId)), Times.Once);
        }

        [Fact]
        public async Task ShouldSaveProperty()
        {
            var command = ClientCommandFaker.GenerateSavePropertyCommand().Generate();
            _clientRepository.Setup(s => s.GetByClientId(It.Is<string>(q => q == command.ClientId))).ReturnsAsync(EntityClientFaker.GenerateClient().Generate()).Verifiable();
            _clientRepository.Setup(s => s.AddProperty(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            var result = await _commandHandler.Handle(command, _tokenSource.Token);

            _clientRepository.Verify(s => s.AddProperty(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _clientRepository.Verify(s => s.GetByClientId(It.Is<string>(q => q == command.ClientId)), Times.Once);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ShouldNotRemoveClaimWhenClientDoesntExist()
        {
            var command = ClientCommandFaker.GenerateRemoveClaimCommand().Generate();


            var result = await _commandHandler.Handle(command, _tokenSource.Token);

            result.Should().BeFalse();
            _clientRepository.Verify(s => s.GetClient(It.Is<string>(a => a == command.ClientId)), Times.Once);
        }


        [Fact]
        public async Task ShouldNotRemoveClaimWhenIdIsDifferent()
        {
            var command = ClientCommandFaker.GenerateRemoveClaimCommand().Generate();
            _clientRepository.Setup(s => s.GetClient(It.Is<string>(a => a == command.ClientId))).ReturnsAsync(EntityClientFaker.GenerateClient().Generate());

            var result = await _commandHandler.Handle(command, _tokenSource.Token);

            result.Should().BeFalse();
            _clientRepository.Verify(s => s.GetClient(It.Is<string>(a => a == command.ClientId)), Times.Once);
        }

        [Fact]
        public async Task ShouldRemoveClaimByTypeAndValue()
        {
            var properties = EntityClientFaker.GenerateClient(clientClaim: _faker.Random.Int(1, 3)).Generate();
            var randomClientSecret = _faker.PickRandom(properties.Claims);
            var command = ClientCommandFaker.GenerateRemoveClaimCommand(randomClientSecret.Type, randomClientSecret.Value).Generate();

            _uow.Setup(s => s.Commit()).ReturnsAsync(true);
            _clientRepository.Setup(s => s.GetClient(It.Is<string>(a => a == command.ClientId))).ReturnsAsync(properties);
            _clientRepository.Setup(s => s.RemoveClaim(It.Is<string>(q => q == command.ClientId), It.Is<string>(a => a == command.Type), It.Is<string>(s => s == command.Value)));

            var result = await _commandHandler.Handle(command, _tokenSource.Token);

            result.Should().BeTrue();
            _clientRepository.Verify(s => s.GetClient(It.Is<string>(a => a == command.ClientId)), Times.Once);
            _clientRepository.Verify(s => s.RemoveClaim(It.Is<string>(q => q == command.ClientId), It.Is<string>(a => a == command.Type), It.Is<string>(s => s == command.Value)), Times.Once);
        }

        [Fact]
        public async Task ShouldRemoveClaimByType()
        {
            var properties = EntityClientFaker.GenerateClient(clientClaim: _faker.Random.Int(1, 3)).Generate();
            var randomClientSecret = _faker.PickRandom(properties.Claims);
            var command = ClientCommandFaker.GenerateRemoveClaimCommand(randomClientSecret.Type, "").Generate();

            _uow.Setup(s => s.Commit()).ReturnsAsync(true);
            _clientRepository.Setup(s => s.GetClient(It.Is<string>(a => a == command.ClientId))).ReturnsAsync(properties);
            _clientRepository.Setup(s => s.RemoveClaim(It.Is<string>(q => q == command.ClientId), It.Is<string>(a => a == command.Type)));

            var result = await _commandHandler.Handle(command, _tokenSource.Token);

            result.Should().BeTrue();
            _clientRepository.Verify(s => s.GetClient(It.Is<string>(a => a == command.ClientId)), Times.Once);
            _clientRepository.Verify(s => s.RemoveClaim(It.Is<string>(q => q == command.ClientId), It.Is<string>(a => a == command.Type)), Times.Once);
        }

        [Fact]
        public async Task ShouldNotSaveClaimWhenClientDoesntExist()
        {
            var command = ClientCommandFaker.GenerateSaveClaimCommand().Generate();
            _clientRepository.Setup(s => s.GetByClientId(It.Is<string>(q => q == command.ClientId))).ReturnsAsync((Client)null);


            var result = await _commandHandler.Handle(command, _tokenSource.Token);


            result.Should().BeFalse();
            _clientRepository.Verify(s => s.GetByClientId(It.Is<string>(q => q == command.ClientId)), Times.Once);
        }

        [Fact]
        public async Task ShouldSaveClaim()
        {
            var command = ClientCommandFaker.GenerateSaveClaimCommand().Generate();
            _clientRepository.Setup(s => s.GetByClientId(It.Is<string>(q => q == command.ClientId))).ReturnsAsync(EntityClientFaker.GenerateClient().Generate()).Verifiable();
            _clientRepository.Setup(s => s.AddClaim(It.Is<string>(q => q == command.ClientId), It.IsAny<Claim>()));

            var result = await _commandHandler.Handle(command, _tokenSource.Token);

            _clientRepository.Verify(s => s.AddClaim(It.Is<string>(q => q == command.ClientId), It.IsAny<Claim>()), Times.Once);
            _clientRepository.Verify(s => s.GetByClientId(It.Is<string>(q => q == command.ClientId)), Times.Once);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ShouldNotSaveClientWithPostLogoutUriWithTrailingSlash()
        {
            var command = ClientCommandFaker.GenerateSaveClientCommand(postLogoutUri: $"{_faker.Internet.Url()}/").Generate();
            var result = await _commandHandler.Handle(command, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task ShouldNotSaveClientWithBlankSpace()
        {
            var command = ClientCommandFaker.GenerateSaveClientCommand(clientId: "test  ").Generate();
            _uow.Setup(s => s.Commit()).ReturnsAsync(true);

            var result = await _commandHandler.Handle(command, _tokenSource.Token);

            _uow.Verify(s => s.Commit(), Times.Once);

            result.Should().BeTrue();

            command.ToModel().ClientId.Should().Be("test");
        }

    }
}
