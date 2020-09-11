﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BetterBullTracker.Models;
using BetterBullTracker.Services;
using BetterBullTracker.Spatial;
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

        [HttpGet]
        public List<Route> Get()
        {
            return Service.GetRoutes().Values.ToList();
        }

        [HttpPost("closest")]
        public ActionResult<Dictionary<int, Stop>> GetClosestStops([FromBody] Coordinate coord)
        {
            Dictionary<int, Stop> stops = new Dictionary<int, Stop>();
            foreach(Route route in Service.GetRoutes().Values)
            {
                Stop closest = null;
                double distance = double.MaxValue;
                foreach(Stop stop in route.RouteStops)
                {
                    double thisDistance = coord.DistanceTo(stop.Coordinate);
                    if (thisDistance < distance)
                    {
                        closest = stop;
                        distance = thisDistance;
                    }
                }
                stops.Add(route.RouteID, closest);
            }
            return stops;
        }
    }
}
