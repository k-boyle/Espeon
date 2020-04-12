using System.Threading.Tasks;

namespace Espeon {
    public interface IInitialisableService {
        Task InitialiseAsync();
    }
}