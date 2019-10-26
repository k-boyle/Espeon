using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Espeon.Core {
	public static partial class Extensions {
		public static Task<T[]> AllAsync<T>(this IEnumerable<Task<T>> tasks) {
			if (tasks == null) {
				throw new ArgumentNullException(nameof(tasks));
			}

			return Task.WhenAll(tasks);
		}

		public static T Invoke<T>(this Action<T> action) where T : new() {
			var obj = new T();
			action.Invoke(obj);

			return obj;
		}
	}
}