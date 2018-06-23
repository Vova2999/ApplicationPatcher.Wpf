﻿using System.Linq;
using System.Text.RegularExpressions;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Wpf.Extensions;

namespace ApplicationPatcher.Wpf.Services.NameRules.Specific {
	public class UpperCamelCaseNameRules : SpecificNameRulesService {
		public UpperCamelCaseNameRules(string prefix, string suffix) : base(prefix, suffix, @"(?<FirstWords>[A-Z][a-z\d]+)*(?<LastWord>[A-Z])?") {
		}

		protected override string[] GetNameWordsFromMatch(Match match) {
			return match.GetValues("FirstWords").Concat(match.GetValues("LastWord")).Where(word => !word.IsNullOrEmpty()).ToArray();
		}

		protected override string CompileNameWithoutPrefixAndSuffix(string[] nameWords) {
			return nameWords
				.Select(word => $"{word.First().ToUpper()}{word.Substring(1).ToLower()}")
				.JoinToString(string.Empty);
		}
	}
}