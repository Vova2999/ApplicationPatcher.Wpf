﻿using System;
using ApplicationPatcher.Wpf.Types.Enums;

namespace ApplicationPatcher.Wpf.Types.Attributes {
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class ConnectMethodToPropertyAttribute : Attribute {
		public readonly MethodType MethodType;
		public readonly string ConnectingPropertyName;

		public ConnectMethodToPropertyAttribute(string connectingPropertyName, MethodType methodType = MethodType.Execute) {
			MethodType = methodType;
			ConnectingPropertyName = connectingPropertyName;
		}
	}
}