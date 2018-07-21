using ApplicationPatcher.Wpf.Types.Attributes.ViewModel;
using ApplicationPatcher.Wpf.Types.Enums;
using FluentAssertions;
using GalaSoft.MvvmLight;
using NUnit.Framework;

namespace ApplicationPatcher.Wpf.Tests.Integration.ViewModels.Properties {
	[PatchingViewModel(ViewModelPatchingType.Selectively)]
	public class ViewModelWithPatchingType : ViewModelBase {
		[PatchingProperty]
		public int FirstProperty { get; set; }

		public string SecondProperty { get; set; }
	}

	[TestFixture]
	public class ViewModelPropertiesTestsWithPatchingType : ViewModelPropertiesTestsBase {
		[Test]
		public void ChangeAllPropertiesTest() {
			var viewModel = CreateViewModel<ViewModelWithPatchingType>();

			viewModel.FirstProperty = 123;
			viewModel.SecondProperty = "123";

			CheckChangedProperties(nameof(viewModel.FirstProperty));
			viewModel.FirstProperty.Should().Be(123);
			viewModel.SecondProperty.Should().Be("123");
		}

		[Test]
		public void ChangeAllPropertiesWithRepeatTest() {
			var viewModel = CreateViewModel<ViewModelWithPatchingType>();

			viewModel.FirstProperty = 123;
			viewModel.FirstProperty = 123;
			viewModel.SecondProperty = "123";
			viewModel.SecondProperty = "123";

			CheckChangedProperties(nameof(viewModel.FirstProperty));
			viewModel.FirstProperty.Should().Be(123);
			viewModel.SecondProperty.Should().Be("123");
		}
	}
}