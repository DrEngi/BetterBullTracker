using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WatsonWebsocket;

namespace BetterBullTracker.WebSockets
{
    public class WebsocketService
    {
        WatsonWsServer Server;
        List<string> Clients;
        
        public WebsocketService()
        {
            Server = new WatsonWsServer("*", 5003, false);
            Clients = new List<string>();

            Server.ClientConnected += Server_ClientConnected;
            Server.ClientDisconnected += Server_ClientDisconnected;
            Server.MessageReceived += Server_MessageReceived;

            Server.Start();
        }

        public async Task SendVehicleUpdateAsync(WSVehicleUpdateMsg msg)
        {
            foreach(string client in Clients)
            {
                await Server.SendAsync(client, JsonConvert.SerializeObject(msg));
            }
        }

        private void Server_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Console.WriteLine("msg: " + e.Data);
        }

        private void Server_ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            Console.WriteLine("new client connected");
            Clients.Add(e.IpPort);
        }

        private void Server_ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            Console.WriteLine("client disconnected");
            Clients.Remove(e.IpPort);
        }
    }
}
