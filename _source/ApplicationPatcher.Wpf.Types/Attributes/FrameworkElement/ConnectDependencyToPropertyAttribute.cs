using System;

namespace ApplicationPatcher.Wpf.Types.Attributes.FrameworkElement {
	[AttributeUsage(AttributeTargets.Field)]
	public class ConnectDependencyToPropertyAttribute : Attribute {
		public readonly string ConnectingPropertyName;

		public ConnectDependencyToPropertyAttribute(string connectingPropertyName) {
			ConnectingPropertyName = connectingPropertyName;
		}
	}
}