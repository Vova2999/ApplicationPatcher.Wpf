using System.Collections.Generic;
using System.Linq;

namespace ApplicationPatcher.Wpf.Services {
	public class ErrorsService {
		public bool HasErrors => errors.Any();
		public IEnumerable<string> Errors => errors;

		private readonly List<string> errors = new List<string>();

		public ErrorsService AddError(string errorMessage) {
			errors.Add(errorMessage);
			return this;
		}

		public ErrorsService AddErrors(IEnumerable<string> errorMessages) {
			errors.AddRange(errorMessages);
			return this;
		}
	}
}