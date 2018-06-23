using System;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Wpf.Services;

namespace ApplicationPatcher.Wpf.Exceptions {
	public class PropertyPatchingException : Exception {
		public PropertyPatchingException(string message) : base($"Internal error of property patching:\n{message}") {
		}
		public PropertyPatchingException(ErrorsService errorsService) : this(errorsService.Errors.JoinToString("\n")) {
		}
	}
}