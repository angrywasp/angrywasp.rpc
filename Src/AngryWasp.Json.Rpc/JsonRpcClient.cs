using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AngryWasp.Json.Rpc
{
    public class JsonRpcClient
    {
        private ushort port;
        private string url;

        HttpClient httpClient;

        public ushort Port => port;

        public string Url => url;

        public JsonRpcClient(string url, ushort port)
        {
            this.url = url;
            this.port = port;
            this.httpClient = new HttpClient();
        }

        public async Task<string> SendRequest(string endpoint, string data)
        {
            try
            {
                byte[] requestData = Encoding.ASCII.GetBytes(data);

                HttpContent content = new StringContent(data, Encoding.ASCII, "application/json");
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var result = await httpClient.PostAsync($"http://{url}:{port}/{endpoint}", content);
                string str = await result.Content.ReadAsStringAsync();
                return str;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}