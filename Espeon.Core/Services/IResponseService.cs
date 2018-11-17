using System.Threading.Tasks;
using Qmmands;
public interface IResponseService
{
    Task<string> GetResponseAsync(Module module, Command command);
}