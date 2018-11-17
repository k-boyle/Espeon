using System;
using System.Threading.Tasks;
using Espeon.Core.Attributes;
using Qmmands;

[Service(typeof(IResponseService), true)]
public class ResponseService : IResponseService
{
    public async Task<string> GetResponseAsync(Module module, Command command)
    {
        throw new NotImplementedException();
    }
}