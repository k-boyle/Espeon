using System.Collections.Generic;
using System.Threading.Tasks;

namespace Espeon {
    public interface ILocalisationProvider {
        ValueTask<IDictionary<Language, IDictionary<LocalisationStringKey, string>>> GetLocalisationsAsync();
    }
}