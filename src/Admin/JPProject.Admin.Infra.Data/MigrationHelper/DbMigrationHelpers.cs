﻿using IdentityServer4.EntityFramework.Entities;
using JPProject.Admin.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace JPProject.Admin.Infra.Data.MigrationHelper
{
    public static class DbMigrationHelpers
    {
        public static async Task CheckDatabases(IServiceProvider serviceProvider, JpDatabaseOptions options)
        {
            using var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();

            var id4Context = scope.ServiceProvider.GetRequiredService<IdentityServerContext>();
            var storeDb = scope.ServiceProvider.GetRequiredService<EventStoreContext>();

            await WaitForDb(id4Context);
            await storeDb.Database.GetPendingMigrationsAsync();
            await storeDb.Database.MigrateAsync();

            var isDatabaseExist = await CheckTableExists<Client>(id4Context);
            if (isDatabaseExist && options.MustThrowExceptionIfDatabaseDontExist)
                throw new Exception("IdentityServer4 Database doesn't exist. Ensure it was created before.'");

        }

        /// <summary>
        /// Check if data table is exist in application
        /// </summary>
        /// <typeparam name="T">Class of data table to check</typeparam>
        /// <param name="db">DB Object</param>
        private static async Task<bool> CheckTableExists<T>(DbContext db) where T : class
        {
            try
            {
                await db.Set<T>().CountAsync();
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }


        private static async Task WaitForDb(DbContext context)
        {
            var maxAttemps = 3;
            var delay = 5000;

            var healthChecker = new DbHealthChecker();
            for (int i = 0; i < maxAttemps; i++)
            {
                var canConnect = await healthChecker.TestConnection(context);
                if (canConnect)
                {
                    return;
                }
                await Task.Delay(delay);
            }

            // after a few attemps we give up
            throw new Exception("Error wating database");

        }
    }
}
