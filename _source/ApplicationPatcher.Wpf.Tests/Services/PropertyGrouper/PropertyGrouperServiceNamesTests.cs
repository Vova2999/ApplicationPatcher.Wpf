using ApplicationPatcher.Tests;
using ApplicationPatcher.Tests.FakeTypes;
using ApplicationPatcher.Wpf.Types.Attributes.ViewModel;
using ApplicationPatcher.Wpf.Types.Enums;
using NUnit.Framework;

namespace ApplicationPatcher.Wpf.Tests.Services.PropertyGrouper {
	[TestFixture]
	public class PropertyGrouperServiceNamesTests : PropertyGrouperServiceTestsBase {
		[Test]
		public void NotValidPatchingPropertyName_InvalidViewModel() {
			const string patchingFirstPropertyName = "AAnyValue";
			const string patchingSecondPropertyName = "_AnyValue";

			var firstViewModelType = FakeCommonTypeBuilder.Create("FirstViewModel")
				.AddProperty(patchingFirstPropertyName, typeof(int), PropertyMethods.HasGetAndSet)
				.Build();

			var secondViewModelType = FakeCommonTypeBuilder.Create("SecondViewModel")
				.AddProperty(patchingSecondPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new PatchingPropertyAttribute())
				.Build();

			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.All, true, false);
			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.Selectively, true, false);

			CheckInvalidViewModel(firstViewModelType,
				ViewModelPatchingType.All,
				$"Not valid patching property name '{patchingFirstPropertyName}'");

			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.Selectively, false, false);

			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.All, true, false);
			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.Selectively, true, false);

			CheckInvalidViewModel(secondViewModelType,
				ViewModelPatchingType.All,
				$"Not valid patching property name '{patchingSecondPropertyName}'");

			CheckInvalidViewModel(secondViewModelType,
				ViewModelPatchingType.Selectively,
				$"Not valid patching property name '{patchingSecondPropertyName}'");
		}

		[Test]
		public void NotValidPatchingPropertyName_ValidViewModel() {
			const string patchingPropertyName = "AnyValue";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet)
				.Build();

			CheckValidViewModel(viewModelType, ViewModelPatchingType.All, false, false, (patchingPropertyName, null));
			CheckValidViewModel(viewModelType, ViewModelPatchingType.Selectively, false, false);
		}
	}
}