using System;

namespace ApplicationPatcher.Wpf.Types.Attributes.ViewModel {
	[AttributeUsage(AttributeTargets.Method)]
	public class NotPatchingCommandAttribute : Attribute {
	}
}