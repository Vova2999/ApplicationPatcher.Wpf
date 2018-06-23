using ApplicationPatcher.Wpf.Configurations;
using ApplicationPatcher.Wpf.Factories;

namespace ApplicationPatcher.Wpf.Services.NameRules {
	public class NameRulesService {
		private readonly ApplicationPatcherWpfConfiguration applicationPatcherWpfConfiguration;
		private readonly SpecificNameRulesServiceFactory specificNameRulesServiceFactory;

		private SpecificNameRulesService SpecificNameRulesServiceForField => specificNameRulesServiceFactory.Create(applicationPatcherWpfConfiguration.FieldNameRules);
		private SpecificNameRulesService SpecificNameRulesServiceForProperty => specificNameRulesServiceFactory.Create(applicationPatcherWpfConfiguration.PropertyNameRules);
		private SpecificNameRulesService SpecificNameRulesServiceForExecuteMethod => specificNameRulesServiceFactory.Create(applicationPatcherWpfConfiguration.ExecuteMethodNameRules);
		private SpecificNameRulesService SpecificNameRulesServiceForCanExecuteMethod => specificNameRulesServiceFactory.Create(applicationPatcherWpfConfiguration.CanExecuteMethodNameRules);

		public NameRulesService(ApplicationPatcherWpfConfiguration applicationPatcherWpfConfiguration, SpecificNameRulesServiceFactory specificNameRulesServiceFactory) {
			this.applicationPatcherWpfConfiguration = applicationPatcherWpfConfiguration;
			this.specificNameRulesServiceFactory = specificNameRulesServiceFactory;
		}

		public bool IsFieldNameValid(string name) {
			return SpecificNameRulesServiceForField.IsNameValid(name);
		}
		public bool IsPropertyNameValid(string name) {
			return SpecificNameRulesServiceForProperty.IsNameValid(name);
		}
		public bool IsExecuteMethodNameValid(string name) {
			return SpecificNameRulesServiceForExecuteMethod.IsNameValid(name);
		}
		public bool IsCanExecuteMethodNameValid(string name) {
			return SpecificNameRulesServiceForCanExecuteMethod.IsNameValid(name);
		}

		public string ConvertFieldNameToPropertyName(string name) {
			return SpecificNameRulesServiceForProperty.CompileName(SpecificNameRulesServiceForField.GetNameWords(name));
		}
	}
}