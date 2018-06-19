using System;

namespace ApplicationPatcher.Wpf.Types.Attributes {
	[AttributeUsage(AttributeTargets.Method)]
	public class NotPatchingCommandAttribute : Attribute {
	}
}