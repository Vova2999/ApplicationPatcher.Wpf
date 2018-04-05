using System;
using ApplicationPatcher.Wpf.Types.Enums;

namespace ApplicationPatcher.Wpf.Types.Attributes.ViewModels {
	[AttributeUsage(AttributeTargets.Class)]
	public class PatchingViewModelAttribute : Attribute {
		public readonly ViewModelPatchingType ViewModelPatchingType;

		public PatchingViewModelAttribute(ViewModelPatchingType viewModelPatchingType = ViewModelPatchingType.All) {
			ViewModelPatchingType = viewModelPatchingType;
		}
	}
}