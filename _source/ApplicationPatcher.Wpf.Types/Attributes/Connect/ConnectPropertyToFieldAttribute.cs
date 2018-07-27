using System;

namespace ApplicationPatcher.Wpf.Types.Attributes.Connect {
	[AttributeUsage(AttributeTargets.Property)]
	public class ConnectPropertyToFieldAttribute : Attribute {
		public readonly string ConnectingFieldName;

		public ConnectPropertyToFieldAttribute(string connectingFieldName) {
			ConnectingFieldName = connectingFieldName;
		}
	}
}