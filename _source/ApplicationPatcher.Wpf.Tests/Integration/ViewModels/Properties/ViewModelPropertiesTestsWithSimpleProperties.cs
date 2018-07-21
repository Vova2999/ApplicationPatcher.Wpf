using FluentAssertions;
using GalaSoft.MvvmLight;
using NUnit.Framework;

namespace ApplicationPatcher.Wpf.Tests.Integration.ViewModels.Properties {
	public class ViewModelWithSimpleProperties : ViewModelBase {
		public int FirstProperty { get; set; }
		public string SecondProperty { get; set; }
	}

	[TestFixture]
	public class ViewModelPropertiesTestsWithSimpleProperties : ViewModelPropertiesTestsBase {
		[Test]
		public void ChangeAllPropertiesTest() {
			var viewModel = CreateViewModel<ViewModelWithSimpleProperties>();

			viewModel.FirstProperty = 123;
			viewModel.SecondProperty = "123";

			CheckChangedProperties(nameof(viewModel.FirstProperty), nameof(viewModel.SecondProperty));
			viewModel.FirstProperty.Should().Be(123);
			viewModel.SecondProperty.Should().Be("123");
		}

		[Test]
		public void ChangeAllPropertiesWithRepeatTest() {
			var viewModel = CreateViewModel<ViewModelWithSimpleProperties>();

			viewModel.FirstProperty = 123;
			viewModel.FirstProperty = 123;
			viewModel.SecondProperty = "123";
			viewModel.SecondProperty = "123";

			CheckChangedProperties(nameof(viewModel.FirstProperty), nameof(viewModel.SecondProperty));
			viewModel.FirstProperty.Should().Be(123);
			viewModel.SecondProperty.Should().Be("123");
		}
	}
}