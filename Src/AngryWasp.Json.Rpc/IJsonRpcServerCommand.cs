namespace AngryWasp.Json.Rpc
{
    public interface IJsonRpcServerCommand
    {
        bool Handle(string req, out object res);
    }
}