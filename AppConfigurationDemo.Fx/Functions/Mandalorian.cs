using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppConfigurationDemo.Fx.Functions
{
    public class Mandalorian
    {
        private readonly Settings _settings;
        private readonly IConfiguration _configuration;
        private readonly IConfigurationRefresher _configurationRefresher;

        public Mandalorian(IOptionsSnapshot<Settings> settings,  IConfiguration configuration, IConfigurationRefresher configurationRefresher)
        {
            _settings = settings.Value;
            _configuration = configuration;
            _configurationRefresher = configurationRefresher;
        }


        [FunctionName("Message")]
        public IActionResult Message(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            _ = _configurationRefresher.TryRefreshAsync();

            return new OkObjectResult(_configuration["Message"]);
        }

        [FunctionName("Relsonare")]
        public async Task<IActionResult> Relsonare(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            _ = _configurationRefresher.TryRefreshAsync();

            if (!_settings.Resolnare)
            {
                var notMandalorian = new List<string>()
                {
                    "You are not a Mandalorian"
                };
                return new OkObjectResult(notMandalorian);
            }
            else
            {
                var sixActions = new List<string>()
                {
                    "Llevar armadura, en especial el casco.",
                    "Hablar el idioma, o sea, mando’a.",
                    "Defenderse a uno mismo y a su familia.",
                    "Criar a sus hijos como mandalorianos.",
                    "Contribuir al bienestar del clan.",
                    "Cuando sea llamado por su Mandalore unirse a su causa."
                };

                return new OkObjectResult(sixActions);
            }
        }
    }
}
