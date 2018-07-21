using System;

namespace ApplicationPatcher.Wpf.Types.Attributes.ViewModel {
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
	public class NotUseSearchByNameAttribute : Attribute {
	}
}