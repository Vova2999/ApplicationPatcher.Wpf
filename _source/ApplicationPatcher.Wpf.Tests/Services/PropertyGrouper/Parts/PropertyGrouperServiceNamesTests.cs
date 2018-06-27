using ApplicationPatcher.Tests;
using ApplicationPatcher.Tests.FakeTypes;
using ApplicationPatcher.Wpf.Types.Attributes;
using ApplicationPatcher.Wpf.Types.Enums;
using NUnit.Framework;

namespace ApplicationPatcher.Wpf.Tests.Services.PropertyGrouper.Parts {
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

			CheckInvalidViewModel(firstViewModelType,
				ViewModelPatchingType.All,
				$"Not valid patching property name '{patchingFirstPropertyName}'");
			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.Selectively);

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

			CheckValidViewModel(viewModelType, ViewModelPatchingType.All, null, patchingPropertyName);
			CheckValidViewModel(viewModelType, ViewModelPatchingType.Selectively);
		}
	}
}