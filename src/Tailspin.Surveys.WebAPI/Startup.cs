// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System; //Needed for KeyVaultConfigurationProvider
using System.IdentityModel.Tokens;
using Microsoft.AspNet.Authentication.JwtBearer;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Diagnostics;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http.Features;
using Microsoft.Data.Entity;
using Microsoft.Dnx.Runtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tailspin.Surveys.Data.DataModels;
using Tailspin.Surveys.Data.DataStore;
using Tailspin.Surveys.Security.Policy;
using AppConfiguration = Tailspin.Surveys.WebApi.Configuration;
using Constants = Tailspin.Surveys.Common.Constants;
using Microsoft.Extensions.PlatformAbstractions;

namespace Tailspin.Surveys.WebApi
{
    /// <summary>
    /// This class contains the starup logic for this WebAPI project.
    /// </summary>
    public class Startup
    {
        private AppConfiguration.ConfigurationOptions _configOptions = new AppConfiguration.ConfigurationOptions();

        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv, ILoggerFactory loggerFactory)
        {
            InitializeLogging(loggerFactory);
            var builder = new ConfigurationBuilder()
                .SetBasePath(appEnv.ApplicationBasePath)
                .AddJsonFile("appsettings.json");

            if (env.IsDevelopment())
            {
                // This reads the configuration keys from the secret store.
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }
            builder.AddEnvironmentVariables();

            // Uncomment the block of code below if you want to load secrets from KeyVault
            // It is recommended to use certs for all authentication when using KeyVault
//#if DNX451
//            var config = builder.Build();
//            builder.AddKeyVaultSecrets(config["AzureAd:ClientId"],
//                config["KeyVault:Name"],
//                config["AzureAd:Asymmetric:CertificateThumbprint"],
//                Convert.ToBoolean(config["AzureAd:Asymmetric:ValidationRequired"]),
//                loggerFactory);
//#endif

            builder.Build().Bind(_configOptions);
        }

        // This method gets called by a runtime.
        // Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(PolicyNames.RequireSurveyCreator,
                    policy =>
                    {
                        policy.AddRequirements(new SurveyCreatorRequirement());
                        policy.RequireAuthenticatedUser(); // Adds DenyAnonymousAuthorizationRequirement 
                        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    });
                options.AddPolicy(PolicyNames.RequireSurveyAdmin,
                    policy =>
                    {
                        policy.AddRequirements(new SurveyAdminRequirement());
                        policy.RequireAuthenticatedUser(); // Adds DenyAnonymousAuthorizationRequirement 
                        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    });
            });

            // Add Entity Framework services to the services container.
            services.AddEntityFramework()
                .AddSqlServer()
                .AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(_configOptions.Data.SurveysConnectionString));

            services.AddScoped<TenantManager, TenantManager>();
            services.AddScoped<UserManager, UserManager>();

            services.AddMvc();

            services.AddScoped<ISurveyStore, SqlServerSurveyStore>();
            services.AddScoped<IQuestionStore, SqlServerQuestionStore>();
            services.AddScoped<IContributorRequestStore, SqlServerContributorRequestStore>();
            services.AddSingleton<IAuthorizationHandler>(factory =>
            {
                var loggerFactory = factory.GetService<ILoggerFactory>();
                return new SurveyAuthorizationHandler(loggerFactory.CreateLogger<SurveyAuthorizationHandler>());
            });
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ApplicationDbContext dbContext, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                //app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage(options =>
                {
                    options.ShowExceptionDetails = true;
                });
            }

            app.UseIISPlatformHandler();

            app.UseJwtBearerAuthentication(options =>
            {
                options.Audience = _configOptions.AzureAd.WebApiResourceId;
                options.Authority = Constants.AuthEndpointPrefix + "common/";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    //Instead of validating against a fixed set of known issuers, we perform custom multi-tenant validation logic
                    ValidateIssuer = false,
                };
                options.Events = new SurveysJwtBearerEvents(loggerFactory.CreateLogger<SurveysJwtBearerEvents>());
            });
            // Add MVC to the request pipeline.
            app.UseMvc();
        }
        private void InitializeLogging(ILoggerFactory loggerFactory)
        {
            loggerFactory.MinimumLevel = LogLevel.Information;
            loggerFactory.AddDebug(LogLevel.Information);
        }
    }
}