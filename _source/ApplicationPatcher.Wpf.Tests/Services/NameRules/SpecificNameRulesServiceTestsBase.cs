using System;
using System.Linq;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Wpf.Services.NameRules;
using ApplicationPatcher.Wpf.Tests.Helpers;
using FluentAssertions;
using NUnit.Framework;

namespace ApplicationPatcher.Wpf.Tests.Services.NameRules {
	[TestFixture]
	public abstract class SpecificNameRulesServiceTestsBase {
		private Random random;
		private readonly SpecificNameRulesService specificNameRulesService;

		protected SpecificNameRulesServiceTestsBase(SpecificNameRulesService specificNameRulesService) {
			this.specificNameRulesService = specificNameRulesService;
		}

		[SetUp]
		public void SetUp() {
			random = new Random(1);
		}

		[Test]
		public void ValidNames_WithoutPrefix_WithoutSuffix() {
			CheckValidNames(null, null);
		}

		[Test]
		public void ValidNames_WithPrefix_WithoutSuffix() {
			var prefixes = new[] { "_", "Prefix", "Prefix_", "_Prefix_", "prefix", "prefix_", "_prefix_" };
			prefixes.ForEach(prefix => CheckValidNames(prefix, null));
		}

		[Test]
		public void ValidNames_WithoutPrefix_WithSuffix() {
			var suffixes = new[] { "_", "Suffix", "_Suffix", "_Suffix_", "suffix", "_suffix", "_suffix_" };
			suffixes.ForEach(suffix => CheckValidNames(null, suffix));
		}

		[Test]
		public void ValidNames_WithPrefix_WithSuffix() {
			var prefixes = new[] { "_", "Prefix", "Prefix_", "_Prefix_" };
			var suffixes = new[] { "_", "Suffix", "_Suffix", "_Suffix_" };
			prefixes.ForEach(prefix => suffixes.ForEach(suffix => CheckValidNames(prefix, suffix)));
		}

		private void CheckValidNames(string prefix, string suffix) {
			foreach (var validName in NamesHelper.GetValidNames(random, new Configurations.NameRules { Prefix = prefix, Suffix = suffix, Type = specificNameRulesService.NameRulesType })) {
				specificNameRulesService.IsNameValid(validName.Name, prefix, suffix).Should().Be(true, $"Name '{validName}' is valid");
				specificNameRulesService.GetNameWords(validName.Name, prefix, suffix).SequenceEqual(validName.Words).Should().Be(true);
			}
		}

		protected void CheckValidName(string validName, string prefix, string suffix) {
			specificNameRulesService.IsNameValid(validName, prefix, suffix).Should().Be(true, $"Name '{validName}' is valid");
		}

		protected void CheckInvalidName(string invalidName, string prefix, string suffix) {
			specificNameRulesService.IsNameValid(invalidName, prefix, suffix).Should().Be(false, $"Name '{invalidName}' is invalid");
		}
	}
}