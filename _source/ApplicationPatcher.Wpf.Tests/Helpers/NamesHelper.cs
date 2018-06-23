using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Wpf.Configurations;

namespace ApplicationPatcher.Wpf.Tests.Helpers {
	public static class NamesHelper {
		private const int nameCount = 1000;
		private const int maxWordCount = 7;
		private const int maxWordLength = 7;
		private static readonly char[] lowerAlphabet = GetCharsInRange('a', 'z');
		private static readonly char[] upperAlphabet = GetCharsInRange('A', 'Z');
		private static readonly char[] digitAlphabet = GetCharsInRange('0', '9');
		private static readonly char[] lowerAndDigitAlphabet = lowerAlphabet.Concat(digitAlphabet).ToArray();
		private static readonly char[] upperAndDigitAlphabet = upperAlphabet.Concat(digitAlphabet).ToArray();

		private static char[] GetCharsInRange(char startChar, char endChar) {
			return Enumerable.Range(startChar, endChar - startChar + 1).Select(x => (char)x).ToArray();
		}

		public static IEnumerable<string> GetValidNames(Random random, NameRules nameRules) {
			switch (nameRules.Type) {
				case NameRulesType.all_lower:
					return GetValidNames(random, GetWordWhenAllLower, GetWordWhenAllLower, () => "_", nameRules.Prefix, nameRules.Suffix);
				case NameRulesType.ALL_UPPER:
					return GetValidNames(random, GetWordWhenAllUpper, GetWordWhenAllUpper, () => "_", nameRules.Prefix, nameRules.Suffix);
				case NameRulesType.First_upper:
					return GetValidNames(random, GetWordWhenFirstUpperAndOthersLower, GetWordWhenAllLower, () => "_", nameRules.Prefix, nameRules.Suffix);
				case NameRulesType.lowerCamelCase:
					return GetValidNames(random, GetWordWhenAllLower, GetWordWhenFirstUpperAndOthersLower, () => GetRandomChar(random, lowerAndDigitAlphabet).ToString(), nameRules.Prefix, nameRules.Suffix);
				case NameRulesType.UpperCamelCase:
					return GetValidNames(random, GetWordWhenFirstUpperAndOthersLower, GetWordWhenFirstUpperAndOthersLower, () => GetRandomChar(random, lowerAndDigitAlphabet).ToString(), nameRules.Prefix, nameRules.Suffix);
				default:
					throw new ArgumentOutOfRangeException(nameof(nameRules.Type), nameRules.Type, null);
			}
		}

		private static IEnumerable<string> GetValidNames(Random random, Func<Random, string> getFirstWord, Func<Random, string> getOthersWord, Func<string> getWordsSeparator, string prefix, string suffix) {
			return Enumerable.Range(0, nameCount)
				.Select(x => $"{prefix}{new[] { getFirstWord(random) }.Concat(Enumerable.Range(0, random.Next(0, maxWordCount - 1)).Select(y => getOthersWord(random))).JoinToString(getWordsSeparator())}{suffix}");
		}

		private static string GetWordWhenAllLower(Random random) {
			return GetWord(random, lowerAlphabet, lowerAndDigitAlphabet);
		}

		private static string GetWordWhenAllUpper(Random random) {
			return GetWord(random, upperAlphabet, upperAndDigitAlphabet);
		}

		private static string GetWordWhenFirstUpperAndOthersLower(Random random) {
			return GetWord(random, upperAlphabet, lowerAndDigitAlphabet);
		}

		private static string GetWord(Random random, IReadOnlyList<char> alphabetForFirstSymbol, IReadOnlyList<char> alphabetForOtherSymbols) {
			return new string(new[] { GetRandomChar(random, alphabetForFirstSymbol) }
				.Concat(Enumerable.Range(0, random.Next(0, maxWordLength - 1)).Select(x => GetRandomChar(random, alphabetForOtherSymbols)))
				.ToArray());
		}

		private static char GetRandomChar(Random random, IReadOnlyList<char> alphabet) {
			return alphabet[random.Next(0, alphabet.Count)];
		}
	}
}