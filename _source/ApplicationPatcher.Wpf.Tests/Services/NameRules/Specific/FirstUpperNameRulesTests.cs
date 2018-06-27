using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Wpf.Services.NameRules.Specific;
using NUnit.Framework;

namespace ApplicationPatcher.Wpf.Tests.Services.NameRules.Specific {
	[TestFixture]
	public class FirstUpperNameRulesTests : SpecificNameRulesServiceTestsBase {
		public FirstUpperNameRulesTests() : base(new FirstUpperNameRules()) {
		}

		[Test]
		public void InvalidNames_AdditionalSymbols() {
			var invalidNames = new[] { "_This_is_invalid_name", "This__is_invalid_name", "This_is__invalid_name", "This_is_invalid__name", "This_is_invalid_name__" };
			invalidNames.ForEach(invalidName => CheckInvalidName(invalidName, null, null));
			CheckValidName("This_is_valid_name", null, null);
			CheckValidName("This_is_valid_name_", null, null);
		}

		[Test]
		public void InvalidNames_UpperSymbols() {
			var invalidNames = new[] { "ThiS_is_invalid_name", "This_Is_invalid_name", "This_is_iNvalid_name", "This_is_invaliD_name", "This_is_invalid_namE" };
			invalidNames.ForEach(invalidName => CheckInvalidName(invalidName, null, null));
			CheckValidName("This_is_valid_name", null, null);
			CheckValidName("This_is_valid_name_", null, null);
		}

		[Test]
		public void InvalidNames_StartsWithDigit() {
			var invalidNames = new[] { "4This_is_invalid_name", "3this_is_invalid_name", "12this_is_invalid_name", "0This_is_invalid_name", "9This_is_invalid_name" };
			invalidNames.ForEach(invalidName => CheckInvalidName(invalidName, null, null));
			CheckValidName("This_is_valid_name", null, null);
			CheckValidName("This_is_valid_name_", null, null);
		}

		[Test]
		public void InvalidNames_IncorrectPrefix() {
			var invalidNames = new[] { "This_is_invalid_name", "_This_is_invalid_name", "pre_This_is_invalid_name", "This_is_invalid_name_", "This_is_invalid_name_prefix_" };
			invalidNames.ForEach(invalidName => CheckInvalidName(invalidName, "_prefix_", null));
			CheckValidName("_prefix_This_is_valid_name", "_prefix_", null);
		}

		[Test]
		public void InvalidNames_IncorrectSuffix() {
			var invalidNames = new[] { "This_is_invalid_name", "This_is_invalid_name_", "This_is_invalid_name_suf", "_This_is_invalid_name", "_suffix_This_is_invalid_name" };
			invalidNames.ForEach(invalidName => CheckInvalidName(invalidName, null, "_suffix_"));
			CheckValidName("This_is_valid_name_suffix_", null, "_suffix_");
		}

		[Test]
		public void InvalidNames_IncorrectPrefixAndSuffix() {
			var invalidNames = new[] { "This_is_invalid_name", "_This_is_invalid_name", "This_is_invalid_name_", "_This_is_invalid_name_", "_suffix_This_is_invalid_name_prefix_" };
			invalidNames.ForEach(invalidName => CheckInvalidName(invalidName, "_prefix_", "_suffix_"));
			CheckValidName("_prefix_This_is_valid_name_suffix_", "_prefix_", "_suffix_");
		}
	}
}