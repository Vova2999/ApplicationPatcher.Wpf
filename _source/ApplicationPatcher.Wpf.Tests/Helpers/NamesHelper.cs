using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Wpf.Configurations;

namespace ApplicationPatcher.Wpf.Tests.Helpers {
	public static class NamesHelper {
		private const int NameCount = 1000;
		private const int MaxWordCount = 5;
		private const int MaxWordLength = 5;
		private static readonly char[] LowerAlphabet = GetCharsInRange('a', 'z');
		private static readonly char[] UpperAlphabet = GetCharsInRange('A', 'Z');
		private static readonly char[] DigitAlphabet = GetCharsInRange('0', '9');
		private static readonly char[] LowerAndDigitAlphabet = LowerAlphabet.Concat(DigitAlphabet).ToArray();
		private static readonly char[] UpperAndDigitAlphabet = UpperAlphabet.Concat(DigitAlphabet).ToArray();

		private static char[] GetCharsInRange(char startChar, char endChar) {
			return Enumerable.Range(startChar, endChar - startChar + 1).Select(x => (char)x).ToArray();
		}

		public static IEnumerable<(string[] Words, string Name)> GetValidNames(Random random, NameRules nameRules) {
			switch (nameRules.Type) {
				case NameRulesType.all_lower:
					return GetValidNames(random, GetWordWhenAllLower, GetWordWhenAllLower, () => "_", nameRules.Prefix, nameRules.Suffix, false);
				case NameRulesType.ALL_UPPER:
					return GetValidNames(random, GetWordWhenAllUpper, GetWordWhenAllUpper, () => "_", nameRules.Prefix, nameRules.Suffix, false);
				case NameRulesType.First_upper:
					return GetValidNames(random, GetWordWhenFirstUpperAndOthersLower, GetWordWhenAllLower, () => "_", nameRules.Prefix, nameRules.Suffix, false);
				case NameRulesType.lowerCamelCase:
					return GetValidNames(random, GetWordWhenAllLower, GetWordWhenFirstUpperAndOthersLower, () => GetRandomChar(random, LowerAndDigitAlphabet).ToString(), nameRules.Prefix, nameRules.Suffix, true);
				case NameRulesType.UpperCamelCase:
					return GetValidNames(random, GetWordWhenFirstUpperAndOthersLower, GetWordWhenFirstUpperAndOthersLower, () => GetRandomChar(random, LowerAndDigitAlphabet).ToString(), nameRules.Prefix, nameRules.Suffix, true);
				default:
					throw new ArgumentOutOfRangeException(nameof(nameRules.Type), nameRules.Type, null);
			}
		}

		private static IEnumerable<(string[] Words, string Name)> GetValidNames(Random random, Func<Random, string> getFirstWord, Func<Random, string> getOthersWord, Func<string> getWordsSeparator, string prefix, string suffix, bool addWordsSeparatorToWords) {
			return Enumerable.Range(0, NameCount).Select(x => GetValidName(random, getFirstWord, getOthersWord, getWordsSeparator, prefix, suffix, addWordsSeparatorToWords));
		}

		private static (string[] Words, string Name) GetValidName(Random random, Func<Random, string> getFirstWord, Func<Random, string> getOthersWord, Func<string> getWordsSeparator, string prefix, string suffix, bool addWordsSeparatorToWords) {
			var words = new[] { getFirstWord(random) }.Concat(Enumerable.Range(0, random.Next(0, MaxWordCount)).Select(y => getOthersWord(random))).ToArray();
			if (!addWordsSeparatorToWords)
				return (words, $"{prefix}{words.JoinToString(getWordsSeparator())}{suffix}");

			var wordsSeparator = getWordsSeparator();
			for (var i = 0; i < words.Length - 1; i++)
				words[i] += wordsSeparator;

			return (words, $"{prefix}{words.JoinToString(string.Empty)}{suffix}");
		}

		private static string GetWordWhenAllLower(Random random) {
			return GetWord(random, LowerAlphabet, LowerAndDigitAlphabet);
		}

		private static string GetWordWhenAllUpper(Random random) {
			return GetWord(random, UpperAlphabet, UpperAndDigitAlphabet);
		}

		private static string GetWordWhenFirstUpperAndOthersLower(Random random) {
			return GetWord(random, UpperAlphabet, LowerAndDigitAlphabet);
		}

		private static string GetWord(Random random, IReadOnlyList<char> alphabetForFirstSymbol, IReadOnlyList<char> alphabetForOtherSymbols) {
			return new string(new[] { GetRandomChar(random, alphabetForFirstSymbol) }
				.Concat(Enumerable.Range(0, random.Next(0, MaxWordLength)).Select(x => GetRandomChar(random, alphabetForOtherSymbols)))
				.ToArray());
		}

		private static char GetRandomChar(Random random, IReadOnlyList<char> alphabet) {
			return alphabet[random.Next(0, alphabet.Count)];
		}
	}
}