using System;
using ApplicationPatcher.Wpf.Types.Enums;

namespace ApplicationPatcher.Wpf.Types.Attributes.ViewModel {
	[AttributeUsage(AttributeTargets.Class)]
	public class PatchingViewModelAttribute : Attribute {
		public readonly ViewModelPatchingType PatchingType;

		public PatchingViewModelAttribute(ViewModelPatchingType patchingType = ViewModelPatchingType.All) {
			PatchingType = patchingType;
		}
	}
}