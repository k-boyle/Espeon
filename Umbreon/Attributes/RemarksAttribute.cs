using System;

namespace Umbreon.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter)]
    public class RemarksAttribute : Attribute
    {
        public string[] RemarkStrings { get; }

        public RemarksAttribute(params string[] remarks)
        {
            RemarkStrings = remarks;
        }
    }
}
