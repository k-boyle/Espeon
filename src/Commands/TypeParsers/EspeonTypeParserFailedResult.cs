using Qmmands;
using System.Threading.Tasks;

namespace Espeon {
    public class EspeonTypeParserFailedResult<T> : TypeParserResult<T> {
        public LocalisationStringKey Key { get; }

        public EspeonTypeParserFailedResult(LocalisationStringKey key) : base("") {
            Key = key;
        }
        
        public static implicit operator ValueTask<EspeonTypeParserFailedResult<T>>(EspeonTypeParserFailedResult<T> result) {
            return new ValueTask<EspeonTypeParserFailedResult<T>>(result);
        }
    }
}