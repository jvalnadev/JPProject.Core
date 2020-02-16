using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Options;
using JPProject.Admin.Domain.Interfaces;
using JPProject.Admin.Infra.Data.Context;
using JPProject.Admin.Infra.Data.Interfaces;
using JPProject.Admin.Infra.Data.Repository;
using JPProject.Admin.Infra.Data.UoW;
using JPProject.Domain.Core.Events;
using JPProject.Domain.Core.Interfaces;
using JPProject.EntityFrameworkCore.EventSourcing;
using JPProject.EntityFrameworkCore.Interfaces;
using JPProject.EntityFrameworkCore.Repository;
using Microsoft.EntityFrameworkCore;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ContextConfiguration
    {
        public static IJpProjectAdminBuilder AddAdminContext(this IJpProjectAdminBuilder services, Action<DbContextOptionsBuilder> optionsAction, JpDatabaseOptions options = null)
        {
            if (options == null)
                options = new JpDatabaseOptions();

            RegisterStorageServices(services.Services);
            ConfigureContexts(services, optionsAction);

            //ContextHelpers.CheckDatabases(services.BuildServiceProvider(), options).Wait();

            return services;
        }

        private static void ConfigureContexts(IJpProjectAdminBuilder services, Action<DbContextOptionsBuilder> optionsAction)
        {
            var operationalStoreOptions = new OperationalStoreOptions();
            services.Services.AddSingleton(operationalStoreOptions);

            var storeOptions = new ConfigurationStoreOptions();
            services.Services.AddSingleton(storeOptions);

            services.Services.AddDbContext<JPProjectAdminUIContext>(optionsAction);
        }

        private static void RegisterStorageServices(IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IEventStoreRepository, EventStoreRepository>();
            services.AddScoped<IAdminContext>(x => x.GetService<JPProjectAdminUIContext>());

            services.AddScoped<IConfigurationDbContext>(x => x.GetService<JPProjectAdminUIContext>());
            services.AddScoped<IPersistedGrantDbContext>(x => x.GetService<JPProjectAdminUIContext>());

            services.AddScoped<IConfigurationDbStore>(x => x.GetService<JPProjectAdminUIContext>());
            services.AddScoped<IPersistedGrantDbStore>(x => x.GetService<JPProjectAdminUIContext>());
            services.AddScoped<IEventStore, SqlEventStore>();

            services.AddScoped<IPersistedGrantRepository, PersistedGrantRepository>();
            services.AddScoped<IApiResourceRepository, ApiResourceRepository>();
            services.AddScoped<IIdentityResourceRepository, IdentityResourceRepository>();
            services.AddScoped<IClientRepository, ClientRepository>();

            services.AddScoped<IAdminUnitOfWork, UnitOfWork>();
            services.AddScoped<IEventStoreRepository, EventStoreRepository>();
            services.AddScoped<IEventStore, SqlEventStore>();
        }

        public static IJpProjectAdminBuilder AddEventStore<TEventStore>(this IJpProjectAdminBuilder services)
            where TEventStore : class, IEventStoreContext
        {
            services.Services.AddScoped<IEventStoreContext, TEventStore>();
            return services;
        }
    }
}
