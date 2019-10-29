using Espeon.Core.Database;
using Espeon.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Commands {
	public class RequireRangeAttribute : EspeonParameterCheckBase {
		private readonly int _minValue;
		private readonly int _maxValue;

		public RequireRangeAttribute(int minValue) : this(minValue, int.MaxValue) { }

		public RequireRangeAttribute(int minValue, int maxValue) {
			if (maxValue <= minValue) {
				throw new ArgumentOutOfRangeException($"{nameof(maxValue)} must be greater than {nameof(minValue)}");
			}

			this._minValue = minValue;
			this._maxValue = maxValue;
		}

		public override ValueTask<CheckResult> CheckAsync(object argument, EspeonContext context,
			IServiceProvider provider) {
			var value = (int) argument;

			if (value >= this._minValue && value < this._maxValue) {
				return CheckResult.Successful;
			}

			var response = provider.GetService<IResponseService>();

			User user = context.Invoker;

			return CheckResult.Unsuccessful(response.GetResponse(this, user.ResponsePack, 0, this._minValue,
				this._maxValue));
		}
	}
}