using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BetterBullTracker.Models;
using BetterBullTracker.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BetterBullTracker.Controllers
{
    [ApiController]
    [Route("weather")]
    public class WeatherForecastController : ControllerBase
    {
        private SyncromaticsService Service;
        
        public WeatherForecastController(SyncromaticsService service)
        {
            Service = service;
        }
        
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        [HttpGet]
        public List<Route> Get()
        {
            return Service.GetRoutes().Values.ToList();
        }
    }
}
