using System;
using System.Linq;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Wpf.Services;

namespace ApplicationPatcher.Wpf.Exceptions {
	public class FrameworkElementDependencyPatchingException : Exception {
		public FrameworkElementDependencyPatchingException(string message) : base($"Internal errors of framework element dependency patching:\n{message}") {
		}
		public FrameworkElementDependencyPatchingException(ErrorsService errorsService) : this(errorsService.Errors.Select((error, i) => $"  {i + 1}) {error}").JoinToString("\n")) {
		}
	}
}