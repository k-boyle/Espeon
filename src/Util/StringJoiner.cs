using System.Text;

namespace Espeon {
    public struct StringJoiner {
        private StringBuilder _builder;
        private readonly string _seperator;

        // account for default ctor
        private StringBuilder Builder => this._builder ??= new StringBuilder();

        public StringJoiner(string seperator) {
            this._seperator = seperator;
            this._builder = new StringBuilder();
        }
        
        public void Append(string str) {
            if (Builder.Length > 0) {
                Builder.Append(this._seperator);
            }

            Builder.Append(str);
        }

        public void Clear() {
            Builder.Clear();
        }
        
        public override string ToString() {
            return Builder.ToString();
        }
    }
}