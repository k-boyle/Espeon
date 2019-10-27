using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Espeon.Commands {
	public class MultiCriteria<T> : ICriterion<T> {
		public IEnumerable<ICriterion<T>> Criteria;

		public MultiCriteria(params ICriterion<T>[] criteria) {
			this.Criteria = criteria ?? throw new ArgumentNullException(nameof(criteria));
		}

		public async ValueTask<bool> JudgeAsync(EspeonContext context, T entity) {
			foreach (ICriterion<T> criterion in this.Criteria) {
				bool result = await criterion.JudgeAsync(context, entity);
				if (!result) {
					return false;
				}
			}

			return true;
		}
	}
}