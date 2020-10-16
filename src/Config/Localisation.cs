using System.Collections.Generic;

namespace Espeon {
    public class Localisation {
        public string Path { get; set; }
        public HashSet<string> ExcludedFiles { get; set; }
        public string ExclusionRegex { get; set; }
    }
}