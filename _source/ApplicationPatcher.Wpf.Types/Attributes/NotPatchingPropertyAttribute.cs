using System;

namespace ApplicationPatcher.Wpf.Types.Attributes {
	[AttributeUsage(AttributeTargets.Property)]
	public class NotPatchingPropertyAttribute : Attribute {
	}
}