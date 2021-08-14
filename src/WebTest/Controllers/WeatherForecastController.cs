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

        private readonly FooBarFeatures _features;
        private readonly IMySuperService _mySuperService;

        public WeatherForecastController(FooBarFeatures features,
            IMySuperService mySuperService)
        {
            _features = features;
            _mySuperService = mySuperService;
        }

        [HttpGet]
        public string Get()
        {
            // string flag
            if (_features.Baz.EnabledFor("michael"))
                throw new Exception("michael is not allowed");

            // Percent flag
            if (!_features.Bar.IsEnabled)
                throw new Exception("Bar not enabled");
            
            // boolean flag
            if (!_features.Foo.IsEnabled)
                throw new Exception("Foo not enabled");
            
            return _mySuperService.DoStuff();
        }
    }

    public class MySuperService : IMySuperService
    {
        private readonly FooBarFeatures _fooBarFeatures;

        public MySuperService(FooBarFeatures fooBarFeatures)
        {
            _fooBarFeatures = fooBarFeatures;
        }
        
        public string DoStuff()
        {
            return _fooBarFeatures.Bar.IsEnabled ? "Hello" : "goodbye";
        }
    }

    public interface IMySuperService
    {
        string DoStuff();
    }
}