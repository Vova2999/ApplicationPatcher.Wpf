using System.Linq;
using System.Text.RegularExpressions;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Wpf.Configurations;
using ApplicationPatcher.Wpf.Extensions;

namespace ApplicationPatcher.Wpf.Services.NameRules.Specific {
	public class LowerCamelCaseNameRules : SpecificNameRulesService {
		public override NameRulesType NameRulesType => NameRulesType.lowerCamelCase;

		public LowerCamelCaseNameRules() : base(@"(?<FirstWord>[a-z][a-z\d]*)(?<MiddleWords>[A-Z][a-z\d]+)*(?<LastWord>[A-Z])?") {
		}

		protected override string[] GetNameWordsFromMatch(Match match) {
			return match.GetValues("FirstWord").Concat(match.GetValues("MiddleWords")).Concat(match.GetValues("LastWord")).ToArray();
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