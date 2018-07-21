using System;
using System.Linq;
using ApplicationPatcher.Wpf.Configurations;

namespace ApplicationPatcher.Wpf.Services.NameRules {
	public class NameRulesService {
		private readonly ApplicationPatcherWpfConfiguration applicationPatcherWpfConfiguration;
		private readonly SpecificNameRulesService[] specificNameRulesServices;

		public NameRulesService(ApplicationPatcherWpfConfiguration applicationPatcherWpfConfiguration, SpecificNameRulesService[] specificNameRulesServices) {
			this.applicationPatcherWpfConfiguration = applicationPatcherWpfConfiguration;
			this.specificNameRulesServices = specificNameRulesServices;
		}

		public bool IsNameValid(string name, UseNameRulesFor useNameRulesFor) {
			var nameRules = GetNameRules(useNameRulesFor);
			return GetSpecificNameRulesService(nameRules.Type).IsNameValid(name, nameRules.Prefix, nameRules.Suffix);
		}

		public string ConvertName(string name, UseNameRulesFor from, UseNameRulesFor to) {
			var fromNameRules = GetNameRules(from);
			var toNameRules = GetNameRules(to);

			var fromSpecificNameRulesService = GetSpecificNameRulesService(fromNameRules.Type);
			var toSpecificNameRulesService = GetSpecificNameRulesService(toNameRules.Type);

			var nameWords = fromSpecificNameRulesService.GetNameWords(name, fromNameRules.Prefix, fromNameRules.Suffix);
			return toSpecificNameRulesService.CompileName(nameWords, toNameRules.Prefix, toNameRules.Suffix);
		}

		private Configurations.NameRules GetNameRules(UseNameRulesFor useNameRulesFor) {
			switch (useNameRulesFor) {
				case UseNameRulesFor.Field:
					return applicationPatcherWpfConfiguration.FieldNameRules;
				case UseNameRulesFor.Property:
					return applicationPatcherWpfConfiguration.PropertyNameRules;
				case UseNameRulesFor.CommandField:
					return applicationPatcherWpfConfiguration.CommandFieldNameRules;
				case UseNameRulesFor.CommandProperty:
					return applicationPatcherWpfConfiguration.CommandPropertyNameRules;
				case UseNameRulesFor.DependencyField:
					return applicationPatcherWpfConfiguration.DependencyFieldNameRules;
				case UseNameRulesFor.DependencyProperty:
					return applicationPatcherWpfConfiguration.DependencyPropertyNameRules;
				case UseNameRulesFor.CommandExecuteMethod:
					return applicationPatcherWpfConfiguration.CommandExecuteMethodNameRules;
				case UseNameRulesFor.CommandCanExecuteMethod:
					return applicationPatcherWpfConfiguration.CommandCanExecuteMethodNameRules;
				default:
					throw new ArgumentOutOfRangeException(nameof(useNameRulesFor), useNameRulesFor, null);
			}
		}

		private SpecificNameRulesService GetSpecificNameRulesService(NameRulesType nameRulesType) {
			return specificNameRulesServices.FirstOrDefault(service => service.NameRulesType == nameRulesType)
				?? throw new ArgumentOutOfRangeException(nameof(nameRulesType), nameRulesType, "Not implement name rules");
		}
	}
}