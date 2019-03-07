using Espeon.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Espeon.Interactive.Criteria
{
    public class MultiCriteria<T> : ICriterion<T>
    {
        public IEnumerable<ICriterion<T>> Criteria;

        public MultiCriteria(params ICriterion<T>[] criteria)
        {
            Criteria = criteria ?? throw new ArgumentNullException(nameof(criteria));
        }

        public async Task<bool> JudgeAsync(EspeonContext context, T entity)
        {
            foreach (var criterion in Criteria)
            {
                var result = await criterion.JudgeAsync(context, entity);
                if (!result)
                    return false;
            }

            return true;
        }
    }
}
