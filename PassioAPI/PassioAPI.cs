using PassioAPI.Models;
using Flurl.Http;
using System.Collections.Generic;

namespace PassioAPI
{
    public class PassioAPI
    {
        private string BackendURL;
        private int PollRate;
        
        public PassioAPI(string backendURL, int pollRate)
        {
            this.BackendURL = backendURL;
            this.PollRate = pollRate;
        }

        public async Task<Root> GetStops()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("json", "{\"s0\":\"76\",\"sA\":1}");

            Root stops = await (this.BackendURL + "/www/mapGetData.php?getStops=2&withOutdated=1&wBounds=1&showBusInOos=0&wTransloc=1").PostUrlEncodedAsync(data).ReceiveJson<Root>();
            return stops;
        }
    }
}