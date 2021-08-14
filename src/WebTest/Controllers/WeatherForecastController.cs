using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly BasketFeatures _features;

        public WeatherForecastController(ILogger<WeatherForecastController> logger,
            BasketFeatures features)
        {
            _logger = logger;
            _features = features;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var featuresEllo = _features.Ello;
            var hello = _features.Hello;
            if (!_features.Abc.EnabledFor("michael"))
                throw new Exception("Michael is totally not allowed");
            
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = rng.Next(-20, 55),
                    Summary = Summaries[rng.Next(Summaries.Length)] + "    featuresEllo.IsEnabled    " + featuresEllo.IsEnabled + "    featuresHello.IsEnabled    " + hello.IsEnabled
                })
                .ToArray();
        }
    }
}