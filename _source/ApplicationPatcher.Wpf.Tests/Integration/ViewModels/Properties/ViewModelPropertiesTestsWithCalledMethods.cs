using ApplicationPatcher.Wpf.Types.Attributes.SelectPatching;
using ApplicationPatcher.Wpf.Types.Attributes.ViewModel;
using ApplicationPatcher.Wpf.Types.Enums;
using FluentAssertions;
using GalaSoft.MvvmLight;
using NUnit.Framework;

namespace ApplicationPatcher.Wpf.Tests.Integration.ViewModels.Properties {
	[PatchingViewModel(ViewModelPatchingType.Selectively)]
	public class ViewModelWithCalledMethods : ViewModelBase {
		[PatchingProperty]
		public int Property { get; set; }

		[PatchingProperty(CalledMethodNameBeforeGetProperty = nameof(CalledMethodBeforeGetProperty))]
		public int Property0 { get; set; }

		[PatchingProperty(CalledMethodNameBeforeSetProperty = nameof(CalledMethodBeforeSetProperty))]
		public int Property1 { get; set; }

		[PatchingProperty(CalledMethodNameAfterSuccessSetProperty = nameof(CalledMethodAfterSuccessSetProperty))]
		public int Property2 { get; set; }

		[PatchingProperty(CalledMethodNameAfterSetProperty = nameof(CalledMethodAfterSetProperty))]
		public int Property3 { get; set; }

		[PatchingProperty(
			CalledMethodNameBeforeSetProperty = nameof(CalledMethodBeforeSetProperty),
			CalledMethodNameAfterSuccessSetProperty = nameof(CalledMethodAfterSuccessSetProperty))]
		public int Property12 { get; set; }

		[PatchingProperty(
			CalledMethodNameAfterSuccessSetProperty = nameof(CalledMethodAfterSuccessSetProperty),
			CalledMethodNameAfterSetProperty = nameof(CalledMethodAfterSetProperty))]
		public int Property23 { get; set; }

		[PatchingProperty(
			CalledMethodNameBeforeSetProperty = nameof(CalledMethodBeforeSetProperty),
			CalledMethodNameAfterSetProperty = nameof(CalledMethodAfterSetProperty))]
		public int Property13 { get; set; }

		[PatchingProperty(
			CalledMethodNameBeforeSetProperty = nameof(CalledMethodBeforeSetProperty),
			CalledMethodNameAfterSuccessSetProperty = nameof(CalledMethodAfterSuccessSetProperty),
			CalledMethodNameAfterSetProperty = nameof(CalledMethodAfterSetProperty))]
		public int Property123 { get; set; }

		public ViewModelWithCalledMethods() {
			CalledMethodBeforeGetPropertyExecutionCount = 0;
			CalledMethodBeforeSetPropertyExecutionCount = 0;
			CalledMethodAfterSuccessSetPropertyExecutionCount = 0;
			CalledMethodAfterSetPropertyExecutionCount = 0;
		}

		public int CalledMethodBeforeGetPropertyExecutionCount { get; private set; }
		public void CalledMethodBeforeGetProperty() {
			CalledMethodBeforeGetPropertyExecutionCount++;
		}

		public int CalledMethodBeforeSetPropertyExecutionCount { get; private set; }
		protected virtual int CalledMethodBeforeSetProperty() {
			CalledMethodBeforeSetPropertyExecutionCount++;
			return 0;
		}

		public static int CalledMethodAfterSuccessSetPropertyExecutionCount { get; private set; }
		public static void CalledMethodAfterSuccessSetProperty() {
			CalledMethodAfterSuccessSetPropertyExecutionCount++;
		}

		public static int CalledMethodAfterSetPropertyExecutionCount { get; private set; }
		private static string CalledMethodAfterSetProperty() {
			CalledMethodAfterSetPropertyExecutionCount++;
			return string.Empty;
		}
	}

	[TestFixture]
	public class ViewModelPropertiesTestsWithCalledMethods : ViewModelPropertiesTestsBase {
		[Test]
		public void PropertyTest() {
			var viewModel = CreateViewModel<ViewModelWithCalledMethods>();

			viewModel.Property = 1;
			viewModel.Property = 2;
			viewModel.Property = 2;
			viewModel.Property = 3;

			CheckChangedProperties(nameof(viewModel.Property), nameof(viewModel.Property), nameof(viewModel.Property));
			viewModel.CalledMethodBeforeGetPropertyExecutionCount.Should().Be(0);
			viewModel.CalledMethodBeforeSetPropertyExecutionCount.Should().Be(0);
			ViewModelWithCalledMethods.CalledMethodAfterSuccessSetPropertyExecutionCount.Should().Be(0);
			ViewModelWithCalledMethods.CalledMethodAfterSetPropertyExecutionCount.Should().Be(0);
		}

		[Test]
		public void Property0Test() {
			var viewModel = CreateViewModel<ViewModelWithCalledMethods>();

			viewModel.Property0 = 1;
			viewModel.Property0 = 2;
			viewModel.Property0 = 2;
			viewModel.Property0 = 3;

			CheckChangedProperties(nameof(viewModel.Property0), nameof(viewModel.Property0), nameof(viewModel.Property0));
			viewModel.CalledMethodBeforeGetPropertyExecutionCount.Should().Be(0);
			viewModel.CalledMethodBeforeSetPropertyExecutionCount.Should().Be(0);
			ViewModelWithCalledMethods.CalledMethodAfterSuccessSetPropertyExecutionCount.Should().Be(0);
			ViewModelWithCalledMethods.CalledMethodAfterSetPropertyExecutionCount.Should().Be(0);

			var viewModelProperty0 = viewModel.Property0;
			viewModelProperty0 = viewModel.Property0;
			viewModelProperty0 = viewModel.Property0;

			CheckChangedProperties(nameof(viewModel.Property0), nameof(viewModel.Property0), nameof(viewModel.Property0));
			viewModel.CalledMethodBeforeGetPropertyExecutionCount.Should().Be(3);
			viewModel.CalledMethodBeforeSetPropertyExecutionCount.Should().Be(0);
			ViewModelWithCalledMethods.CalledMethodAfterSuccessSetPropertyExecutionCount.Should().Be(0);
			ViewModelWithCalledMethods.CalledMethodAfterSetPropertyExecutionCount.Should().Be(0);
		}

		[Test]
		public void Property1Test() {
			var viewModel = CreateViewModel<ViewModelWithCalledMethods>();

			viewModel.Property1 = 1;
			viewModel.Property1 = 2;
			viewModel.Property1 = 2;
			viewModel.Property1 = 3;

			CheckChangedProperties(nameof(viewModel.Property1), nameof(viewModel.Property1), nameof(viewModel.Property1));
			viewModel.CalledMethodBeforeGetPropertyExecutionCount.Should().Be(0);
			viewModel.CalledMethodBeforeSetPropertyExecutionCount.Should().Be(4);
			ViewModelWithCalledMethods.CalledMethodAfterSuccessSetPropertyExecutionCount.Should().Be(0);
			ViewModelWithCalledMethods.CalledMethodAfterSetPropertyExecutionCount.Should().Be(0);
		}

		[Test]
		public void Property2Test() {
			var viewModel = CreateViewModel<ViewModelWithCalledMethods>();

			viewModel.Property2 = 1;
			viewModel.Property2 = 2;
			viewModel.Property2 = 2;
			viewModel.Property2 = 3;

			CheckChangedProperties(nameof(viewModel.Property2), nameof(viewModel.Property2), nameof(viewModel.Property2));
			viewModel.CalledMethodBeforeGetPropertyExecutionCount.Should().Be(0);
			viewModel.CalledMethodBeforeSetPropertyExecutionCount.Should().Be(0);
			ViewModelWithCalledMethods.CalledMethodAfterSuccessSetPropertyExecutionCount.Should().Be(3);
			ViewModelWithCalledMethods.CalledMethodAfterSetPropertyExecutionCount.Should().Be(0);
		}

		[Test]
		public void Property3Test() {
			var viewModel = CreateViewModel<ViewModelWithCalledMethods>();

			viewModel.Property3 = 1;
			viewModel.Property3 = 2;
			viewModel.Property3 = 2;
			viewModel.Property3 = 3;

			CheckChangedProperties(nameof(viewModel.Property3), nameof(viewModel.Property3), nameof(viewModel.Property3));
			viewModel.CalledMethodBeforeGetPropertyExecutionCount.Should().Be(0);
			viewModel.CalledMethodBeforeSetPropertyExecutionCount.Should().Be(0);
			ViewModelWithCalledMethods.CalledMethodAfterSuccessSetPropertyExecutionCount.Should().Be(0);
			ViewModelWithCalledMethods.CalledMethodAfterSetPropertyExecutionCount.Should().Be(4);
		}

		[Test]
		public void Property12Test() {
			var viewModel = CreateViewModel<ViewModelWithCalledMethods>();

			viewModel.Property12 = 1;
			viewModel.Property12 = 2;
			viewModel.Property12 = 2;
			viewModel.Property12 = 3;

			CheckChangedProperties(nameof(viewModel.Property12), nameof(viewModel.Property12), nameof(viewModel.Property12));
			viewModel.CalledMethodBeforeGetPropertyExecutionCount.Should().Be(0);
			viewModel.CalledMethodBeforeSetPropertyExecutionCount.Should().Be(4);
			ViewModelWithCalledMethods.CalledMethodAfterSuccessSetPropertyExecutionCount.Should().Be(3);
			ViewModelWithCalledMethods.CalledMethodAfterSetPropertyExecutionCount.Should().Be(0);
		}

		[Test]
		public void Property23Test() {
			var viewModel = CreateViewModel<ViewModelWithCalledMethods>();

			viewModel.Property23 = 1;
			viewModel.Property23 = 2;
			viewModel.Property23 = 2;
			viewModel.Property23 = 3;

			CheckChangedProperties(nameof(viewModel.Property23), nameof(viewModel.Property23), nameof(viewModel.Property23));
			viewModel.CalledMethodBeforeGetPropertyExecutionCount.Should().Be(0);
			viewModel.CalledMethodBeforeSetPropertyExecutionCount.Should().Be(0);
			ViewModelWithCalledMethods.CalledMethodAfterSuccessSetPropertyExecutionCount.Should().Be(3);
			ViewModelWithCalledMethods.CalledMethodAfterSetPropertyExecutionCount.Should().Be(4);
		}

		[Test]
		public void Property13Test() {
			var viewModel = CreateViewModel<ViewModelWithCalledMethods>();

			viewModel.Property13 = 1;
			viewModel.Property13 = 2;
			viewModel.Property13 = 2;
			viewModel.Property13 = 3;

			CheckChangedProperties(nameof(viewModel.Property13), nameof(viewModel.Property13), nameof(viewModel.Property13));
			viewModel.CalledMethodBeforeGetPropertyExecutionCount.Should().Be(0);
			viewModel.CalledMethodBeforeSetPropertyExecutionCount.Should().Be(4);
			ViewModelWithCalledMethods.CalledMethodAfterSuccessSetPropertyExecutionCount.Should().Be(0);
			ViewModelWithCalledMethods.CalledMethodAfterSetPropertyExecutionCount.Should().Be(4);
		}

		[Test]
		public void Property123Test() {
			var viewModel = CreateViewModel<ViewModelWithCalledMethods>();

			viewModel.Property123 = 1;
			viewModel.Property123 = 2;
			viewModel.Property123 = 2;
			viewModel.Property123 = 3;

			CheckChangedProperties(nameof(viewModel.Property123), nameof(viewModel.Property123), nameof(viewModel.Property123));
			viewModel.CalledMethodBeforeGetPropertyExecutionCount.Should().Be(0);
			viewModel.CalledMethodBeforeSetPropertyExecutionCount.Should().Be(4);
			ViewModelWithCalledMethods.CalledMethodAfterSuccessSetPropertyExecutionCount.Should().Be(3);
			ViewModelWithCalledMethods.CalledMethodAfterSetPropertyExecutionCount.Should().Be(4);
		}
	}
}