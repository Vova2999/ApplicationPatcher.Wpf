using System;
using System.Linq;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Wpf.Services;

namespace ApplicationPatcher.Wpf.Exceptions {
	public class PropertyPatchingException : Exception {
		public PropertyPatchingException(string message) : base($"Internal errors of property patching:\n{message}") {
		}
		public PropertyPatchingException(ErrorsService errorsService) : this(errorsService.Errors.Select((error, i) => $"  {i + 1}) {error}").JoinToString("\n")) {
		}
	}
}