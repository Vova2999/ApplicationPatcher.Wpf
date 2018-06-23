using System;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Wpf.Configurations;
using ApplicationPatcher.Wpf.Services.NameRules;
using ApplicationPatcher.Wpf.Tests.Helpers;
using FluentAssertions;
using NUnit.Framework;

namespace ApplicationPatcher.Wpf.Tests.Services.NameRules {
	[TestFixture]
	public abstract class SpecificNameRulesServiceTestsBase {
		protected Random Random;
		protected abstract NameRulesType NameRulesType { get; }

		protected abstract SpecificNameRulesService CreateSpecificNameRulesService(string prefix, string suffix);

		[SetUp]
		public void SpecificNameRulesServiceTestsBaseSetUp() {
			Random = new Random(1);
		}

		[Test]
		public void ValidNames_WithPrefix_WithSuffix() {
			var prefixes = new[] { "_", "Prefix", "Prefix_", "_Prefix_" };
			var suffixes = new[] { "_", "Suffix", "_Suffix", "_Suffix_" };
			prefixes.ForEach(prefix => suffixes.ForEach(suffix => CheckValidNames(prefix, suffix)));
		}

		[Test]
		public void ValidNames_WithPrefix_WithoutSuffix() {
			var prefixes = new[] { "_", "Prefix", "Prefix_", "_Prefix_", "preifx", "prefix_", "_prefix_" };
			prefixes.ForEach(prefix => CheckValidNames(prefix, null));
		}

		[Test]
		public void ValidNames_WithoutPrefix_WithSuffix() {
			var suffixes = new[] { "_", "Suffix", "_Suffix", "_Suffix_", "suffix", "_suffix", "_suffix_" };
			suffixes.ForEach(suffix => CheckValidNames(null, suffix));
		}

		[Test]
		public void ValidNames_WithoutPrefix_WithoutSuffix() {
			CheckValidNames(null, null);
		}

		private void CheckValidNames(string prefix, string suffix) {
			var upperCamelCaseNameRules = CreateSpecificNameRulesService(prefix, suffix);
			foreach (var validName in NamesHelper.GetValidNames(Random, new Configurations.NameRules { Prefix = prefix, Suffix = suffix, Type = NameRulesType }))
				upperCamelCaseNameRules.IsNameValid(validName).Should().Be(true, $"Name '{validName}' is valid");
		}

		protected void CheckValidName(string validName, string prefix, string suffix) {
			var upperCamelCaseNameRules = CreateSpecificNameRulesService(prefix, suffix);
			upperCamelCaseNameRules.IsNameValid(validName).Should().Be(true, $"Name '{validName}' is valid");
		}

		protected void CheckInvalidName(string invalidName, string prefix, string suffix) {
			var upperCamelCaseNameRules = CreateSpecificNameRulesService(prefix, suffix);
			upperCamelCaseNameRules.IsNameValid(invalidName).Should().Be(false, $"Name '{invalidName}' is invalid");
		}
	}
}