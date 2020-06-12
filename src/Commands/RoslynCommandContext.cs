namespace Espeon {
    public class RoslynCommandContext {
        public EspeonCommandContext Context { get; }

        public RoslynCommandContext(EspeonCommandContext context) {
            Context = context;
        }
        
        public string Inspect(object obj) {
            return obj.Inspect();
        }
    }
}