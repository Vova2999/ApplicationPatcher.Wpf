﻿using System;

namespace ApplicationPatcher.Wpf.Types.Attributes.Connect {
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
	public class ConnectFieldToPropertyAttribute : Attribute {
		public readonly string ConnectingPropertyName;

		public ConnectFieldToPropertyAttribute(string connectingPropertyName) {
			ConnectingPropertyName = connectingPropertyName;
		}
	}
}