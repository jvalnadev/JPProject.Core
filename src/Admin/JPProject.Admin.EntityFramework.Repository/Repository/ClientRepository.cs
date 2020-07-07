using FluentValidation;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Mappers;
using JPProject.Admin.Domain.Interfaces;
using JPProject.Domain.Core.Util;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static JPProject.Admin.EntityFramework.Repository.Repository.ModelMappers;


namespace JPProject.Admin.EntityFramework.Repository.Repository
{
    public class ClientRepository : IClientRepository
    {
        private readonly IConfigurationDbContext _context;
        public DbSet<IdentityServer4.EntityFramework.Entities.Client> DbSet { get; set; }
        public ClientRepository(IConfigurationDbContext context)
        {
            _context = context;
            this.DbSet = context.Clients;
        }

        public async Task<IdentityServer4.Models.Client> GetClient(string clientId)
        {
            var client = await GetFullClient(clientId);

            return client.ToModel();
        }

        public Task<List<string>> ListClients()
        {
            return DbSet.Select(s => s.ClientId).ToListAsync();
        }

        public async Task<IdentityServer4.Models.Client> GetByClientId(string clientId)
        {
            var client = await DbSet.Where(x => x.ClientId == clientId).SingleOrDefaultAsync();
            return client.ToModel();
        }

        public async Task<IdentityServer4.Models.Client> GetClientDefaultDetails(string clientId)
        {
            var client = await DbSet
                .Include(x => x.AllowedGrantTypes)
                .Include(x => x.RedirectUris)
                .Include(x => x.PostLogoutRedirectUris)
                .Include(x => x.AllowedScopes)
                .Include(x => x.IdentityProviderRestrictions)
                .Include(x => x.AllowedCorsOrigins)
                .Where(x => x.ClientId == clientId)
                .SingleOrDefaultAsync();
            return client.ToModel();
        }

        public async Task RemoveSecret(string clientId, IdentityServer4.Models.Secret secret)
        {
            var client = await DbSet.Include(s => s.ClientSecrets).SingleOrDefaultAsync(s => s.ClientId == clientId);
            client.ClientSecrets.RemoveAll(r => r.Type == secret.Type && r.Value == secret.Value);
        }

        public async Task AddSecret(string clientId, IdentityServer4.Models.Secret secret)
        {
            var client = await FindClient(clientId).Include(s => s.ClientSecrets).FirstOrDefaultAsync();
            var entity = Mapper.Map<IdentityServer4.EntityFramework.Entities.ClientSecret>(secret);
            client.ClientSecrets.Add(entity);
            DbSet.Update(client);

        }

        public async Task RemoveProperty(string clientId, string key, string value)
        {
            var client = await FindClient(clientId).Include(i => i.Properties).FirstOrDefaultAsync();
            client.Properties.RemoveAll(r => r.Key == key && r.Value == value);
            DbSet.Update(client);
        }

        public async Task AddProperty(string clientId, string key, string value)
        {
            var client = await FindClient(clientId).Include(i => i.Properties).FirstOrDefaultAsync();
            client.Properties.Add(new IdentityServer4.EntityFramework.Entities.ClientProperty() { Key = key, Value = value });
            DbSet.Update(client);
        }

        public async Task RemoveClaim(string clientId, string type)
        {
            var client = await FindClient(clientId).Include(i => i.Claims).FirstOrDefaultAsync();
            client.Properties.RemoveAll(r => r.Key == type);
            DbSet.Update(client);
        }

        public async Task RemoveClaim(string clientId, string type, string value)
        {
            var client = await FindClient(clientId).Include(i => i.Claims).FirstOrDefaultAsync();
            client.Claims.RemoveAll(r => r.Type == type && r.Value == value);
            DbSet.Update(client);
        }

        public async Task AddClaim(string clientId, IdentityServer4.Models.ClientClaim claim)
        {
            var client = await FindClient(clientId).Include(i => i.Claims).FirstOrDefaultAsync();
            var entity = Mapper.Map<ClientClaim>(claim);
            client.Claims.Add(entity);
            DbSet.Update(client);
        }

        public async Task<IEnumerable<IdentityServer4.Models.Secret>> GetSecrets(string clientId)
        {
            var client = await FindClient(clientId).Include(i => i.ClientSecrets).FirstOrDefaultAsync();
            return Mapper.Map<IEnumerable<IdentityServer4.Models.Secret>>(client.ClientSecrets);
        }

        public async Task<IEnumerable<IdentityServer4.Models.Client>> All()
        {
            var clients = await DbSet.OrderBy(a => a.ClientName).ToListAsync();
            return clients.Select(s => s.ToModel());
        }

        public async Task<IDictionary<string, string>> GetProperties(string clientId)
        {
            var client = await FindClient(clientId).Include(i => i.Properties).FirstOrDefaultAsync();
            return Mapper.Map<IDictionary<string, string>>(client.Properties);
        }

        public async Task<IEnumerable<IdentityServer4.Models.ClientClaim>> GetClaims(string clientId)
        {
            var client = await FindClient(clientId).Include(c => c.Claims).FirstOrDefaultAsync();
            return Mapper.Map<IEnumerable<IdentityServer4.Models.ClientClaim>>(client.Claims);
        }

        private IQueryable<IdentityServer4.EntityFramework.Entities.Client> FindClient(string clientId)
        {
            return DbSet.Where(f => f.ClientId == clientId);
        }

        public async Task UpdateWithChildrens(string oldClientId, IdentityServer4.Models.Client client)
        {
            var newClient = client.ToEntity();
            var clientDb = await GetFullClient(oldClientId);

            newClient.Id = clientDb.Id;
            newClient.ShallowCopyTo(clientDb);
        }


        private async Task<IdentityServer4.EntityFramework.Entities.Client> RemoveClientRelationsAsync(string clientId)
        {
            var clientDb = await DbSet
                                .Include(s => s.AllowedScopes)
                                .Include(s => s.AllowedCorsOrigins)
                                .Include(s => s.AllowedGrantTypes)
                                .Include(s => s.RedirectUris)
                                .Include(s => s.AllowedCorsOrigins)
                                .Include(s => s.IdentityProviderRestrictions)
                                .Include(s => s.PostLogoutRedirectUris)
                                .Where(x => x.ClientId == clientId)
                                .FirstAsync();

            clientDb.AllowedScopes?.Clear();
            clientDb.AllowedGrantTypes?.Clear();
            clientDb.RedirectUris?.Clear();
            clientDb.AllowedCorsOrigins?.Clear();
            clientDb.IdentityProviderRestrictions?.Clear();
            clientDb.PostLogoutRedirectUris?.Clear();
            return clientDb;
        }

        public void Add(IdentityServer4.Models.Client obj)
        {
            DbSet.Add(obj.ToEntity());
        }

        public void Update(IdentityServer4.Models.Client obj)
        {
            var client = DbSet.FirstOrDefault(w => w.ClientId == obj.ClientId);
            var newOne = obj.ToEntity();
            newOne.Id = client.Id;
            DbSet.Update(newOne);
        }

        public void Remove(IdentityServer4.Models.Client obj)
        {
            var client = DbSet.FirstOrDefault(w => w.ClientId == obj.ClientId);
            DbSet.Remove(client);
        }

        private async Task<IdentityServer4.EntityFramework.Entities.Client> GetFullClient(string clientId)
        {
            return await DbSet
                .Include(x => x.AllowedGrantTypes)
                .Include(x => x.RedirectUris)
                .Include(x => x.PostLogoutRedirectUris)
                .Include(x => x.AllowedScopes)
                .Include(x => x.IdentityProviderRestrictions)
                .Include(x => x.AllowedCorsOrigins)
                .Include(x => x.ClientSecrets)
                .Include(x => x.Claims)
                .Include(x => x.Properties)
                .Where(x => x.ClientId == clientId)
                .SingleOrDefaultAsync();
        }

        public void Dispose()
        {
        }
    }
}
