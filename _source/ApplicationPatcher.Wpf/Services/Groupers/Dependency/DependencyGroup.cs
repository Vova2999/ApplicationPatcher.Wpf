using ApplicationPatcher.Core.Types.CommonMembers;

namespace ApplicationPatcher.Wpf.Services.Groupers.Dependency {
	public class DependencyGroup {
		public readonly CommonField Field;
		public readonly CommonProperty Property;

		public DependencyGroup(CommonField field, CommonProperty property) {
			Field = field;
			Property = property;
		}
	}
}