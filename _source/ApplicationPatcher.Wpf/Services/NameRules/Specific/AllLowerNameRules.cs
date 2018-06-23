using System.Linq;
using System.Text.RegularExpressions;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Wpf.Extensions;

namespace ApplicationPatcher.Wpf.Services.NameRules.Specific {
	public class AllLowerNameRules : SpecificNameRulesService {
		public AllLowerNameRules(string prefix, string suffix) : base(prefix, suffix, @"(?<FirstWord>[a-z\d]+)(_($|(?<LastWords>[a-z\d]+)))*") {
		}

		protected override string[] GetNameWordsFromMatch(Match match) {
			return match.GetValues("FirstWord").Concat(match.GetValues("LastWords")).ToArray();
		}

		protected override string CompileNameWithoutPrefixAndSuffix(string[] nameWords) {
			return nameWords.Select(word => word.ToLower()).JoinToString("_");
		}
	}
}