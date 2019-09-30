using ApplicationPatcher.Tests;
using ApplicationPatcher.Tests.FakeTypes;
using ApplicationPatcher.Wpf.Types.Attributes.Connect;
using ApplicationPatcher.Wpf.Types.Attributes.SelectPatching;
using ApplicationPatcher.Wpf.Types.Enums;
using NUnit.Framework;

namespace ApplicationPatcher.Wpf.Tests.Services.Groupers.Property {
	[TestFixture]
	public class PropertyGrouperServiceAttributesTests : PropertyGrouperServiceTestsBase {
		[Test]
		public void PatchingPropertyCanNotInheritedFromCommandType_InvalidViewModel() {
			const string patchingPropertyName = "AnyValue";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddProperty(patchingPropertyName, CommandType.Type, PropertyMethods.HasGetAndSet, new PatchingPropertyAttribute())
				.Build();

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.All,
				$"Patching property '{patchingPropertyName}' can not inherited from '{KnownTypeNames.ICommand}'");

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.Selectively,
				$"Patching property '{patchingPropertyName}' can not inherited from '{KnownTypeNames.ICommand}'");
		}

		[Test]
		public void PatchingPropertyCanNotInheritedFromCommandType_ValidViewModel() {
			const string patchingPropertyName = "AnyValue";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new PatchingPropertyAttribute())
				.Build();

			CheckValidViewModel(viewModelType, ViewModelPatchingType.All, (patchingPropertyName, null));
			CheckValidViewModel(viewModelType, ViewModelPatchingType.Selectively, (patchingPropertyName, null));
		}

		[Test]
		public void PatchingPropertyCanNotHavePatchingAndNotPatchingPropertyAttribute_InvalidViewModel() {
			const string patchingPropertyName = "AnyValue";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new PatchingPropertyAttribute(), new NotPatchingPropertyAttribute())
				.Build();

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.All,
				$"Patching property '{patchingPropertyName}' can not have '{nameof(PatchingPropertyAttribute)}' and '{nameof(NotPatchingPropertyAttribute)}' at the same time");

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.Selectively,
				$"Patching property '{patchingPropertyName}' can not have '{nameof(PatchingPropertyAttribute)}' and '{nameof(NotPatchingPropertyAttribute)}' at the same time");
		}

		[Test]
		public void PatchingPropertyCanNotHavePatchingAndNotPatchingPropertyAttribute_ValidViewModel() {
			const string patchingPropertyName = "AnyValue";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new NotPatchingPropertyAttribute())
				.Build();

			CheckValidViewModel(viewModelType, ViewModelPatchingType.All);
			CheckValidViewModel(viewModelType, ViewModelPatchingType.Selectively);
		}

		[Test]
		public void NotFoundFieldSpecifiedInConnectPropertyToFieldAttribute_InvalidViewModel() {
			const string patchingPropertyName = "AnyValue";
			const string patchingFieldName = "otherName";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new ConnectPropertyToFieldAttribute(patchingFieldName))
				.Build();

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.All,
				$"Not found field with name '{patchingFieldName}', specified in '{nameof(ConnectPropertyToFieldAttribute)}' at property '{patchingPropertyName}'");

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.Selectively,
				$"Not found field with name '{patchingFieldName}', specified in '{nameof(ConnectPropertyToFieldAttribute)}' at property '{patchingPropertyName}'");
		}

		[Test]
		public void NotFoundFieldSpecifiedInConnectPropertyToFieldAttribute_ValidViewModel() {
			const string patchingPropertyName = "AnyValue";
			const string patchingFieldName = "otherName";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new ConnectPropertyToFieldAttribute(patchingFieldName))
				.AddField(patchingFieldName, typeof(int))
				.Build();

			CheckValidViewModel(viewModelType, ViewModelPatchingType.All, (patchingPropertyName, patchingFieldName));
			CheckValidViewModel(viewModelType, ViewModelPatchingType.Selectively, (patchingPropertyName, patchingFieldName));
		}

		[Test]
		public void NotFoundPropertySpecifiedInConnectFieldToPropertyAttribute_InvalidViewModel() {
			const string patchingPropertyName = "AnyValue";
			const string patchingFieldName = "otherName";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddField(patchingFieldName, typeof(int), new ConnectFieldToPropertyAttribute(patchingPropertyName))
				.Build();

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.All,
				$"Not found property with name '{patchingPropertyName}', specified in '{nameof(ConnectFieldToPropertyAttribute)}' at field '{patchingFieldName}'");

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.Selectively,
				$"Not found property with name '{patchingPropertyName}', specified in '{nameof(ConnectFieldToPropertyAttribute)}' at field '{patchingFieldName}'");
		}

		[Test]
		public void NotFoundPropertySpecifiedInConnectFieldToPropertyAttribute_ValidViewModel() {
			const string patchingPropertyName = "AnyValue";
			const string patchingFieldName = "otherName";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet)
				.AddField(patchingFieldName, typeof(int), new ConnectFieldToPropertyAttribute(patchingPropertyName))
				.Build();

			CheckValidViewModel(viewModelType, ViewModelPatchingType.All, (patchingPropertyName, patchingFieldName));
			CheckValidViewModel(viewModelType, ViewModelPatchingType.Selectively, (patchingPropertyName, patchingFieldName));
		}

		[Test]
		public void TypesDoNotMatchBetweenFieldAndProperty_InvalidViewModel() {
			const string patchingPropertyName = "AnyValue";
			const string patchingFieldName = "otherName";

			var firstViewModelType = FakeCommonTypeBuilder.Create("FirstViewModel")
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet)
				.AddField(patchingFieldName, typeof(bool), new ConnectFieldToPropertyAttribute(patchingPropertyName))
				.Build();

			var secondViewModelType = FakeCommonTypeBuilder.Create("SecondViewModel")
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new ConnectPropertyToFieldAttribute(patchingFieldName))
				.AddField(patchingFieldName, typeof(bool))
				.Build();

			CheckInvalidViewModel(firstViewModelType,
				ViewModelPatchingType.All,
				$"Types do not match between field '{patchingFieldName}' and property '{patchingPropertyName}', connection in '{nameof(ConnectFieldToPropertyAttribute)}' at field '{patchingFieldName}'");

			CheckInvalidViewModel(firstViewModelType,
				ViewModelPatchingType.Selectively,
				$"Types do not match between field '{patchingFieldName}' and property '{patchingPropertyName}', connection in '{nameof(ConnectFieldToPropertyAttribute)}' at field '{patchingFieldName}'");

			CheckInvalidViewModel(secondViewModelType,
				ViewModelPatchingType.All,
				$"Types do not match between property '{patchingPropertyName}' and field '{patchingFieldName}', connection in '{nameof(ConnectPropertyToFieldAttribute)}' at property '{patchingPropertyName}'");

			CheckInvalidViewModel(secondViewModelType,
				ViewModelPatchingType.Selectively,
				$"Types do not match between property '{patchingPropertyName}' and field '{patchingFieldName}', connection in '{nameof(ConnectPropertyToFieldAttribute)}' at property '{patchingPropertyName}'");
		}

		[Test]
		public void TypesDoNotMatchBetweenFieldAndProperty_ValidViewModel() {
			const string patchingPropertyName = "AnyValue";
			const string patchingFieldName = "otherName";

			var firstViewModelType = FakeCommonTypeBuilder.Create("FirstViewModel")
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet)
				.AddField(patchingFieldName, typeof(int), new ConnectFieldToPropertyAttribute(patchingPropertyName))
				.Build();

			var secondViewModelType = FakeCommonTypeBuilder.Create("SecondViewModel")
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new ConnectPropertyToFieldAttribute(patchingFieldName))
				.AddField(patchingFieldName, typeof(int))
				.Build();

			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.All, (patchingPropertyName, patchingFieldName));
			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.Selectively, (patchingPropertyName, patchingFieldName));

			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.All, (patchingPropertyName, patchingFieldName));
			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.Selectively, (patchingPropertyName, patchingFieldName));
		}

		[Test]
		public void PatchingPropertyCanNotHaveNotPatchingPropertyAttribute_InvalidViewModel() {
			const string patchingPropertyName = "AnyValue";
			const string patchingFieldName = "otherName";

			var firstViewModelType = FakeCommonTypeBuilder.Create("FirstViewModel")
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new ConnectPropertyToFieldAttribute(patchingFieldName), new NotPatchingPropertyAttribute())
				.AddField(patchingFieldName, typeof(int))
				.Build();

			var secondViewModelType = FakeCommonTypeBuilder.Create("SecondViewModel")
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new NotPatchingPropertyAttribute())
				.AddField(patchingFieldName, typeof(int), new ConnectFieldToPropertyAttribute(patchingPropertyName))
				.Build();

			CheckInvalidViewModel(firstViewModelType,
				ViewModelPatchingType.All,
				$"Patching property '{patchingPropertyName}' can not have '{nameof(NotPatchingPropertyAttribute)}', connection in '{nameof(ConnectPropertyToFieldAttribute)}' at property '{patchingPropertyName}'");

			CheckInvalidViewModel(firstViewModelType,
				ViewModelPatchingType.Selectively,
				$"Patching property '{patchingPropertyName}' can not have '{nameof(NotPatchingPropertyAttribute)}', connection in '{nameof(ConnectPropertyToFieldAttribute)}' at property '{patchingPropertyName}'");

			CheckInvalidViewModel(secondViewModelType,
				ViewModelPatchingType.All,
				$"Patching property '{patchingPropertyName}' can not have '{nameof(NotPatchingPropertyAttribute)}', connection in '{nameof(ConnectFieldToPropertyAttribute)}' at field '{patchingFieldName}'");

			CheckInvalidViewModel(secondViewModelType,
				ViewModelPatchingType.Selectively,
				$"Patching property '{patchingPropertyName}' can not have '{nameof(NotPatchingPropertyAttribute)}', connection in '{nameof(ConnectFieldToPropertyAttribute)}' at field '{patchingFieldName}'");
		}

		[Test]
		public void PatchingPropertyCanNotHaveNotPatchingPropertyAttribute_ValidViewModel() {
			const string patchingPropertyName = "AnyValue";
			const string patchingFieldName = "otherName";

			var firstViewModelType = FakeCommonTypeBuilder.Create("FirstViewModel")
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new ConnectPropertyToFieldAttribute(patchingFieldName))
				.AddField(patchingFieldName, typeof(int))
				.Build();

			var secondViewModelType = FakeCommonTypeBuilder.Create("SecondViewModel")
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet)
				.AddField(patchingFieldName, typeof(int), new ConnectFieldToPropertyAttribute(patchingPropertyName))
				.Build();

			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.All, (patchingPropertyName, patchingFieldName));
			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.Selectively, (patchingPropertyName, patchingFieldName));

			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.All, (patchingPropertyName, patchingFieldName));
			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.Selectively, (patchingPropertyName, patchingFieldName));
		}
	}
}