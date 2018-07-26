using System;
using ApplicationPatcher.Wpf.Types.Enums;

// ReSharper disable ClassNeverInstantiated.Global

namespace ApplicationPatcher.Wpf.Types.Attributes {
	[AttributeUsage(AttributeTargets.Assembly)]
	public class SelectingFrameworkElementAttribute : Attribute {
		public readonly FrameworkElementSelectingType SelectingType;

		public SelectingFrameworkElementAttribute(FrameworkElementSelectingType selectingType = FrameworkElementSelectingType.All) {
			SelectingType = selectingType;
		}
	}
}