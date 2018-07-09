using ApplicationPatcher.Wpf.Configurations;
using ApplicationPatcher.Wpf.Services.NameRules;
using ApplicationPatcher.Wpf.Services.NameRules.Specific;

namespace ApplicationPatcher.Wpf.Tests.Helpers {
	public static class NameRulesServiceHelper {
		public static NameRulesService CreateService(ApplicationPatcherWpfConfiguration configuration) {
			var specificNameRulesServices =
				new SpecificNameRulesService[] {
					new AllLowerNameRules(),
					new AllUpperNameRules(),
					new FirstUpperNameRules(),
					new LowerCamelCaseNameRules(),
					new UpperCamelCaseNameRules()
				};

			return new NameRulesService(configuration, specificNameRulesServices);
		}
	}
}