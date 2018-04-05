using System;
using ApplicationPatcher.Wpf.Types.Enums;

namespace ApplicationPatcher.Wpf.Types.Attributes.Commands.Methods {
	[AttributeUsage(AttributeTargets.Method)]
	public class PatchingToCommandAttribute : Attribute {
		private readonly CommandMethodType? commandMethodType;
		private readonly string commandPropertyName;

		public PatchingToCommandAttribute(CommandMethodType? commandMethodType = null, string commandPropertyName = null) {
			this.commandMethodType = commandMethodType;
			this.commandPropertyName = commandPropertyName;
		}
	}
}