using System.Linq;
using System.Text.RegularExpressions;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Wpf.Configurations;
using ApplicationPatcher.Wpf.Extensions;

namespace ApplicationPatcher.Wpf.Services.NameRules.Specific {
	public class FirstUpperNameRules : SpecificNameRulesService {
		public override NameRulesType NameRulesType => NameRulesType.First_upper;

		public FirstUpperNameRules() : base(@"(?<FirstWord>[A-Z][a-z\d]*)(_(?<LastWords>[a-z\d]+))*_?") {
		}

		protected override string[] GetNameWordsFromMatch(Match match) {
			return match.GetValues("FirstWord").Concat(match.GetValues("LastWords")).Concat(match.GetValues("LastWord")).Where(word => !word.IsNullOrEmpty()).ToArray();
		}

		protected override string CompileNameWithoutPrefixAndSuffix(string[] nameWords) {
			return nameWords.Take(1)
				.Select(word => $"{word.First().ToUpper()}{word.Substring(1).ToLower()}")
				.Concat(nameWords.Skip(1)
					.Select(word => word.ToLower()))
				.JoinToString("_");
		}
	}
}