using System;

namespace ApplicationPatcher.Wpf.Types.Attributes.Properties {
	[AttributeUsage(AttributeTargets.Property)]
	public class NotPatchingPropertyAttribute : Attribute {
	}
}