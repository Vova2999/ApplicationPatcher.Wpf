using System;

// ReSharper disable ClassNeverInstantiated.Global

namespace ApplicationPatcher.Wpf.Types.Attributes.FrameworkElement {
	[AttributeUsage(AttributeTargets.Class)]
	public class NotPatchingFrameworkElementAttribute : Attribute {
	}
}