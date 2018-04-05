using System;

namespace ApplicationPatcher.Wpf.Types.Attributes.Commands.Properties {
	[AttributeUsage(AttributeTargets.Property)]
	public class CombineWithMethodAttribute : Attribute {
		public readonly string ExecuteMethodName;
		public readonly string CanExecuteMethodName;

		public CombineWithMethodAttribute(string executeMethodName = null, string canExecuteMethodName = null) {
			ExecuteMethodName = executeMethodName;
			CanExecuteMethodName = canExecuteMethodName;
		}
	}
}