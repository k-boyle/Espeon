using System.Text;

namespace Espeon {
    public readonly struct StringJoiner {
        private readonly StringBuilder _builder;
        private readonly string _seperator;

        public StringJoiner(string seperator) {
            this._seperator = seperator;
            this._builder = new StringBuilder();
        }
        
        public void Append(string str) {
            if (this._builder.Length > 0) {
                this._builder.Append(this._seperator);
            }

            this._builder.Append(str);
        }

        public void Clear() {
            this._builder.Clear();
        }
        
        public override string ToString() {
            return this._builder.ToString();
        }
    }
}