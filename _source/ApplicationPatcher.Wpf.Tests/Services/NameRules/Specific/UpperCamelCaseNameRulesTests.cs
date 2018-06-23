using ApplicationPatcher.Wpf.Services.NameRules.Specific;
using FluentAssertions;
using NUnit.Framework;

namespace ApplicationPatcher.Wpf.Tests.Services.NameRules.Specific {
	[TestFixture]
	public class UpperCamelCaseNameRulesTests {
		[Test]
		public void A() {
			const string name = "This";
			var upperCamelCaseNameRules = new UpperCamelCaseNameRules(null, null);
			var lowerCamelCaseNameRules = new LowerCamelCaseNameRules(null, null);

			var compileName = lowerCamelCaseNameRules.CompileName(upperCamelCaseNameRules.GetNameWords(name));
			compileName.Should().Be("this");
		}
	}
}