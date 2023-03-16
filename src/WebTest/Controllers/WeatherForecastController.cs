using System;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace WebTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IMySuperService _mySuperService;
        private readonly NewStuffFeatures _newStuffFeatures;

        public WeatherForecastController(
            IMySuperService mySuperService, 
            NewStuffFeatures newStuffFeatures)
        {
            _mySuperService = mySuperService;
            _newStuffFeatures = newStuffFeatures;
        }

        [HttpGet]
        public string Get()
        {
            var newGuid = Guid.NewGuid();
            var s = new
            {
                SometimesIWork = $"{newGuid} = {_newStuffFeatures.SometimesIWork.EnabledFor(newGuid).ToString()}",
                Baz111 = $"hello = {_newStuffFeatures.Baz111.EnabledFor("hello")}",
                Bazz333 = $"hello = {_newStuffFeatures.Baz333.EnabledFor("hello")}",
                Baz555 = $"hello = {_newStuffFeatures.Baz555.EnabledFor("hello")}",
                EndsWith = $"hello = {_newStuffFeatures.EndsWith.EnabledFor("hello")}",
                Hello = _newStuffFeatures.Hello.IsEnabled,
                CanUseEmails = _newStuffFeatures.CanUseEmails.IsEnabled,
            };

            return JsonSerializer.Serialize(s);
        }
    }

    public class MySuperService : IMySuperService
    {
        private readonly NewStuffFeatures _fooBarFeatures;

        public MySuperService(NewStuffFeatures fooBarFeatures)
        {
            _fooBarFeatures = fooBarFeatures;
        }
        
        public string DoStuff()
        {
            return _fooBarFeatures.SometimesIWork.EnabledFor(50) ? "Hello" : "goodbye";
        }
    }

    public interface IMySuperService
    {
        string DoStuff();
    }
}