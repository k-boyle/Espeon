using System.Threading.Tasks;

namespace Espeon.Commands {
	public interface ICriterion<in T> {
		ValueTask<bool> JudgeAsync(EspeonContext context, T entity);
	}
}