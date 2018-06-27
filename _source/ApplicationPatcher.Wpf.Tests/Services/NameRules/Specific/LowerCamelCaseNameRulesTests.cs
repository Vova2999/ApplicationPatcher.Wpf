using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Wpf.Services.NameRules.Specific;
using NUnit.Framework;

namespace ApplicationPatcher.Wpf.Tests.Services.NameRules.Specific {
	[TestFixture]
	public class LowerCamelCaseNameRulesTests : SpecificNameRulesServiceTestsBase {
		public LowerCamelCaseNameRulesTests() : base(new LowerCamelCaseNameRules()) {
		}

		[Test]
		public void InvalidNames_AdditionalSymbols() {
			var invalidNames = new[] { "_thisIsInvalidName", "this_IsInvalidName", "thisIsInv_alidName", "thisIsInvalid_Name", "thisIsInvalidNam_e", "thisIsInvalidName_" };
			invalidNames.ForEach(invalidName => CheckInvalidName(invalidName, null, null));
			CheckValidName("thisIsValidName", null, null);
		}

		[Test]
		public void InvalidNames_DoubleUpperSymbols() {
			var invalidNames = new[] { "ThisIsInvalidName", "thiSIsInvalidName", "thisISInvalidName", "thisIsINvalidName", "thisIsInvaliDName", "thisIsInvalidNaME" };
			invalidNames.ForEach(invalidName => CheckInvalidName(invalidName, null, null));
			CheckValidName("thisIsValidName", null, null);
		}

		[Test]
		public void InvalidNames_StartsWithDigit() {
			var invalidNames = new[] { "4thisIsInvalidName", "3hisIsInvalidName", "12thisIsInvalidName", "0thisIsInvalidName", "9thisIsInvalidName" };
			invalidNames.ForEach(invalidName => CheckInvalidName(invalidName, null, null));
			CheckValidName("thisIsValidName", null, null);
		}

		[Test]
		public void InvalidNames_IncorrectPrefix() {
			var invalidNames = new[] { "thisIsInvalidName", "_thisIsInvalidName", "pre_thisIsInvalidName", "thisIsInvalidName_", "thisIsInvalidName_prefix_" };
			invalidNames.ForEach(invalidName => CheckInvalidName(invalidName, "_prefix_", null));
			CheckValidName("_prefix_thisIsValidName", "_prefix_", null);
		}

		[Test]
		public void InvalidNames_IncorrectSuffix() {
			var invalidNames = new[] { "thisIsInvalidName", "thisIsInvalidName_", "thisIsInvalidName_suf", "_thisIsInvalidName", "_suffix_thisIsInvalidName" };
			invalidNames.ForEach(invalidName => CheckInvalidName(invalidName, null, "_suffix_"));
			CheckValidName("thisIsValidName_suffix_", null, "_suffix_");
		}

		[Test]
		public void InvalidNames_IncorrectPrefixAndSuffix() {
			var invalidNames = new[] { "thisIsInvalidName", "_thisIsInvalidName", "thisIsInvalidName_", "_thisIsInvalidName_", "_suffix_thisIsInvalidName_prefix_" };
			invalidNames.ForEach(invalidName => CheckInvalidName(invalidName, "_prefix_", "_suffix_"));
			CheckValidName("_prefix_thisIsValidName_suffix_", "_prefix_", "_suffix_");
		}
	}
}