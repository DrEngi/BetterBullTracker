using System;
using System.Collections.Generic;
using MongoDB.Driver;
using SyncromaticsAPI.SyncromaticsModels ;

namespace HistoryDownloader
{
    class Program
    {
        private static MongoClient Client;
        private static IMongoDatabase Database;
        private static IMongoCollection<VehiclePosition> Vehicles;
        private static IMongoCollection<SyncromaticsAPI.SyncromaticsModels.SyncromaticsRoute> Routes;

        private static SyncromaticsAPI.SyncromaticsAPI API;

        private static List<int> InsertedRoutes = new List<int>();

        private static int index = 0;

        public static async System.Threading.Tasks.Task Main(string[] args)
        {
            Console.WriteLine("Starting...");

            Client = new MongoClient($"");
            Database = Client.GetDatabase("bus-dev");

            Vehicles = Database.GetCollection<VehiclePosition>("vehicles");
            Routes = Database.GetCollection<SyncromaticsRoute>("routes");

            API = new SyncromaticsAPI.SyncromaticsAPI("http://usfbullrunner.com", 3000);

            (await API.GetRoutesAsync()).ForEach(x =>
            {
                Routes.InsertOne(x);
                InsertedRoutes.Add(x.ID);
            });

            API.NewVehicleDownloaded += Api_NewVehicleDownloaded;

            API.Start();

            while (true) Console.ReadLine();
        }

        private static void Api_NewVehicleDownloaded(object sender, SyncromaticsAPI.Events.VehicleDownloadedArgs e)
        {
            if (!InsertedRoutes.Contains(e.Route.ID))
            {
                Routes.InsertOne(e.Route);
                InsertedRoutes.Add(e.Route.ID);
            }

            Vehicles.InsertOne(new VehiclePosition()
            {
                Index = API.getIndex(),
                Vehicle = e.Vehicle
            });
        }
    }
}
