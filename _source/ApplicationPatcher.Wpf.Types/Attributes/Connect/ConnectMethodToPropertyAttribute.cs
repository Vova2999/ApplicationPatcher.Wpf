using System;

namespace ApplicationPatcher.Wpf.Types.Attributes.Connect {
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class ConnectMethodToPropertyAttribute : Attribute {
		public readonly string ConnectingPropertyName;

		public ConnectMethodToPropertyAttribute(string connectingPropertyName) {
			ConnectingPropertyName = connectingPropertyName;
		}
	}
}