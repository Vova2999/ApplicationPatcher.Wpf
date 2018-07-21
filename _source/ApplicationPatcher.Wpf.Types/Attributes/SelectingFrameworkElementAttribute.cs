using System;
using ApplicationPatcher.Wpf.Types.Enums;

namespace ApplicationPatcher.Wpf.Types.Attributes {
	[AttributeUsage(AttributeTargets.Assembly)]
	public class SelectingFrameworkElementAttribute : Attribute {
		public readonly FrameworkElementSelectingType SelectingType;

		public SelectingFrameworkElementAttribute(FrameworkElementSelectingType selectingType = FrameworkElementSelectingType.All) {
			SelectingType = selectingType;
		}
	}
}