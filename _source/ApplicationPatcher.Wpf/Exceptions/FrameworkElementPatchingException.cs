﻿using System;
using System.Linq;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Wpf.Services;

namespace ApplicationPatcher.Wpf.Exceptions {
	public class FrameworkElementPatchingException : Exception {
		public FrameworkElementPatchingException(string message) : base($"Internal errors of framework element patching:\n{message}") {
		}
		public FrameworkElementPatchingException(ErrorsService errorsService) : this(errorsService.Errors.Select((error, i) => $"  {i + 1}) {error}").JoinToString("\n")) {
		}
	}
}