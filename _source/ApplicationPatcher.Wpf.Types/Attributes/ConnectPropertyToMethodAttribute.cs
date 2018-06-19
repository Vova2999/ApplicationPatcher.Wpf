using System;
using ApplicationPatcher.Wpf.Types.Enums;

namespace ApplicationPatcher.Wpf.Types.Attributes {
	[AttributeUsage(AttributeTargets.Method)]
	public class ConnectPropertyToMethodAttribute : Attribute {
		public readonly string ConnectingExecuteMethodName;
		public readonly string ConnectingCanExecuteMethodName;

		public ConnectPropertyToMethodAttribute(string connectingExecuteMethodName, string connectingCanExecuteMethodName) {
			ConnectingExecuteMethodName = connectingExecuteMethodName;
			ConnectingCanExecuteMethodName = connectingCanExecuteMethodName;
		}

		public ConnectPropertyToMethodAttribute(string connectingMethodName, MethodType methodType = MethodType.Execute) {
			switch (methodType) {
				case MethodType.Execute:
					ConnectingExecuteMethodName = connectingMethodName;
					break;
				case MethodType.CanExecute:
					ConnectingCanExecuteMethodName = connectingMethodName;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(methodType), methodType, null);
			}
		}
	}
}