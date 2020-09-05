using AngryWasp.Logger;
using Newtonsoft.Json;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System;

using Log = AngryWasp.Logger.Log;
using AngryWasp.Helpers;
using System.Reflection;
using System.Linq;

namespace AngryWasp.Json.Rpc
{
    public class JsonRpcServer
    {
        public const uint API_LEVEL = 1;
        HttpListener listener;

        private ushort port = 0;

        public ushort Port => port;

        public delegate bool RpcFunc(string args, out object value);

        private Dictionary<string, RpcFunc> commands = new Dictionary<string, RpcFunc>();

        public JsonRpcServer(ushort port)
        {
            this.port = port;
        }

        public void RegisterCommand(string key, RpcFunc value)
        {
            if (!commands.ContainsKey(key))
                commands.Add(key, value);
        }

        public void RegisterCommands()
        {
            RegisterCommands(Assembly.GetEntryAssembly());
        }

        public void RegisterCommands(Assembly assembly)
        {
            var types = ReflectionHelper.Instance.GetTypesInheritingOrImplementing(assembly, typeof(IJsonRpcServerCommand))
                .Where(m => m.GetCustomAttributes(typeof(JsonRpcServerCommandAttribute), false).Length > 0)
                .ToArray();

            foreach (var type in types)
            {
                IJsonRpcServerCommand i = (IJsonRpcServerCommand)Activator.CreateInstance(type);
                JsonRpcServerCommandAttribute a = i.GetType().GetCustomAttributes(true).OfType<JsonRpcServerCommandAttribute>().FirstOrDefault();
                RegisterCommand(a.Key, i.Handle);
            }
        }

        public void Start()
        {

            listener = new HttpListener();
            //todo: SSL and authentication
            listener.Prefixes.Add($"http://*:{port}/");
            listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;

            try
            {
                listener.Start();
            } catch (Exception ex)
            {
                Log.Instance.Write(Log_Severity.Error, ex.Message);
            }
            
            Log.Instance.Write($"Local RPC endpoint on port {port}");
            Log.Instance.Write("RPC server initialized");

            Task.Run(() =>
            {
                while(true)
                {
                    HttpListenerContext context = listener.GetContext();
                    HttpListenerRequest request = context.Request;
                    HandleRequest(context);
                }
            });
        }

        private void HandleRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            response.ContentType = "application/json";

            string text;
            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                text = reader.ReadToEnd();
            }

            string method = request.Url.Segments[1];
            bool ok = false;
            object resultData = null;

            if (commands.ContainsKey(method))
            {
                try
                {
                    ok = commands[method].Invoke(text, out resultData);
                }
                catch
                {
                    resultData = new JsonResponse<object>() {
                        Data = "Exception in RPC request"}.Serialize();
                }
            }
            else
            {
                resultData = new JsonResponse<object>() {
                    Data = "The specified method does not exist"}.Serialize();
            }

            response.StatusCode = ok ? (int)Response_Code.OK : (int)Response_Code.Error;
            ((JsonResponseBase)resultData).Status = ok ? "OK" : "ERROR";
            response.OutputStream.Write(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(resultData)));
            context.Response.Close();
        }
    }
}