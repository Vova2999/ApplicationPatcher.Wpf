using ApplicationPatcher.Core.Types.CommonMembers;

namespace ApplicationPatcher.Wpf.Services.Groupers.Property {
	public class PropertyGroup {
		public readonly CommonField Field;
		public readonly CommonProperty Property;

		public readonly CommonMethod CalledMethodBeforeGetProperty;
		public readonly CommonMethod CalledMethodBeforeSetProperty;
		public readonly CommonMethod CalledMethodAfterSuccessSetProperty;
		public readonly CommonMethod CalledMethodAfterSetProperty;

		public PropertyGroup(CommonField field,
							 CommonProperty property,
							 CommonMethod calledMethodBeforeGetProperty,
							 CommonMethod calledMethodBeforeSetProperty,
							 CommonMethod calledMethodAfterSuccessSetProperty,
							 CommonMethod calledMethodAfterSetProperty) {
			Field = field;
			Property = property;
			CalledMethodBeforeGetProperty = calledMethodBeforeGetProperty;
			CalledMethodBeforeSetProperty = calledMethodBeforeSetProperty;
			CalledMethodAfterSuccessSetProperty = calledMethodAfterSuccessSetProperty;
			CalledMethodAfterSetProperty = calledMethodAfterSetProperty;
		}
	}
}