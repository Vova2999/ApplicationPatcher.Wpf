using ApplicationPatcher.Core.Types.CommonMembers;

namespace ApplicationPatcher.Wpf.Services.PropertyGrouper {
	public class PropertyGroup {
		public readonly CommonField Field;
		public readonly CommonProperty Property;

		public PropertyGroup(CommonField field, CommonProperty property) {
			Field = field;
			Property = property;
		}
	}
}