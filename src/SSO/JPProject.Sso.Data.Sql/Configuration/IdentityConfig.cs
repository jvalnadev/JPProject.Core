﻿using System;
using System.Reflection;
using JPProject.Sso.Infra.Data.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JPProject.Sso.EntityFrameworkCore.SqlServer.Configuration
{
    public static class IdentityConfig
    {

        public static IServiceCollection WithSqlServer(this IServiceCollection builder, string connectionString)
        {
            var migrationsAssembly = typeof(IdentityConfig).GetTypeInfo().Assembly.GetName().Name;
            builder.AddEntityFrameworkSqlServer().AddSsoContext(opt => opt.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly)));

            return builder;
        }

        public static IServiceCollection WithSqlServer(this IServiceCollection builder, Action<DbContextOptionsBuilder> optionsAction)
        {
            builder.AddEntityFrameworkSqlServer().AddSsoContext(optionsAction);

            return builder;
        }
    }
}
