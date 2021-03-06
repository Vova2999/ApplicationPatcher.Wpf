﻿using System;
using System.Linq;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Wpf.Services;

namespace ApplicationPatcher.Wpf.Exceptions {
	public class ViewModelCommandPatchingException : Exception {
		public ViewModelCommandPatchingException(string message) : base($"Internal errors of view model command patching:\n{message}") {
		}
		public ViewModelCommandPatchingException(ErrorsService errorsService) : this(errorsService.Errors.Select((error, i) => $"  {i + 1}) {error}").JoinToString("\n")) {
		}
	}
}