using System;

namespace ApplicationPatcher.Wpf.Types.Attributes.Connect {
	[AttributeUsage(AttributeTargets.Method)]
	public class ConnectPropertyToMethodAttribute : Attribute {
		public readonly string[] ConnectingMethodNames;

		public ConnectPropertyToMethodAttribute(string connectingMethodName) {
			ConnectingMethodNames = new[] { connectingMethodName };
		}
		public ConnectPropertyToMethodAttribute(string connectingFirstMethodName, string connectingSecondMethodName) {
			ConnectingMethodNames = new[] { connectingFirstMethodName, connectingSecondMethodName };
		}
	}
}