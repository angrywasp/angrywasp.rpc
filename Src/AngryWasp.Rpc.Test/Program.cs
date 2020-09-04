using AngryWasp.Rpc.Client;
using AngryWasp.Rpc.Common;
using AngryWasp.Rpc.Server;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace AngryWasp.Rpc.Test
{
    public static class Program
    {
        private static JsonRpcClient client;
        private static JsonRpcServer server;

        public static JsonRpcClient Client => client;

        [STAThread]
        public static void Main(string[] args)
        {
            Log.Initialize();
            var an = Assembly.GetEntryAssembly().GetName();
            Log.WriteConsole($"{an.Name}: {an.Version}");

            client = new JsonRpcClient("127.0.0.1", 5000);
            server = new JsonRpcServer(5000);

            server.RegisterCommands();
            server.Start();

            Application.RegisterCommands();
            Application.Start();
        }
    }

    // CLI command to send RPC request

    [ApplicationCommand("echo", "Send the test echo rpc command")]
    public class EchoCliCommand : IApplicationCommand
    {
        public bool Handle(string command)
        {
            JsonRequest<EchoRequest> request = new JsonRequest<EchoRequest>();
            //The API level can be set manually on a per call basis
            //request.ApiLevel.Major = 2;
            request.Data.Value = command;

            Task.Run( async () =>
            {
                string requestString = request.Serialize();
                Log.WriteConsole("Sending JSON");
                Log.WriteConsole(requestString);

                string str = await Program.Client.SendRequest("echo", requestString);
                Log.WriteConsole("Received JSON");
                Log.WriteConsole(str);

                JsonResponse<EchoResponse> response;
                if (!JsonResponse<EchoResponse>.Deserialize(str, out response))
                    Log.WriteError("Error with JSON response");
                else
                    Log.WriteConsole($"JSON responded with {response.Data.Value}");
            });

            return true;
        }
    }

    // Server side RPC request handler
    public class EchoRequest
    {
        [JsonProperty("value")]
        public string Value { get; set; } = string.Empty;
    }

    public class EchoResponse
    {
        [JsonProperty("value")]
        public string Value { get; set; } = string.Empty;
    }

    [JsonRpcServerCommand("echo")]
    public class EchoServerCommand : IJsonRpcServerCommand
    {
        public bool Handle(string requestString, out object responseObject)
        {
            JsonRequest<EchoRequest> request = null;
            responseObject = null;

            // deserialize the incoming request
            if (!JsonRequest<EchoRequest>.Deserialize(requestString, out request))
                return false;

            //echo command just sends back what it receives
            JsonResponse<EchoResponse> response = new JsonResponse<EchoResponse>();
            response.Data.Value = request.Data.Value;

            //return
            responseObject = response;
            return true;
        }
    }
}