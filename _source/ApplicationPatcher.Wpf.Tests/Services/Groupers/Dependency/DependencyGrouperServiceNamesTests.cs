using ApplicationPatcher.Tests;
using ApplicationPatcher.Tests.FakeTypes;
using ApplicationPatcher.Wpf.Types.Attributes.SelectPatching;
using ApplicationPatcher.Wpf.Types.Enums;
using NUnit.Framework;

namespace ApplicationPatcher.Wpf.Tests.Services.Groupers.Dependency {
	[TestFixture]
	public class DependencyGrouperServiceNamesTests : DependencyGrouperServiceTestsBase {
		[Test]
		public void NotValidPatchingPropertyName_InvalidViewModel() {
			const string patchingFirstPropertyName = "AAnyValue";
			const string patchingSecondPropertyName = "_AnyValue";

			var firstFrameworkElementType = FakeCommonTypeBuilder.Create("FirstFrameworkElement")
				.AddProperty(patchingFirstPropertyName, typeof(int), PropertyMethods.HasGetAndSet)
				.Build();

			var secondFrameworkElementType = FakeCommonTypeBuilder.Create("SecondFrameworkElement")
				.AddProperty(patchingSecondPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new PatchingPropertyAttribute())
				.Build();

			CheckValidFrameworkElement(firstFrameworkElementType, FrameworkElementPatchingType.All, true, false);
			CheckValidFrameworkElement(firstFrameworkElementType, FrameworkElementPatchingType.Selectively, true, false);

			CheckInvalidFrameworkElement(firstFrameworkElementType,
				FrameworkElementPatchingType.All,
				$"Not valid patching property name '{patchingFirstPropertyName}'");

			CheckValidFrameworkElement(firstFrameworkElementType, FrameworkElementPatchingType.Selectively, false, false);

			CheckValidFrameworkElement(secondFrameworkElementType, FrameworkElementPatchingType.All, true, false);
			CheckValidFrameworkElement(secondFrameworkElementType, FrameworkElementPatchingType.Selectively, true, false);

			CheckInvalidFrameworkElement(secondFrameworkElementType,
				FrameworkElementPatchingType.All,
				$"Not valid patching property name '{patchingSecondPropertyName}'");

			CheckInvalidFrameworkElement(secondFrameworkElementType,
				FrameworkElementPatchingType.Selectively,
				$"Not valid patching property name '{patchingSecondPropertyName}'");
		}

		[Test]
		public void NotValidPatchingPropertyName_ValidViewModel() {
			const string patchingPropertyName = "AnyValue";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet)
				.Build();

			CheckValidFrameworkElement(viewModelType, FrameworkElementPatchingType.All, false, false, (patchingPropertyName, null));
			CheckValidFrameworkElement(viewModelType, FrameworkElementPatchingType.Selectively, false, false);
		}
	}
}