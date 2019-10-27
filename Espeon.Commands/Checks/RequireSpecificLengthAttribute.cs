using Espeon.Core.Databases;
using Espeon.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Commands {
	public class RequireSpecificLengthAttribute : EspeonParameterCheckBase {
		private readonly int _minLength;
		private readonly int _maxLength;

		public RequireSpecificLengthAttribute(int maxLength) : this(0, maxLength) { }

		public RequireSpecificLengthAttribute(int minLength, int maxLength) {
			this._minLength = minLength;
			this._maxLength = maxLength;
		}

		public override ValueTask<CheckResult> CheckAsync(object argument, EspeonContext context,
			IServiceProvider provider) {
			string str = argument.ToString();

			if (str.Length > this._minLength && str.Length < this._maxLength) {
				return CheckResult.Successful;
			}

			var response = provider.GetService<IResponseService>();

			User user = context.Invoker;

			return CheckResult.Unsuccessful(response.GetResponse(this, user.ResponsePack, 0, this._minLength,
				this._maxLength));
		}
	}
}