using System;
using ApplicationPatcher.Wpf.Configurations;
using ApplicationPatcher.Wpf.Services.NameRules.Specific;

namespace ApplicationPatcher.Wpf.Services.NameRules {
	public class NameRulesService {
		private readonly ApplicationPatcherWpfConfiguration applicationPatcherWpfConfiguration;

		public NameRulesService(ApplicationPatcherWpfConfiguration applicationPatcherWpfConfiguration) {
			this.applicationPatcherWpfConfiguration = applicationPatcherWpfConfiguration;
		}

		public bool IsNameValid(string name, UseNameRulesFor useNameRulesFor) {
			return CreateSpecificNameRulesService(useNameRulesFor).IsNameValid(name);
		}

		public string ConvertName(string name, UseNameRulesFor from, UseNameRulesFor to) {
			return CreateSpecificNameRulesService(to).CompileName(CreateSpecificNameRulesService(from).GetNameWords(name));
		}

		private SpecificNameRulesService CreateSpecificNameRulesService(UseNameRulesFor useNameRulesFor) {
			switch (useNameRulesFor) {
				case UseNameRulesFor.Field:
					return CreateSpecificNameRulesService(applicationPatcherWpfConfiguration.FieldNameRules);
				case UseNameRulesFor.Property:
					return CreateSpecificNameRulesService(applicationPatcherWpfConfiguration.PropertyNameRules);
				case UseNameRulesFor.CommandField:
					return CreateSpecificNameRulesService(applicationPatcherWpfConfiguration.CommandFieldNameRules);
				case UseNameRulesFor.CommandProperty:
					return CreateSpecificNameRulesService(applicationPatcherWpfConfiguration.CommandPropertyNameRules);
				case UseNameRulesFor.CommandExecuteMethod:
					return CreateSpecificNameRulesService(applicationPatcherWpfConfiguration.CommandExecuteMethodNameRules);
				case UseNameRulesFor.CommandCanExecuteMethod:
					return CreateSpecificNameRulesService(applicationPatcherWpfConfiguration.CommandCanExecuteMethodNameRules);
				default:
					throw new ArgumentOutOfRangeException(nameof(useNameRulesFor), useNameRulesFor, null);
			}
		}

		private SpecificNameRulesService CreateSpecificNameRulesService(Configurations.NameRules nameRules) {
			switch (nameRules.Type) {
				case NameRulesType.all_lower:
					return new AllLowerNameRules(nameRules.Prefix, nameRules.Suffix);
				case NameRulesType.ALL_UPPER:
					return new AllUpperNameRules(nameRules.Prefix, nameRules.Suffix);
				case NameRulesType.First_upper:
					return new FirstUpperNameRules(nameRules.Prefix, nameRules.Suffix);
				case NameRulesType.lowerCamelCase:
					return new LowerCamelCaseNameRules(nameRules.Prefix, nameRules.Suffix);
				case NameRulesType.UpperCamelCase:
					return new UpperCamelCaseNameRules(nameRules.Prefix, nameRules.Suffix);
				default:
					throw new ArgumentOutOfRangeException(nameof(nameRules.Type), nameRules.Type, null);
			}
		}
	}
}