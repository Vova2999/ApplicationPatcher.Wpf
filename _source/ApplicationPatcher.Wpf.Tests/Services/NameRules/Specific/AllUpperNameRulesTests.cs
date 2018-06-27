using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Wpf.Services.NameRules.Specific;
using NUnit.Framework;

namespace ApplicationPatcher.Wpf.Tests.Services.NameRules.Specific {
	[TestFixture]
	public class AllUpperNameRulesTests : SpecificNameRulesServiceTestsBase {
		public AllUpperNameRulesTests() : base(new AllUpperNameRules()) {
		}

		[Test]
		public void InvalidNames_AdditionalSymbols() {
			var invalidNames = new[] { "_THIS_IS_INVALID_NAME", "THIS__IS_INVALID_NAME", "THIS_IS__INVALID_NAME", "THIS_IS_INVALID__NAME", "THIS_IS_INVALID_NAME__" };
			invalidNames.ForEach(invalidName => CheckInvalidName(invalidName, null, null));
			CheckValidName("THIS_IS_VALID_NAME", null, null);
			CheckValidName("THIS_IS_VALID_NAME_", null, null);
		}

		[Test]
		public void InvalidNames_LowerSymbols() {
			var invalidNames = new[] { "tHIS_IS_INVALID_NAME", "THIs_IS_INVALID_NAME", "THIS_iS_INVALID_NAME", "THIS_Is_INVALID_NAME", "THIS_IS_INVALID_NAMe" };
			invalidNames.ForEach(invalidName => CheckInvalidName(invalidName, null, null));
			CheckValidName("THIS_IS_VALID_NAME", null, null);
			CheckValidName("THIS_IS_VALID_NAME_", null, null);
		}

		[Test]
		public void InvalidNames_StartsWithDigit() {
			var invalidNames = new[] { "4THIS_IS_INVALID_NAME", "3HIS_IS_INVALID_NAME", "12THIS_IS_INVALID_NAME", "0THIS_IS_INVALID_NAME", "9THIS_IS_INVALID_NAME" };
			invalidNames.ForEach(invalidName => CheckInvalidName(invalidName, null, null));
			CheckValidName("THIS_IS_VALID_NAME", null, null);
			CheckValidName("THIS_IS_VALID_NAME_", null, null);
		}

		[Test]
		public void InvalidNames_IncorrectPrefix() {
			var invalidNames = new[] { "THIS_IS_INVALID_NAME", "_THIS_IS_INVALID_NAME", "pre_THIS_IS_INVALID_NAME", "THIS_IS_INVALID_NAME_", "THIS_IS_INVALID_NAME_prefix_" };
			invalidNames.ForEach(invalidName => CheckInvalidName(invalidName, "_prefix_", null));
			CheckValidName("_prefix_THIS_IS_VALID_NAME", "_prefix_", null);
		}

		[Test]
		public void InvalidNames_IncorrectSuffix() {
			var invalidNames = new[] { "THIS_IS_INVALID_NAME", "THIS_IS_INVALID_NAME_", "THIS_IS_INVALID_NAME_suf", "_THIS_IS_INVALID_NAME", "_suffix_THIS_IS_INVALID_NAME" };
			invalidNames.ForEach(invalidName => CheckInvalidName(invalidName, null, "_suffix_"));
			CheckValidName("THIS_IS_VALID_NAME_suffix_", null, "_suffix_");
		}

		[Test]
		public void InvalidNames_IncorrectPrefixAndSuffix() {
			var invalidNames = new[] { "THIS_IS_INVALID_NAME", "_THIS_IS_INVALID_NAME", "THIS_IS_INVALID_NAME_", "_THIS_IS_INVALID_NAME_", "_suffix_THIS_IS_INVALID_NAME_prefix_" };
			invalidNames.ForEach(invalidName => CheckInvalidName(invalidName, "_prefix_", "_suffix_"));
			CheckValidName("_prefix_THIS_IS_VALID_NAME_suffix_", "_prefix_", "_suffix_");
		}
	}
}