using System;

namespace Espeon {
    public class ExampleAttribute : Attribute {
        public string Value { get; }

        public ExampleAttribute(string value) {
            Value = value;
        }
    }
}