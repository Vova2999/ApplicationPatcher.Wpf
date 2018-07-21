using System;

namespace ApplicationPatcher.Wpf.Types.Attributes.ViewModel {
	[AttributeUsage(AttributeTargets.Class)]
	public class NotPatchingViewModelAttribute : Attribute {
	}
}