using ApplicationPatcher.Core.Types.CommonInterfaces;

namespace ApplicationPatcher.Wpf.Services.Groupers.Dependency {
	public class DependencyGroup {
		public readonly ICommonField Field;
		public readonly ICommonProperty Property;

		public DependencyGroup(ICommonField field, ICommonProperty property) {
			Field = field;
			Property = property;
		}
	}
}