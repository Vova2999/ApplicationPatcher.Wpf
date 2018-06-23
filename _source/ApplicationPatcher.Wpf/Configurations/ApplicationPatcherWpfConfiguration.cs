using ApplicationPatcher.Core;
using ApplicationPatcher.Wpf.Types.Enums;

namespace ApplicationPatcher.Wpf.Configurations {
	public class ApplicationPatcherWpfConfiguration : ConfigurationFile<ApplicationPatcherWpfConfiguration> {
		protected override string ConfigurationFileName => "ApplicationPatcher.Wpf.config.xml";

		public NameRules FieldNameRules { get; set; }
		public NameRules PropertyNameRules { get; set; }
		public NameRules ExecuteMethodNameRules { get; set; }
		public NameRules CanExecuteMethodNameRules { get; set; }
		public ViewModelPatchingType DefaultViewModelPatchingType { get; set; }
	}
}