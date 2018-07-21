using ApplicationPatcher.Tests;
using ApplicationPatcher.Tests.FakeTypes;
using ApplicationPatcher.Wpf.Types.Attributes;
using ApplicationPatcher.Wpf.Types.Attributes.ViewModel;
using ApplicationPatcher.Wpf.Types.Enums;
using NUnit.Framework;

namespace ApplicationPatcher.Wpf.Tests.Services.Groupers.Property {
	[TestFixture]
	public class PropertyGrouperServiceGroupsTests : PropertyGrouperServiceTestsBase {
		[Test]
		public void NotFoundFieldForPropertyWhenUsingNotUseSearchByNameAttribute_InvalidViewModel() {
			const string patchingPropertyName = "AnyValue";
			const string patchingFieldName = "otherName";

			var firstViewModelType = FakeCommonTypeBuilder.Create("FirstViewModel")
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new NotUseSearchByNameAttribute())
				.AddField(patchingFieldName, typeof(int))
				.Build();

			var secondViewModelType = FakeCommonTypeBuilder.Create("SecondViewModel")
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new PatchingPropertyAttribute(), new NotUseSearchByNameAttribute())
				.AddField(patchingFieldName, typeof(int))
				.Build();

			CheckInvalidViewModel(firstViewModelType,
				ViewModelPatchingType.All,
				$"Not found field for property '{patchingPropertyName}' when using '{nameof(NotUseSearchByNameAttribute)}'");

			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.Selectively, false, false);

			CheckInvalidViewModel(secondViewModelType,
				ViewModelPatchingType.All,
				$"Not found field for property '{patchingPropertyName}' when using '{nameof(NotUseSearchByNameAttribute)}'");

			CheckInvalidViewModel(secondViewModelType,
				ViewModelPatchingType.Selectively,
				$"Not found field for property '{patchingPropertyName}' when using '{nameof(NotUseSearchByNameAttribute)}'");
		}

		[Test]
		public void NotFoundFieldForPropertyWhenUsingNotUseSearchByNameAttribute_ValidViewModel() {
			const string patchingPropertyName = "AnyValue";
			const string patchingFieldName = "otherName";

			var firstViewModelType = FakeCommonTypeBuilder.Create("FirstViewModel")
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new NotUseSearchByNameAttribute(), new ConnectPropertyToFieldAttribute(patchingFieldName))
				.AddField(patchingFieldName, typeof(int))
				.Build();

			var secondViewModelType = FakeCommonTypeBuilder.Create("SecondViewModel")
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new NotUseSearchByNameAttribute())
				.AddField(patchingFieldName, typeof(int), new ConnectFieldToPropertyAttribute(patchingPropertyName))
				.Build();

			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.All, false, false, (patchingPropertyName, patchingFieldName));
			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.Selectively, false, false, (patchingPropertyName, patchingFieldName));

			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.All, false, false, (patchingPropertyName, patchingFieldName));
			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.Selectively, false, false, (patchingPropertyName, patchingFieldName));
		}

		[Test]
		public void MultiConnectPropertyToFieldFound_InvalidViewModel() {
			const string patchingPropertyName = "AnyValue";
			const string patchingFirstFieldName = "otherName";
			const string patchingSecondFieldName = "anyValue";

			var firstViewModelType = FakeCommonTypeBuilder.Create("FirstViewModel")
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new ConnectPropertyToFieldAttribute(patchingFirstFieldName))
				.AddField(patchingFirstFieldName, typeof(int))
				.AddField(patchingSecondFieldName, typeof(int))
				.Build();

			var secondViewModelType = FakeCommonTypeBuilder.Create("SecondViewModel")
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new ConnectPropertyToFieldAttribute(patchingFirstFieldName))
				.AddField(patchingFirstFieldName, typeof(int))
				.AddField(patchingSecondFieldName, typeof(int), new ConnectFieldToPropertyAttribute(patchingPropertyName))
				.Build();

			var thirdViewModelType = FakeCommonTypeBuilder.Create("ThirdViewModel")
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet)
				.AddField(patchingFirstFieldName, typeof(int), new ConnectFieldToPropertyAttribute(patchingPropertyName))
				.AddField(patchingSecondFieldName, typeof(int))
				.Build();

			var fourthViewModelType = FakeCommonTypeBuilder.Create("FourthViewModel")
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet)
				.AddField(patchingFirstFieldName, typeof(int), new ConnectFieldToPropertyAttribute(patchingPropertyName))
				.AddField(patchingSecondFieldName, typeof(int), new ConnectFieldToPropertyAttribute(patchingPropertyName))
				.Build();

			CheckInvalidViewModel(firstViewModelType,
				ViewModelPatchingType.All,
				$"Multi-connect property to field found: property '{patchingPropertyName}', fields: '{patchingSecondFieldName}', '{patchingFirstFieldName}'",
				false,
				true);

			CheckInvalidViewModel(firstViewModelType,
				ViewModelPatchingType.Selectively,
				$"Multi-connect property to field found: property '{patchingPropertyName}', fields: '{patchingSecondFieldName}', '{patchingFirstFieldName}'",
				false,
				true);

			CheckInvalidViewModel(secondViewModelType,
				ViewModelPatchingType.All,
				$"Multi-connect property to field found: property '{patchingPropertyName}', fields: '{patchingFirstFieldName}', '{patchingSecondFieldName}'");

			CheckInvalidViewModel(secondViewModelType,
				ViewModelPatchingType.Selectively,
				$"Multi-connect property to field found: property '{patchingPropertyName}', fields: '{patchingFirstFieldName}', '{patchingSecondFieldName}'");

			CheckInvalidViewModel(thirdViewModelType,
				ViewModelPatchingType.All,
				$"Multi-connect property to field found: property '{patchingPropertyName}', fields: '{patchingSecondFieldName}', '{patchingFirstFieldName}'");

			CheckValidViewModel(thirdViewModelType, ViewModelPatchingType.Selectively, false, false, (patchingPropertyName, patchingFirstFieldName));

			CheckInvalidViewModel(fourthViewModelType,
				ViewModelPatchingType.All,
				$"Multi-connect property to field found: property '{patchingPropertyName}', fields: '{patchingSecondFieldName}', '{patchingFirstFieldName}'");

			CheckInvalidViewModel(fourthViewModelType,
				ViewModelPatchingType.Selectively,
				$"Multi-connect property to field found: property '{patchingPropertyName}', fields: '{patchingFirstFieldName}', '{patchingSecondFieldName}'");
		}

		[Test]
		public void MultiConnectPropertyToFieldFound_ValidViewModel() {
			const string patchingPropertyName = "AnyValue";
			const string patchingFirstFieldName = "otherName";
			const string patchingSecondFieldName = "anyValue";

			var firstViewModelType = FakeCommonTypeBuilder.Create("FirstViewModel")
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet)
				.AddField(patchingFirstFieldName, typeof(int))
				.AddField(patchingSecondFieldName, typeof(int))
				.Build();

			var secondViewModelType = FakeCommonTypeBuilder.Create("SecondViewModel")
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new ConnectPropertyToFieldAttribute(patchingFirstFieldName))
				.AddField(patchingFirstFieldName, typeof(int))
				.AddField(patchingSecondFieldName, typeof(int))
				.Build();

			var thirdViewModelType = FakeCommonTypeBuilder.Create("ThirdViewModel")
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new NotUseSearchByNameAttribute())
				.AddField(patchingFirstFieldName, typeof(int), new ConnectFieldToPropertyAttribute(patchingPropertyName))
				.AddField(patchingSecondFieldName, typeof(int))
				.Build();

			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.All, false, false, (patchingPropertyName, patchingSecondFieldName));
			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.Selectively, false, false);

			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.All, false, false, (patchingPropertyName, patchingFirstFieldName));
			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.Selectively, false, false, (patchingPropertyName, patchingFirstFieldName));

			CheckValidViewModel(thirdViewModelType, ViewModelPatchingType.All, false, false, (patchingPropertyName, patchingFirstFieldName));
			CheckValidViewModel(thirdViewModelType, ViewModelPatchingType.Selectively, false, false, (patchingPropertyName, patchingFirstFieldName));
		}

		[Test]
		public void TypesDoNotMatchInsideGroup_InvalidViewModel() {
			const string patchingPropertyName = "AnyValue";
			const string patchingFieldName = "anyValue";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet)
				.AddField(patchingFieldName, typeof(bool))
				.Build();

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.All,
				$"Types do not match inside group: property '{patchingPropertyName}', field '{patchingFieldName}'");

			CheckValidViewModel(viewModelType, ViewModelPatchingType.Selectively, false, false);
		}

		[Test]
		public void TypesDoNotMatchInsideGroup_ValidViewModel() {
			const string patchingPropertyName = "AnyValue";
			const string patchingFieldName = "anyValue";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet)
				.AddField(patchingFieldName, typeof(int))
				.Build();

			CheckValidViewModel(viewModelType, ViewModelPatchingType.All, false, false, (patchingPropertyName, patchingFieldName));
			CheckValidViewModel(viewModelType, ViewModelPatchingType.Selectively, false, false);
		}
	}
}