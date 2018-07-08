using System;

namespace ApplicationPatcher.Wpf.Types.Attributes {
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class ConnectMethodToMethodAttribute : Attribute {
		public readonly string ConnectingMethodName;

		public ConnectMethodToMethodAttribute(string connectingMethodName) {
			ConnectingMethodName = connectingMethodName;
		}
	}
}