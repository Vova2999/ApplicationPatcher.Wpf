using System;
using System.Linq;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Wpf.Services;

// ReSharper disable MemberCanBePrivate.Global

namespace ApplicationPatcher.Wpf.Exceptions {
	public class ViewModelPropertyPatchingException : Exception {
		public ViewModelPropertyPatchingException(string message) : base($"Internal errors of property patching:\n{message}") {
		}
		public ViewModelPropertyPatchingException(ErrorsService errorsService) : this(errorsService.Errors.Select((error, i) => $"  {i + 1}) {error}").JoinToString("\n")) {
		}
	}
}