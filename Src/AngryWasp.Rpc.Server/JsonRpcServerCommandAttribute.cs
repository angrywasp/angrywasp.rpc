using System;

namespace AngryWasp.Rpc.Server
{
    public class JsonRpcServerCommandAttribute : Attribute
    {
        private string key = string.Empty;

        public string Key => key;

        public JsonRpcServerCommandAttribute(string key)
        {
            this.key = key;
        }
    }
}