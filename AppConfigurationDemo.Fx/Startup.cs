using AppConfigurationDemo.Fx.Functions;
using Azure.Identity;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Reflection;

[assembly: FunctionsStartup(typeof(AppConfigurationDemo.Fx.Startup))]
namespace AppConfigurationDemo.Fx
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            IConfigurationRefresher configurationRefresher = null;

            // Load configuration from Azure App Configuration
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();

            var configuration = configurationBuilder
                                    .SetBasePath(Directory.GetCurrentDirectory())
                                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                                    .AddEnvironmentVariables() 
                                    .AddUserSecrets(Assembly.GetExecutingAssembly())
                                    .Build();

            // Get data from secrets
            var connectionString = configuration.GetConnectionString("AppConfiguration");
            var endpoint = new Uri(configuration["AppConfiguration:Endpoint"]);


            /// Exclude ManagedIdentityCredential for demo purpose
            var appConfigurationCredentialOptions = new DefaultAzureCredentialOptions()
            {
                ExcludeManagedIdentityCredential = true,
                ExcludeEnvironmentCredential = true,
                ExcludeInteractiveBrowserCredential = true,
                ExcludeSharedTokenCacheCredential = true,
                ExcludeVisualStudioCodeCredential = true,
                ExcludeVisualStudioCredential = true
            };
            var appConfigurationCredential = new DefaultAzureCredential(appConfigurationCredentialOptions);

            var keyVaultCredentialOptions = new DefaultAzureCredentialOptions()
            {
                ExcludeManagedIdentityCredential = true,
                ExcludeEnvironmentCredential = true,
                ExcludeInteractiveBrowserCredential = true,
                ExcludeSharedTokenCacheCredential = true,
                ExcludeVisualStudioCodeCredential = true,
                ExcludeVisualStudioCredential = true
            };
            var keyVaultCredential = new DefaultAzureCredential(keyVaultCredentialOptions);

            // Create a new configurationbuilder and add appconfiguration
            configuration = configurationBuilder.AddAzureAppConfiguration(options =>
            {
                // We can connect to Azure AppConfiguration using a connection string or an uri with credentials
                options.Connect(connectionString)
                //options.Connect(endpoint, appConfigurationCredential)
                       .Select(KeyFilter.Any, LabelFilter.Null)
                       .Select(KeyFilter.Any, $"{Assembly.GetExecutingAssembly().GetName().Name}:{builder.GetContext().EnvironmentName}")
                        // We can configure refresh to use dynamic configuration
                        .ConfigureRefresh(refresh =>
                        {
                            refresh.Register("Sentinel", refreshAll: true)
                                    .SetCacheExpiration(new TimeSpan(0, 0, 10));
                        })
                       // We can connect to Azure Key Vault using credentials 
                       .ConfigureKeyVault(kv =>
                       {
                           kv.SetCredential(keyVaultCredential);
                       });
                // We can configure refresh to use dynamic configuration
                configurationRefresher = options.GetRefresher();
            }).Build();

            builder.Services.Configure<Settings>(configuration.GetSection("Settings"));
            builder.Services.AddSingleton<IConfiguration>(configuration);
            builder.Services.AddSingleton<IConfigurationRefresher>(configurationRefresher);
        }         
    }
}
