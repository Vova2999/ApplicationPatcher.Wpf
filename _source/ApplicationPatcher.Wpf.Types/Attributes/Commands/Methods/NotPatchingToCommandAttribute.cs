using System;

namespace ApplicationPatcher.Wpf.Types.Attributes.Commands.Methods {
	[AttributeUsage(AttributeTargets.Method)]
	public class NotPatchingToCommandAttribute : Attribute {
	}
}