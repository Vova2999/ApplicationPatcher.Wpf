using System;

namespace ApplicationPatcher.Wpf.Types.Attributes.FrameworkElement {
	[AttributeUsage(AttributeTargets.Property)]
	public class NotPatchingDependencyPropertyAttribute : Attribute {
	}
}