using System;
using System.Collections.Generic;

namespace Espeon {
    public class ScheduledTaskComparer : IComparer<IScheduledTask> {
        public static ScheduledTaskComparer Instance = new ScheduledTaskComparer();
        
        private ScheduledTaskComparer() {}
        
        public int Compare(IScheduledTask left, IScheduledTask right) {
            if (left is null) {
                throw new ArgumentNullException(nameof(left));
            }
            if (right is null) {
                throw new ArgumentNullException(nameof(right));
            }
            
            return left.ExecuteAt.CompareTo(right.ExecuteAt);
        }
    }
}