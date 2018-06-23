using System;
using ApplicationPatcher.Wpf.Configurations;
using ApplicationPatcher.Wpf.Services.NameRules;
using ApplicationPatcher.Wpf.Services.NameRules.Specific;

namespace ApplicationPatcher.Wpf.Factories {
	public class SpecificNameRulesServiceFactory {
		public SpecificNameRulesService Create(NameRules nameRules) {
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