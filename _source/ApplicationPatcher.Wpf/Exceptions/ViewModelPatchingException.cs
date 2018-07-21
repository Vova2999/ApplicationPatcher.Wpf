using System;
using System.Linq;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Wpf.Services;

// ReSharper disable MemberCanBePrivate.Global

namespace ApplicationPatcher.Wpf.Exceptions {
	public class ViewModelPatchingException : Exception {
		public ViewModelPatchingException(string message) : base($"Internal errors of view model patching:\n{message}") {
		}
		public ViewModelPatchingException(ErrorsService errorsService) : this(errorsService.Errors.Select((error, i) => $"  {i + 1}) {error}").JoinToString("\n")) {
		}
	}
}