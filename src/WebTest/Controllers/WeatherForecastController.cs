using System;
using Microsoft.AspNetCore.Mvc;

namespace WebTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly FooBarFeatures _features;
        private readonly IMySuperService _mySuperService;
        private readonly NewStuffFeatures _newStuffFeatures;

        public WeatherForecastController(FooBarFeatures features,
            IMySuperService mySuperService, NewStuffFeatures newStuffFeatures)
        {
            _features = features;
            _mySuperService = mySuperService;
            _newStuffFeatures = newStuffFeatures;
        }

        [HttpGet]
        public string Get()
        {
            // string flag
            if (_features.Baz.EnabledFor("michael"))
                throw new Exception("michael is not allowed");

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
            return _fooBarFeatures.Baz.EnabledFor("1") ? "Hello" : "goodbye";
        }
    }

    public interface IMySuperService
    {
        string DoStuff();
    }
}