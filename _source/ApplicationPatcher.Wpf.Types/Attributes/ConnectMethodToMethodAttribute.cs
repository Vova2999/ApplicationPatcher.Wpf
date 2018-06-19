using System;
using ApplicationPatcher.Wpf.Types.Enums;

namespace ApplicationPatcher.Wpf.Types.Attributes {
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class ConnectMethodToMethodAttribute : Attribute {
		public readonly MethodType ThisMethodType;
		public readonly string ConnectingMethodName;

		public ConnectMethodToMethodAttribute(string connectingMethodName, MethodType thisMethodType = MethodType.Execute) {
			ThisMethodType = thisMethodType;
			ConnectingMethodName = connectingMethodName;
		}
	}
}