using AppConfigurationDemo.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace AppConfigurationDemo.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class MandalorianController : ControllerBase
    { 
        private readonly ILogger<MandalorianController> _logger;
        private readonly IConfiguration _configuration;

        public MandalorianController(IConfiguration configuration, ILogger<MandalorianController> logger)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet]
        public IEnumerable<MandalorianModel> Get()
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<string>), 200)]
        [ProducesResponseType(typeof(BadRequestObjectResult), 400)]
        [ProducesResponseType(typeof(NotFoundObjectResult), 404)]
        [ProducesResponseType(500)]
        [Route("resolnare")]
        public IActionResult Resolnare()
        {
            _ = bool.TryParse(_configuration["Settings:Resolnare"], out bool resolnare);

            if(!resolnare)
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

        [HttpGet]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(BadRequestObjectResult), 400)]
        [ProducesResponseType(typeof(NotFoundObjectResult), 404)]
        [ProducesResponseType(500)]
        [Route("message")]
        public IActionResult Message()
        {
            return new OkObjectResult(_configuration["Message"]);           
        }
    }
}
