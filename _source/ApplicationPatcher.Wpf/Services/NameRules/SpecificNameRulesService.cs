using System;
using System.Text;
using System.Text.RegularExpressions;

namespace ApplicationPatcher.Wpf.Services.NameRules {
	public abstract class SpecificNameRulesService {
		private readonly string prefix;
		private readonly string suffix;
		private readonly string pattern;

		protected SpecificNameRulesService(string prefix, string suffix, string shortPattern) {
			this.prefix = prefix;
			this.suffix = suffix;

			pattern = $"^{prefix ?? string.Empty}{shortPattern}{suffix ?? string.Empty}$";
		}

		public bool IsNameValid(string name) {
			return Regex.IsMatch(name, pattern);
		}

		public string[] GetNameWords(string name) {
			var match = Regex.Match(name, pattern);
			return match.Success ? GetNameWordsFromMatch(match) : throw new InvalidOperationException($"Name '{name}' is invalid by pattern '{pattern}'");
		}

		protected abstract string[] GetNameWordsFromMatch(Match match);

		public string CompileName(string[] nameWords) {
			var stringBuilder = new StringBuilder();
			stringBuilder.Append(prefix);
			stringBuilder.Append(CompileNameWithoutPrefixAndSuffix(nameWords));
			stringBuilder.Append(suffix);

			return stringBuilder.ToString();
		}

		protected abstract string CompileNameWithoutPrefixAndSuffix(string[] nameWords);
	}
}