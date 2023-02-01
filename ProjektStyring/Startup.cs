using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using ProjektStyring.Services;
using RestSharp;

namespace ProjektStyring
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = "oidc";
            })
                .AddCookie("Cookies")
                .AddOpenIdConnect("oidc", config =>
                {
                    config.Authority = "https://skpidb.azurewebsites.net";
                    config.ClientId = "88b7017b-6462-4fdb-aeb5-de8421b73aaf";
                    config.ClientSecret = "ZTQ5NTBjODktMWNmOC00ZDg0LTg3OWMtYmE2MDMyZTRjZTkx";
                    config.SaveTokens = true;
                    config.UseTokenLifetime = true;
                    config.ResponseType = "code";

                    config.Scope.Clear();
                    config.Scope.Add("openid");
                    config.Scope.Add("profile");
                    config.Scope.Add("role");
                    config.ClaimActions.MapJsonKey("role", "roles");

                    config.GetClaimsFromUserInfoEndpoint = true;

                    config.TokenValidationParameters.NameClaimType = "name";
                    config.TokenValidationParameters.RoleClaimType = "role";

                    config.SignedOutRedirectUri = "/";
                });
            services.AddControllersWithViews();
            services.AddSingleton<IProjectsCosmosDbService>(InitializeCosmosClientProjectsInstanceAsync(Configuration.GetSection("CosmosDB.Projects")).GetAwaiter().GetResult());
            services.AddSingleton<ITasksCosmosDbService>(InitializeCosmosClientTasksInstanceAsync(Configuration.GetSection("CosmosDB.Tasks")).GetAwaiter().GetResult());
            services.AddSingleton<IFeedbackCosmosDbService>(InitializeCosmosClientFeedbackInstanceAsync(Configuration.GetSection("FeedbackDB.Tasks")).GetAwaiter().GetResult());
            services.AddSingleton<IDocumentationBlobService>(InitializeDocumentationBlobInstanceAsync(Configuration.GetSection("BlobStorage.Documentation")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapControllerRoute(
                //    name: "project",
                //    pattern: "{controller=Project}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
        private static async Task<ProjectsCosmosDbService> InitializeCosmosClientProjectsInstanceAsync(IConfigurationSection configurationSection)
        {
            string databaseName = configurationSection.GetSection("DatabaseName").Value;
            string containerName = configurationSection.GetSection("ContainerName").Value;
            string account = configurationSection.GetSection("Account").Value;
            string key = configurationSection.GetSection("Key").Value;
            Microsoft.Azure.Cosmos.CosmosClient client = new Microsoft.Azure.Cosmos.CosmosClient(account, key);
            ProjectsCosmosDbService cosmosDbService = new ProjectsCosmosDbService(client, databaseName, containerName);
            Microsoft.Azure.Cosmos.DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);

            await database.Database.CreateContainerIfNotExistsAsync(containerName, "/id");

            return cosmosDbService;
        }
        private static async Task<TasksCosmosDbService> InitializeCosmosClientTasksInstanceAsync(IConfigurationSection configurationSection)
        {
            string databaseName = configurationSection.GetSection("DatabaseName").Value;
            string containerName = configurationSection.GetSection("ContainerName").Value;
            string account = configurationSection.GetSection("Account").Value;
            string key = configurationSection.GetSection("Key").Value;
            Microsoft.Azure.Cosmos.CosmosClient client = new Microsoft.Azure.Cosmos.CosmosClient(account, key);
            TasksCosmosDbService cosmosDbService = new TasksCosmosDbService(client, databaseName, containerName);
            Microsoft.Azure.Cosmos.DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
            await database.Database.CreateContainerIfNotExistsAsync(containerName, "/id");

            return cosmosDbService;
        }

        private static async Task<FeedbackCosmosDbService> InitializeCosmosClientFeedbackInstanceAsync(IConfigurationSection configurationSection)
        {
            string databaseName = configurationSection.GetSection("DatabaseName").Value;
            string containerName = configurationSection.GetSection("ContainerName").Value;
            string account = configurationSection.GetSection("Account").Value;
            string key = configurationSection.GetSection("Key").Value;
            Microsoft.Azure.Cosmos.CosmosClient client = new Microsoft.Azure.Cosmos.CosmosClient(account, key);
            FeedbackCosmosDbService cosmosDbService = new FeedbackCosmosDbService(client, databaseName, containerName);
            Microsoft.Azure.Cosmos.DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
            await database.Database.CreateContainerIfNotExistsAsync(containerName, "/id");

            return cosmosDbService;
        }

        static DocumentationBlobService InitializeDocumentationBlobInstanceAsync(IConfigurationSection configurationSection)
        {
            DocumentationBlobService docService = new DocumentationBlobService(new Azure.Storage.Blobs.BlobServiceClient(configurationSection.GetSection("ConnectionString").Value));
            return docService;
        }
    }
}