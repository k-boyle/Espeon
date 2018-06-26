using System;

namespace Umbreon.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter)]
    public class Remarks : Attribute
    {
        public string[] RemarkStrings { get; }

        public Remarks(params string[] remarks)
        {
            RemarkStrings = remarks;
        }
    }
}
