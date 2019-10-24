using IdentityServer4.EntityFramework.Options;
using JPProject.Admin.Infra.Data.MigrationHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace JPProject.Admin.Infra.Data.Context
{
    public static class ContextConfiguration
    {
        public static IServiceCollection AddAdminContext(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsAction, JpDatabaseOptions options = null)
        {
            if (options == null)
                options = new JpDatabaseOptions();

            var operationalStoreOptions = new OperationalStoreOptions();
            services.AddSingleton(operationalStoreOptions);

            var storeOptions = new ConfigurationStoreOptions();
            services.AddSingleton(storeOptions);

            services.AddDbContext<IdentityServerContext>(optionsAction);
            services.AddDbContext<EventStoreContext>(optionsAction);

            DbMigrationHelpers.CheckDatabases(services.BuildServiceProvider(), options).Wait();

            return services;
        }
    }

    public class JpDatabaseOptions
    {
        public bool MustThrowExceptionIfDatabaseDontExist { get; set; }
    }
}
