using ApplicationPatcher.Core;
using ApplicationPatcher.Wpf.Types.Enums;

namespace ApplicationPatcher.Wpf.Configurations {
	public class ApplicationPatcherWpfConfiguration : ConfigurationFile<ApplicationPatcherWpfConfiguration> {
		protected override string ConfigurationFileName => "ApplicationPatcher.Wpf.config.xml";

		public NameRules FieldNameRules { get; set; }
		public NameRules PropertyNameRules { get; set; }
		public NameRules CommandFieldNameRules { get; set; }
		public NameRules CommandPropertyNameRules { get; set; }
		public NameRules CommandExecuteMethodNameRules { get; set; }
		public NameRules CommandCanExecuteMethodNameRules { get; set; }
		public ViewModelPatchingType DefaultViewModelPatchingType { get; set; }
	}
}