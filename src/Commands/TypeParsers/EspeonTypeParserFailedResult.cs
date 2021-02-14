using System.Threading.Tasks;
using Qmmands;

namespace Espeon {
    public class EspeonTypeParserFailedResult<T> : TypeParserResult<T> {
        public EspeonTypeParserFailedResult(LocalisationStringKey key) : base(key.ToString()) {
        }

        public static implicit operator ValueTask<EspeonTypeParserFailedResult<T>>(EspeonTypeParserFailedResult<T> result) {
            return new(result);
        }
    }
}