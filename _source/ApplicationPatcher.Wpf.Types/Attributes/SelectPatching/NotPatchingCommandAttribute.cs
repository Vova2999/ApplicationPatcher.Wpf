using System;

namespace ApplicationPatcher.Wpf.Types.Attributes.SelectPatching {
	[AttributeUsage(AttributeTargets.Method)]
	public class NotPatchingCommandAttribute : Attribute {
	}
}