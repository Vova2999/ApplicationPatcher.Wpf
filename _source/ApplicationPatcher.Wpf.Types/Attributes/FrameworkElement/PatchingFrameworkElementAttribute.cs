using System;
using ApplicationPatcher.Wpf.Types.Enums;

// ReSharper disable ClassNeverInstantiated.Global

namespace ApplicationPatcher.Wpf.Types.Attributes.FrameworkElement {
	[AttributeUsage(AttributeTargets.Class)]
	public class PatchingFrameworkElementAttribute : Attribute {
		public readonly FrameworkElementPatchingType PatchingType;

		public PatchingFrameworkElementAttribute(FrameworkElementPatchingType patchingType = FrameworkElementPatchingType.All) {
			PatchingType = patchingType;
		}
	}
}