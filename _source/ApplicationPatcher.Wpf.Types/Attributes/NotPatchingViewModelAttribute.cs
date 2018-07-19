using System;

namespace ApplicationPatcher.Wpf.Types.Attributes {
	[AttributeUsage(AttributeTargets.Class)]
	public class NotPatchingViewModelAttribute : Attribute {
	}
}