using Qmmands;
using System.Collections.Generic;

namespace Espeon.Services
{
    public interface IResponseService
    {
        void LoadResponses(IEnumerable<Module> module);
        string GetResponse(string module, string command, ResponsePack pack, int index, params object[] args);
        string GetResponse(object obj, ResponsePack pack, int index, params object[] args);
    }
}
