using System.Threading.Tasks;

namespace Espeon {
    public interface IOnReadyService {
        Task OnReadyAsync(EspeonDbContext context);
    }
}