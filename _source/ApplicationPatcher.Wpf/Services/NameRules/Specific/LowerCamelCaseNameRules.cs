using System.Linq;
using System.Text.RegularExpressions;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Wpf.Extensions;

namespace ApplicationPatcher.Wpf.Services.NameRules.Specific {
	public class LowerCamelCaseNameRules : SpecificNameRulesService {
		public LowerCamelCaseNameRules(string prefix, string suffix) : base(prefix, suffix, @"(?<FirstWord>[a-z]{1}($|[a-z\d]+))(?<LastWords>[A-Z]{1}($|[a-z\d]+))*") {
		}

		protected override string[] GetNameWordsFromMatch(Match match) {
			return match.GetValues("FirstWord").Concat(match.GetValues("LastWords")).ToArray();
		}

		protected override string CompileNameWithoutPrefixAndSuffix(string[] nameWords) {
			return nameWords.Take(1)
				.Select(word => word.ToLower())
				.Concat(nameWords.Skip(1)
					.Select(word => $"{word.First().ToUpper()}{word.Substring(1).ToLower()}"))
				.JoinToString(string.Empty);
		}
	}
}