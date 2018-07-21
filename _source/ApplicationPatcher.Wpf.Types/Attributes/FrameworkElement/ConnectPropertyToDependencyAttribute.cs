using System;

namespace ApplicationPatcher.Wpf.Types.Attributes.FrameworkElement {
	[AttributeUsage(AttributeTargets.Property)]
	public class ConnectPropertyToDependencyAttribute : Attribute {
		public readonly string ConnectingDependencyName;

		public ConnectPropertyToDependencyAttribute(string connectingDependencyName) {
			ConnectingDependencyName = connectingDependencyName;
		}
	}
}