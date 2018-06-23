using System;
using System.Linq;
using System.Text.RegularExpressions;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Wpf.Extensions;

namespace ApplicationPatcher.Wpf.Services.NameRules.Specific {
	public class AllUpperNameRules : SpecificNameRulesService {
		public AllUpperNameRules(string prefix, string suffix) : base(prefix, suffix, @"(?<FirstWord>[A-Z\d]+)(_($|(?<LastWords>[A-Z\d]+)))*") {
		}

		protected override string[] GetNameWordsFromMatch(Match match) {
			return match.GetValues("FirstWord").Concat(match.GetValues("LastWords")).ToArray();
		}

		protected override string CompileNameWithoutPrefixAndSuffix(string[] nameWords) {
			return nameWords.Select(word => word.ToUpper()).JoinToString("_");
		}
	}
}