using ApplicationPatcher.Core.Types.CommonInterfaces;

namespace ApplicationPatcher.Wpf.Services.Groupers.Property {
	public class PropertyGroup {
		public readonly ICommonField Field;
		public readonly ICommonProperty Property;

		public readonly ICommonMethod CalledMethodBeforeGetProperty;
		public readonly ICommonMethod CalledMethodBeforeSetProperty;
		public readonly ICommonMethod CalledMethodAfterSuccessSetProperty;
		public readonly ICommonMethod CalledMethodAfterSetProperty;

		public PropertyGroup(ICommonField field,
							 ICommonProperty property,
							 ICommonMethod calledMethodBeforeGetProperty,
							 ICommonMethod calledMethodBeforeSetProperty,
							 ICommonMethod calledMethodAfterSuccessSetProperty,
							 ICommonMethod calledMethodAfterSetProperty) {
			Field = field;
			Property = property;
			CalledMethodBeforeGetProperty = calledMethodBeforeGetProperty;
			CalledMethodBeforeSetProperty = calledMethodBeforeSetProperty;
			CalledMethodAfterSuccessSetProperty = calledMethodAfterSuccessSetProperty;
			CalledMethodAfterSetProperty = calledMethodAfterSetProperty;
		}
	}
}