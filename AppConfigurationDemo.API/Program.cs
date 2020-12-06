using Azure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Hosting;
using System;
using System.Reflection;

namespace AppConfigurationDemo.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureAppConfiguration((context, config) =>
                    {
                        var env = context.HostingEnvironment;
                        var configuration = config.Build();

                        // Get data from secrets
                        var connectionString = configuration.GetConnectionString("AppConfiguration");
                        var endpoint = new Uri(configuration["AppConfiguration:Endpoint"]);

                        // Exclude ManagedIdentityCredential for demo purpose
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
                        var keVaultCredential = new DefaultAzureCredential(keyVaultCredentialOptions);

                        config.AddAzureAppConfiguration(options =>
                        {
                            // We can connect to Azure AppConfiguration using a connection string or an uri with credentials
                            //options.Connect(connectionString)
                            options.Connect(endpoint, appConfigurationCredential)
                                .Select(KeyFilter.Any, LabelFilter.Null)
                                .Select(KeyFilter.Any, $"{Assembly.GetExecutingAssembly().GetName().Name}:{env.EnvironmentName}")
                                // We can configure refresh to use dynamic configuration
                                .ConfigureRefresh(refresh =>
                                {
                                    refresh.Register("Sentinel", refreshAll: true)
                                            .SetCacheExpiration(new TimeSpan(0, 0, 10));
                                })
                                // We can connect to Azure Key Vault using credentials 
                                .ConfigureKeyVault(kv =>
                                {
                                    kv.SetCredential(keVaultCredential);
                                });
                        });
                    }).UseStartup<Startup>();
                });
    }
}
