using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using SyncromaticsAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SYNCDownloader // Note: actual namespace depends on the project name.
{
    public class Program
    {
        private static MongoClient Client;
        private static IMongoDatabase Database;
        private static IMongoCollection<VehiclePosition> Collection;
        private static SyncromaticsAPI.SyncromaticsAPI Syncromatics;
        
        public static async Task Main(string[] args)
        {
            DatabaseConfig config = JsonConvert.DeserializeObject<DatabaseConfig>(File.ReadAllText("config.json"));
            Client = new MongoClient($"mongodb://{config.username}:{config.password}@{config.address}:{config.port}");
            Database = Client.GetDatabase("bus-dev");
            Collection = Database.GetCollection<VehiclePosition>("positions3");
            Console.WriteLine("Starting");

            Syncromatics = new SyncromaticsAPI.SyncromaticsAPI("https://usfbullrunner.com", 3000);
            Syncromatics.NewVehicleDownloaded += Syncromatics_NewVehicleDownloadedAsync;
            await Syncromatics.GetRoutesAsync();
            Syncromatics.Start();

            await Task.Delay(-1);
        }

        private static async void Syncromatics_NewVehicleDownloadedAsync(object? sender, SyncromaticsAPI.Events.VehicleDownloadedArgs e)
        {
            Console.WriteLine("started");
            await Collection.InsertOneAsync(new VehiclePosition()
            {
                Index = Syncromatics.getIndex(),
                Vehicle = e.Vehicle,
                Route = e.Route
            });
        }
    }

    public class DatabaseConfig
    {
        public string address { get; set; }
        public int port { get; set; }
        public string username { get; set; }
        public string password { get; set; }
    }

    public class VehiclePosition
    {
        public BsonObjectId _id { get; set; }
        public int Index { get; set; }
        public SyncromaticsAPI.SyncromaticsModels.SyncromaticsVehicle Vehicle { get; set; }
        public SyncromaticsAPI.SyncromaticsModels.SyncromaticsRoute Route { get; set; }
    }
}