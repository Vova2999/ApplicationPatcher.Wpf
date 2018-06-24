using System;

namespace ApplicationPatcher.Wpf.Types.Attributes {
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
	public class NotUseSearchByName : Attribute {
	}
}