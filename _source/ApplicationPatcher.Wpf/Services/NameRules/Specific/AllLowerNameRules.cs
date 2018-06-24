using System.Linq;
using System.Text.RegularExpressions;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Wpf.Configurations;
using ApplicationPatcher.Wpf.Extensions;

namespace ApplicationPatcher.Wpf.Services.NameRules.Specific {
	public class AllLowerNameRules : SpecificNameRulesService {
		public override NameRulesType NameRulesType => NameRulesType.all_lower;

		public AllLowerNameRules() : base(@"(?<FirstWord>[a-z][a-z\d]*)(_(?<LastWords>[a-z\d]+))*_?") {
		}

		protected override string[] GetNameWordsFromMatch(Match match) {
			return match.GetValues("FirstWord").Concat(match.GetValues("LastWords")).ToArray();
		}

		protected override string CompileNameWithoutPrefixAndSuffix(string[] nameWords) {
			return nameWords.Select(word => word.ToLower()).JoinToString("_");
		}
	}
}