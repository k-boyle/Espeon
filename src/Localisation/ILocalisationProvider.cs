using System.Collections.Generic;
using System.Threading.Tasks;

namespace Espeon {
    public interface ILocalisationProvider {
        ValueTask<IDictionary<Localisation, IDictionary<LocalisationStringKey, string>>> GetLocalisationsAsync();
    }
}