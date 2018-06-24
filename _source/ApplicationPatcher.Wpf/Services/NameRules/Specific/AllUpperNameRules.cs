using System.Linq;
using System.Text.RegularExpressions;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Wpf.Configurations;
using ApplicationPatcher.Wpf.Extensions;

namespace ApplicationPatcher.Wpf.Services.NameRules.Specific {
	public class AllUpperNameRules : SpecificNameRulesService {
		public override NameRulesType NameRulesType => NameRulesType.ALL_UPPER;

		public AllUpperNameRules() : base(@"(?<FirstWord>[A-Z][A-Z\d]*)(_(?<LastWords>[A-Z\d]+))*_?") {
		}

		protected override string[] GetNameWordsFromMatch(Match match) {
			return match.GetValues("FirstWord").Concat(match.GetValues("LastWords")).Where(word => !word.IsNullOrEmpty()).ToArray();
		}

		protected override string CompileNameWithoutPrefixAndSuffix(string[] nameWords) {
			return nameWords.Select(word => word.ToUpper()).JoinToString("_");
		}
	}
}