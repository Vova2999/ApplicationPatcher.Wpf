using System;
using System.Text.RegularExpressions;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Wpf.Configurations;

namespace ApplicationPatcher.Wpf.Services.NameRules {
	public abstract class SpecificNameRulesService {
		public abstract NameRulesType NameRulesType { get; }
		private readonly string shortPattern;

		protected SpecificNameRulesService(string shortPattern) {
			this.shortPattern = shortPattern;
		}

		public bool IsNameValid(string name, string prefix, string suffix) {
			return Regex.IsMatch(name, GetFullPattern(prefix, suffix));
		}

		public string[] GetNameWords(string name, string prefix, string suffix) {
			var match = Regex.Match(name, GetFullPattern(prefix, suffix));
			return match.Success
				? GetNameWordsFromMatch(match)
				: throw new InvalidOperationException($"Name '{name}' is invalid by NameRulesType '{NameRulesType}' with prefix '{prefix}' and suffix '{suffix}'");
		}

		protected abstract string[] GetNameWordsFromMatch(Match match);

		public string CompileName(string[] nameWords, string prefix, string suffix) {
			return $"{prefix.EmptyIfNull()}{CompileNameWithoutPrefixAndSuffix(nameWords)}{suffix.EmptyIfNull()}";
		}

		protected abstract string CompileNameWithoutPrefixAndSuffix(string[] nameWords);

		private string GetFullPattern(string prefix, string suffix) {
			return $"^{prefix.EmptyIfNull()}{shortPattern}{suffix.EmptyIfNull()}$";
		}
	}
}