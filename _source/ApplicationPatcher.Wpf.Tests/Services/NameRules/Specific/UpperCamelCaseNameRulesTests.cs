using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Wpf.Services.NameRules.Specific;
using NUnit.Framework;

namespace ApplicationPatcher.Wpf.Tests.Services.NameRules.Specific {
	[TestFixture]
	public class UpperCamelCaseNameRulesTests : SpecificNameRulesServiceTestsBase {
		public UpperCamelCaseNameRulesTests() : base(new UpperCamelCaseNameRules()) {
		}

		[Test]
		public void InvalidNames_AdditionalSymbols() {
			var invalidNames = new[] { "_ThisIsInvalidName", "This_IsInvalidName", "ThisIsInv_alidName", "ThisIsInvalid_Name", "ThisIsInvalidNam_e", "ThisIsInvalidName_" };
			invalidNames.ForEach(invalidName => CheckInvalidName(invalidName, null, null));
			CheckValidName("ThisIsValidName", null, null);
		}

		[Test]
		public void InvalidNames_DoubleUpperSymbols() {
			var invalidNames = new[] { "THisIsInvalidName", "ThiSIsInvalidName", "ThisISInvalidName", "ThisIsINvalidName", "ThisIsInvaliDName", "ThisIsInvalidNaME" };
			invalidNames.ForEach(invalidName => CheckInvalidName(invalidName, null, null));
			CheckValidName("ThisIsValidName", null, null);
		}

		[Test]
		public void InvalidNames_StartsWithDigit() {
			var invalidNames = new[] { "4ThisIsInvalidName", "3hisIsInvalidName", "12ThisIsInvalidName", "0ThisIsInvalidName", "9ThisIsInvalidName" };
			invalidNames.ForEach(invalidName => CheckInvalidName(invalidName, null, null));
			CheckValidName("ThisIsValidName", null, null);
		}

		[Test]
		public void InvalidNames_IncorrectPrefix() {
			var invalidNames = new[] { "ThisIsInvalidName", "_ThisIsInvalidName", "pre_ThisIsInvalidName", "ThisIsInvalidName_", "ThisIsInvalidName_prefix_" };
			invalidNames.ForEach(invalidName => CheckInvalidName(invalidName, "_prefix_", null));
			CheckValidName("_prefix_ThisIsValidName", "_prefix_", null);
		}

		[Test]
		public void InvalidNames_IncorrectSuffix() {
			var invalidNames = new[] { "ThisIsInvalidName", "ThisIsInvalidName_", "ThisIsInvalidName_suf", "_ThisIsInvalidName", "_suffix_ThisIsInvalidName" };
			invalidNames.ForEach(invalidName => CheckInvalidName(invalidName, null, "_suffix_"));
			CheckValidName("ThisIsValidName_suffix_", null, "_suffix_");
		}

		[Test]
		public void InvalidNames_IncorrectPrefixAndSuffix() {
			var invalidNames = new[] { "ThisIsInvalidName", "_ThisIsInvalidName", "ThisIsInvalidName_", "_ThisIsInvalidName_", "_suffix_ThisIsInvalidName_prefix_" };
			invalidNames.ForEach(invalidName => CheckInvalidName(invalidName, "_prefix_", "_suffix_"));
			CheckValidName("_prefix_ThisIsValidName_suffix_", "_prefix_", "_suffix_");
		}
	}
}