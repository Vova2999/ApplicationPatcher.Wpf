using System;
using ApplicationPatcher.Wpf.Types.Enums;

namespace ApplicationPatcher.Wpf.Types.Attributes.FrameworkElement {
	[AttributeUsage(AttributeTargets.Class)]
	public class PatchingFrameworkElementAttribute : Attribute {
		public readonly FrameworkElementPatchingType PatchingType;

		public PatchingFrameworkElementAttribute(FrameworkElementPatchingType patchingType = FrameworkElementPatchingType.All) {
			PatchingType = patchingType;
		}
	}
}