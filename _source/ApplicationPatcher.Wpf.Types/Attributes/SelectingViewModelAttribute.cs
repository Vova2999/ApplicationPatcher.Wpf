using System;
using ApplicationPatcher.Wpf.Types.Enums;

namespace ApplicationPatcher.Wpf.Types.Attributes {
	[AttributeUsage(AttributeTargets.Assembly)]
	public class SelectingViewModelAttribute : Attribute {
		public readonly ViewModelSelectingType SelectingType;

		public SelectingViewModelAttribute(ViewModelSelectingType selectingType = ViewModelSelectingType.All) {
			SelectingType = selectingType;
		}
	}
}