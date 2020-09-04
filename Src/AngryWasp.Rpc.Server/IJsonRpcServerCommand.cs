namespace AngryWasp.Rpc.Server
{
    public interface IJsonRpcServerCommand
    {
        bool Handle(string req, out object res);
    }
}