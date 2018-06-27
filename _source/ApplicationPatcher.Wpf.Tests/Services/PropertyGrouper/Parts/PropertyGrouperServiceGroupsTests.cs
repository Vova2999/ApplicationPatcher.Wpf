using ApplicationPatcher.Tests;
using ApplicationPatcher.Tests.FakeTypes;
using ApplicationPatcher.Wpf.Types.Attributes;
using ApplicationPatcher.Wpf.Types.Enums;
using NUnit.Framework;

namespace ApplicationPatcher.Wpf.Tests.Services.PropertyGrouper.Parts {
	[TestFixture]
	public class PropertyGrouperServiceGroupsTests : PropertyGrouperServiceTestsBase {
		[Test]
		public void NotFoundFieldForPropertyWhenUsingNotUseSearchByNameAttribute_InvalidViewModel() {
			const string patchingFieldName = "otherName";
			const string patchingPropertyName = "AnyValue";

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
			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.Selectively);

			CheckInvalidViewModel(secondViewModelType,
				ViewModelPatchingType.All,
				$"Not found field for property '{patchingPropertyName}' when using '{nameof(NotUseSearchByNameAttribute)}'");
			CheckInvalidViewModel(secondViewModelType,
				ViewModelPatchingType.Selectively,
				$"Not found field for property '{patchingPropertyName}' when using '{nameof(NotUseSearchByNameAttribute)}'");
		}

		[Test]
		public void NotFoundFieldForPropertyWhenUsingNotUseSearchByNameAttribute_ValidViewModel() {
			const string patchingFieldName = "otherName";
			const string patchingPropertyName = "AnyValue";

			var firstViewModelType = FakeCommonTypeBuilder.Create("FirstViewModel")
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new NotUseSearchByNameAttribute(), new ConnectPropertyToFieldAttribute(patchingFieldName))
				.AddField(patchingFieldName, typeof(int))
				.Build();

			var secondViewModelType = FakeCommonTypeBuilder.Create("SecondViewModel")
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new NotUseSearchByNameAttribute())
				.AddField(patchingFieldName, typeof(int), new ConnectFieldToPropertyAttribute(patchingPropertyName))
				.Build();

			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.All, patchingFieldName, patchingPropertyName);
			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.Selectively, patchingFieldName, patchingPropertyName);

			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.All, patchingFieldName, patchingPropertyName);
			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.Selectively, patchingFieldName, patchingPropertyName);
		}

		[Test]
		public void MultiConnectPropertyToFieldFound_InvalidViewModel() {
			const string patchingFirstFieldName = "otherName";
			const string patchingSecondFieldName = "anyValue";
			const string patchingPropertyName = "AnyValue";

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
				true);
			CheckInvalidViewModel(firstViewModelType,
				ViewModelPatchingType.Selectively,
				$"Multi-connect property to field found: property '{patchingPropertyName}', fields: '{patchingSecondFieldName}', '{patchingFirstFieldName}'",
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
			CheckValidViewModel(thirdViewModelType, ViewModelPatchingType.Selectively, patchingFirstFieldName, patchingPropertyName);

			CheckInvalidViewModel(fourthViewModelType,
				ViewModelPatchingType.All,
				$"Multi-connect property to field found: property '{patchingPropertyName}', fields: '{patchingSecondFieldName}', '{patchingFirstFieldName}'");
			CheckInvalidViewModel(fourthViewModelType,
				ViewModelPatchingType.Selectively,
				$"Multi-connect property to field found: property '{patchingPropertyName}', fields: '{patchingFirstFieldName}', '{patchingSecondFieldName}'");
		}

		[Test]
		public void MultiConnectPropertyToFieldFound_ValidViewModel() {
			const string patchingFirstFieldName = "otherName";
			const string patchingSecondFieldName = "anyValue";
			const string patchingPropertyName = "AnyValue";

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

			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.All, patchingSecondFieldName, patchingPropertyName);
			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.Selectively);

			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.All, patchingFirstFieldName, patchingPropertyName);
			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.Selectively, patchingFirstFieldName, patchingPropertyName);

			CheckValidViewModel(thirdViewModelType, ViewModelPatchingType.All, patchingFirstFieldName, patchingPropertyName);
			CheckValidViewModel(thirdViewModelType, ViewModelPatchingType.Selectively, patchingFirstFieldName, patchingPropertyName);
		}

		[Test]
		public void TypesDoNotMatchInsideGroup_InvalidViewModel() {
			const string patchingFieldName = "anyValue";
			const string patchingPropertyName = "AnyValue";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet)
				.AddField(patchingFieldName, typeof(bool))
				.Build();

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.All,
				$"Types do not match inside group: property '{patchingPropertyName}', field '{patchingFieldName}'");
			CheckValidViewModel(viewModelType, ViewModelPatchingType.Selectively);
		}

		[Test]
		public void TypesDoNotMatchInsideGroup_ValidViewModel() {
			const string patchingFieldName = "anyValue";
			const string patchingPropertyName = "AnyValue";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet)
				.AddField(patchingFieldName, typeof(int))
				.Build();

			CheckValidViewModel(viewModelType, ViewModelPatchingType.All, patchingFieldName, patchingPropertyName);
			CheckValidViewModel(viewModelType, ViewModelPatchingType.Selectively);
		}
	}
}