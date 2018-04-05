using System;

namespace ApplicationPatcher.Wpf.Exceptions {
	public class PropertyPatchingException : Exception {
		private const string errorMessage = "Internal error of property patching";

		public PropertyPatchingException(string message, string propertyFullName) : base(errorMessage, new Exception($"{message}, property name: {propertyFullName}")) {
		}
	}
}