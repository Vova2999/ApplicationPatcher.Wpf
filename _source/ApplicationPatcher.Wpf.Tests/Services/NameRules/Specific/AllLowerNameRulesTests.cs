using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Wpf.Configurations;
using ApplicationPatcher.Wpf.Services.NameRules;
using ApplicationPatcher.Wpf.Services.NameRules.Specific;
using NUnit.Framework;

namespace ApplicationPatcher.Wpf.Tests.Services.NameRules.Specific {
	[TestFixture]
	public class AllLowerNameRulesTests : SpecificNameRulesServiceTestsBase {
		protected override NameRulesType NameRulesType => NameRulesType.all_lower;

		protected override SpecificNameRulesService CreateSpecificNameRulesService(string prefix, string suffix) {
			return new AllLowerNameRules(prefix, suffix);
		}

		[Test]
		public void InvalidNames_AdditionalSymbols() {
			var invalidNames = new[] { "_this_is_invalid_name", "this__is_invalid_name", "this_is__invalid_name", "this_is_invalid__name", "this_is_invalid_name__" };
			invalidNames.ForEach(invalidName => CheckInvalidName(invalidName, null, null));
			CheckValidName("this_is_valid_name", null, null);
			CheckValidName("this_is_valid_name_", null, null);
		}

		[Test]
		public void InvalidNames_LowerSymbols() {
			var invalidNames = new[] { "This_is_invalid_name", "tHis_is_invalid_name", "this_Is_invalid_name", "this_iS_invalid_name", "this_is_invalid_namE" };
			invalidNames.ForEach(invalidName => CheckInvalidName(invalidName, null, null));
			CheckValidName("this_is_valid_name", null, null);
			CheckValidName("this_is_valid_name_", null, null);
		}

		[Test]
		public void InvalidNames_IncorrectPrefix() {
			var invalidNames = new[] { "this_is_invalid_name", "_this_is_invalid_name", "pre_this_is_invalid_name", "this_is_invalid_name_", "this_is_invalid_name_prefix_" };
			invalidNames.ForEach(invalidName => CheckInvalidName(invalidName, "_prefix_", null));
			CheckValidName("_prefix_this_is_valid_name", "_prefix_", null);
		}

		[Test]
		public void InvalidNames_IncorrectPrefix_IncorrectSuffix() {
			var invalidNames = new[] { "this_is_invalid_name", "_this_is_invalid_name", "this_is_invalid_name_", "_this_is_invalid_name_", "_suffix_this_is_invalid_name_prefix_" };
			invalidNames.ForEach(invalidName => CheckInvalidName(invalidName, "_prefix_", "_suffix_"));
			CheckValidName("_prefix_this_is_valid_name_suffix_", "_prefix_", "_suffix_");
		}

		[Test]
		public void InvalidNames_IncorrectSuffix() {
			var invalidNames = new[] { "this_is_invalid_name", "this_is_invalid_name_", "this_is_invalid_name_suf", "_this_is_invalid_name", "_suffix_this_is_invalid_name" };
			invalidNames.ForEach(invalidName => CheckInvalidName(invalidName, null, "_suffix_"));
			CheckValidName("this_is_valid_name_suffix_", null, "_suffix_");
		}

		[Test]
		public void InvalidNames_StartsWithDigit() {
			var invalidNames = new[] { "4this_is_invalid_name", "3his_is_invalid_name", "12this_is_invalid_name", "0this_is_invalid_name", "9this_is_invalid_name" };
			invalidNames.ForEach(invalidName => CheckInvalidName(invalidName, null, null));
			CheckValidName("this_is_valid_name", null, null);
			CheckValidName("this_is_valid_name_", null, null);
		}
	}
}