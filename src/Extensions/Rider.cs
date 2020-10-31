using JetBrains.Annotations;

namespace Espeon {
    //TODO extract this to a different project
    public static class RiderExtensions {
        [SourceTemplate]
        [Macro(Target = "type", Expression = "fixedTypeName()")]
        public static void @is(this object obj) {
            /*$ if (obj is type) {
                $END$
             }*/
        }
    }
}