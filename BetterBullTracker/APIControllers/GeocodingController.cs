using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BetterBullTracker.Databases;
using BetterBullTracker.Databases.Models;
using BetterBullTracker.Spatial;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BetterBullTracker.APIControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeocodingController : ControllerBase
    {
        private DatabaseService Database;

        public GeocodingController(DatabaseService database)
        {
            Database = database;
        }

        [HttpGet("{text}")]
        public async Task<List<POI>> GetBuildingsBySearchAsync(string text)
        {
            List<DBBuilding> bldgs = await Database.GetBuildingCollection().GetBuildingsAsync(text);
            List<POI> pointsToReturn = new List<POI>();

            bldgs.ForEach(x => pointsToReturn.Add(new POI() { Name = x.Name, ShortName = x.ShortName }));

            return pointsToReturn;
        }
    }
}
