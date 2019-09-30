using ApplicationPatcher.Tests;
using ApplicationPatcher.Tests.FakeTypes;
using ApplicationPatcher.Wpf.Types.Attributes.Connect;
using ApplicationPatcher.Wpf.Types.Enums;
using NUnit.Framework;

namespace ApplicationPatcher.Wpf.Tests.Services.Groupers.Command {
	[TestFixture]
	public class CommandGrouperServiceGroupsTests : CommandGrouperServiceTestsBase {
		[Test]
		public void MultiConnectPropertyToFieldFound_InvalidViewModel() {
			const string fieldName = "anyActionCommand";
			const string secondFieldName = "anySecondActionCommand";
			const string propertyName = "AnyActionCommand";
			const string executeMethodName = "ExecuteAnyActionMethod";

			var firstViewModelType = FakeCommonTypeBuilder.Create("FirstViewModel")
				.AddField(fieldName, CommandType.Type)
				.AddField(secondFieldName, CommandType.Type)
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet, new ConnectPropertyToFieldAttribute(secondFieldName))
				.AddMethod(executeMethodName, typeof(void), null)
				.Build();

			var secondViewModelType = FakeCommonTypeBuilder.Create("SecondViewModel")
				.AddField(fieldName, CommandType.Type)
				.AddField(secondFieldName, CommandType.Type, new ConnectFieldToPropertyAttribute(propertyName))
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet)
				.AddMethod(executeMethodName, typeof(void), null)
				.Build();

			CheckInvalidViewModel(firstViewModelType,
				ViewModelPatchingType.All,
				$"Multi-connect property to field found: property '{propertyName}', fields: '{fieldName}', '{secondFieldName}'",
				false,
				true);

			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.All, (executeMethodName, null, propertyName, secondFieldName));
			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.Selectively, false, true);
			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.Selectively);

			CheckInvalidViewModel(secondViewModelType,
				ViewModelPatchingType.All,
				$"Multi-connect property to field found: property '{propertyName}', fields: '{fieldName}', '{secondFieldName}'");

			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.Selectively);
		}

		[Test]
		public void MultiConnectPropertyToFieldFound_ValidViewModel() {
			const string fieldName = "anyActionCommand";
			const string propertyName = "AnyActionCommand";
			const string executeMethodName = "ExecuteAnyActionMethod";

			var firstViewModelType = FakeCommonTypeBuilder.Create("FirstViewModel")
				.AddField(fieldName, CommandType.Type)
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet, new ConnectPropertyToFieldAttribute(fieldName))
				.AddMethod(executeMethodName, typeof(void), null)
				.Build();

			var secondViewModelType = FakeCommonTypeBuilder.Create("SecondViewModel")
				.AddField(fieldName, CommandType.Type, new ConnectFieldToPropertyAttribute(propertyName))
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet)
				.AddMethod(executeMethodName, typeof(void), null)
				.Build();

			var thirdViewModelType = FakeCommonTypeBuilder.Create("ThirdViewModel")
				.AddField(fieldName, CommandType.Type)
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet)
				.AddMethod(executeMethodName, typeof(void), null)
				.Build();

			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.All, (executeMethodName, null, propertyName, fieldName));
			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.Selectively);

			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.All, (executeMethodName, null, propertyName, fieldName));
			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.Selectively);

			CheckValidViewModel(thirdViewModelType, ViewModelPatchingType.All, (executeMethodName, null, propertyName, fieldName));
			CheckValidViewModel(thirdViewModelType, ViewModelPatchingType.Selectively);
		}

		[Test]
		public void MultiConnectExecuteMethodToCanExecuteMethodFound_InvalidViewModel() {
			const string executeMethodName = "ExecuteAnyActionMethod";
			const string canExecuteMethodName = "CanExecuteAnyActionMethod";
			const string secondCanExecuteMethodName = "CanExecuteAnySecondActionMethod";

			var firstViewModelType = FakeCommonTypeBuilder.Create("FirstViewModel")
				.AddMethod(executeMethodName, typeof(void), null, new ConnectMethodToMethodAttribute(secondCanExecuteMethodName))
				.AddMethod(canExecuteMethodName, typeof(bool), null)
				.AddMethod(secondCanExecuteMethodName, typeof(bool), null)
				.Build();

			var secondViewModelType = FakeCommonTypeBuilder.Create("SecondViewModel")
				.AddMethod(executeMethodName, typeof(void), null)
				.AddMethod(canExecuteMethodName, typeof(bool), null)
				.AddMethod(secondCanExecuteMethodName, typeof(bool), null, new ConnectMethodToMethodAttribute(executeMethodName))
				.Build();

			var thirdViewModelType = FakeCommonTypeBuilder.Create("ThirdViewModel")
				.AddMethod(executeMethodName, typeof(void), null, new ConnectMethodToMethodAttribute(canExecuteMethodName), new ConnectMethodToMethodAttribute(secondCanExecuteMethodName))
				.AddMethod(canExecuteMethodName, typeof(bool), null)
				.AddMethod(secondCanExecuteMethodName, typeof(bool), null)
				.Build();

			var fourthViewModelType = FakeCommonTypeBuilder.Create("FourthViewModel")
				.AddMethod(executeMethodName, typeof(void), null)
				.AddMethod(canExecuteMethodName, typeof(bool), null, new ConnectMethodToMethodAttribute(executeMethodName))
				.AddMethod(secondCanExecuteMethodName, typeof(bool), null, new ConnectMethodToMethodAttribute(executeMethodName))
				.Build();

			CheckInvalidViewModel(firstViewModelType,
				ViewModelPatchingType.All,
				$"Multi-connect execute method to can execute method found: execute method '{executeMethodName}', can execute methods: '{canExecuteMethodName}', '{secondCanExecuteMethodName}'",
				false,
				true);

			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.Selectively, (executeMethodName, secondCanExecuteMethodName, null, null));

			CheckInvalidViewModel(secondViewModelType,
				ViewModelPatchingType.All,
				$"Multi-connect execute method to can execute method found: execute method '{executeMethodName}', can execute methods: '{canExecuteMethodName}', '{secondCanExecuteMethodName}'",
				false,
				true);

			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.Selectively, (executeMethodName, secondCanExecuteMethodName, null, null));

			CheckInvalidViewModel(thirdViewModelType,
				ViewModelPatchingType.All,
				$"Multi-connect execute method to can execute method found: execute method '{executeMethodName}', can execute methods: '{canExecuteMethodName}', '{secondCanExecuteMethodName}'",
				false,
				true);

			CheckInvalidViewModel(thirdViewModelType,
				ViewModelPatchingType.Selectively,
				$"Multi-connect execute method to can execute method found: execute method '{executeMethodName}', can execute methods: '{canExecuteMethodName}', '{secondCanExecuteMethodName}'",
				false,
				true);

			CheckInvalidViewModel(fourthViewModelType,
				ViewModelPatchingType.All,
				$"Multi-connect execute method to can execute method found: execute method '{executeMethodName}', can execute methods: '{canExecuteMethodName}', '{secondCanExecuteMethodName}'",
				false,
				true);

			CheckInvalidViewModel(fourthViewModelType,
				ViewModelPatchingType.Selectively,
				$"Multi-connect execute method to can execute method found: execute method '{executeMethodName}', can execute methods: '{canExecuteMethodName}', '{secondCanExecuteMethodName}'",
				false,
				true);
		}

		[Test]
		public void MultiConnectExecuteMethodToCanExecuteMethodFound_ValidViewModel() {
			const string executeMethodName = "ExecuteAnyActionMethod";
			const string secondExecuteMethodName = "ExecuteAnySecondActionMethod";
			const string canExecuteMethodName = "CanExecuteAnyActionMethod";
			const string secondCanExecuteMethodName = "CanExecuteAnySecondActionMethod";

			var firstViewModelType = FakeCommonTypeBuilder.Create("FirstViewModel")
				.AddMethod(executeMethodName, typeof(void), null, new ConnectMethodToMethodAttribute(canExecuteMethodName))
				.AddMethod(canExecuteMethodName, typeof(bool), null)
				.AddMethod(secondCanExecuteMethodName, typeof(bool), null)
				.Build();

			var secondViewModelType = FakeCommonTypeBuilder.Create("SecondViewModel")
				.AddMethod(executeMethodName, typeof(void), null)
				.AddMethod(canExecuteMethodName, typeof(bool), null, new ConnectMethodToMethodAttribute(executeMethodName))
				.AddMethod(secondCanExecuteMethodName, typeof(bool), null)
				.Build();

			var thirdViewModelType = FakeCommonTypeBuilder.Create("ThirdViewModel")
				.AddMethod(executeMethodName, typeof(void), null)
				.AddMethod(secondExecuteMethodName, typeof(void), null)
				.AddMethod(canExecuteMethodName, typeof(bool), null, new ConnectMethodToMethodAttribute(executeMethodName), new ConnectMethodToMethodAttribute(secondExecuteMethodName))
				.Build();

			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.All, false, true, (executeMethodName, canExecuteMethodName, null, null));
			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.All, (executeMethodName, canExecuteMethodName, null, null));
			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.Selectively, false, true, (executeMethodName, canExecuteMethodName, null, null));
			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.Selectively, (executeMethodName, canExecuteMethodName, null, null));

			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.All, false, true, (executeMethodName, canExecuteMethodName, null, null));
			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.All, (executeMethodName, canExecuteMethodName, null, null));
			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.Selectively, false, true, (executeMethodName, canExecuteMethodName, null, null));
			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.Selectively, (executeMethodName, canExecuteMethodName, null, null));

			CheckValidViewModel(thirdViewModelType, ViewModelPatchingType.All, false, true, (executeMethodName, canExecuteMethodName, null, null), (secondExecuteMethodName, canExecuteMethodName, null, null));
			CheckValidViewModel(thirdViewModelType, ViewModelPatchingType.All, (executeMethodName, canExecuteMethodName, null, null), (secondExecuteMethodName, canExecuteMethodName, null, null));
			CheckValidViewModel(thirdViewModelType, ViewModelPatchingType.Selectively, false, true, (executeMethodName, canExecuteMethodName, null, null), (secondExecuteMethodName, canExecuteMethodName, null, null));
			CheckValidViewModel(thirdViewModelType, ViewModelPatchingType.Selectively, (executeMethodName, canExecuteMethodName, null, null), (secondExecuteMethodName, canExecuteMethodName, null, null));
		}

		[Test]
		public void TypesDoNotMatchInsideGroup_InvalidViewModel() {
			const string fieldName = "anyActionCommand";
			const string propertyName = "AnyActionCommand";
			const string executeMethodName = "ExecuteAnyActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddField(fieldName, RelayCommand.Type)
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet)
				.AddMethod(executeMethodName, typeof(void), null)
				.Build();

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.All,
				$"Types do not match inside group: property '{propertyName}', field '{fieldName}'");

			CheckValidViewModel(viewModelType, ViewModelPatchingType.Selectively);
		}

		[Test]
		public void TypesDoNotMatchInsideGroup_ValidViewModel() {
			const string fieldName = "anyActionCommand";
			const string propertyName = "AnyActionCommand";
			const string executeMethodName = "ExecuteAnyActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddField(fieldName, CommandType.Type)
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet)
				.AddMethod(executeMethodName, typeof(void), null)
				.Build();

			CheckValidViewModel(viewModelType, ViewModelPatchingType.All, (executeMethodName, null, propertyName, fieldName));
			CheckValidViewModel(viewModelType, ViewModelPatchingType.Selectively);
		}

		[Test]
		public void NotFoundPropertyForExecuteMethodWhenUsingNotUseSearchByNameAttribute_InvalidViewModel() {
			const string fieldName = "anyActionCommand";
			const string propertyName = "AnyActionCommand";
			const string executeMethodName = "ExecuteAnyActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddField(fieldName, CommandType.Type)
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet)
				.AddMethod(executeMethodName, typeof(void), null, new NotUseSearchByNameAttribute())
				.Build();

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.All,
				$"Not found property for execute method '{executeMethodName}' when using '{nameof(NotUseSearchByNameAttribute)}'");

			CheckValidViewModel(viewModelType, ViewModelPatchingType.Selectively);
		}

		[Test]
		public void NotFoundPropertyForExecuteMethodWhenUsingNotUseSearchByNameAttribute_ValidViewModel() {
			const string fieldName = "anyActionCommand";
			const string propertyName = "AnyActionCommand";
			const string executeMethodName = "ExecuteAnyActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddField(fieldName, CommandType.Type)
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet)
				.AddMethod(executeMethodName, typeof(void), null, new NotUseSearchByNameAttribute(), new ConnectMethodToPropertyAttribute(propertyName))
				.Build();

			CheckValidViewModel(viewModelType, ViewModelPatchingType.All, (executeMethodName, null, propertyName, fieldName));
			CheckValidViewModel(viewModelType, ViewModelPatchingType.Selectively, (executeMethodName, null, propertyName, fieldName));
		}

		[Test]
		public void NotFoundFieldForPropertyWhenUsingNotUseSearchByNameAttribute_InvalidViewModel() {
			const string fieldName = "anyActionCommand";
			const string propertyName = "AnyActionCommand";
			const string executeMethodName = "ExecuteAnyActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddField(fieldName, CommandType.Type)
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet, new NotUseSearchByNameAttribute())
				.AddMethod(executeMethodName, typeof(void), null)
				.Build();

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.All,
				$"Not found field for property '{propertyName}' when using '{nameof(NotUseSearchByNameAttribute)}'");

			CheckValidViewModel(viewModelType, ViewModelPatchingType.Selectively);
		}

		[Test]
		public void NotFoundFieldForPropertyWhenUsingNotUseSearchByNameAttribute_ValidViewModel() {
			const string fieldName = "anyActionCommand";
			const string propertyName = "AnyActionCommand";
			const string executeMethodName = "ExecuteAnyActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddField(fieldName, CommandType.Type)
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet, new NotUseSearchByNameAttribute(), new ConnectPropertyToFieldAttribute(fieldName))
				.AddMethod(executeMethodName, typeof(void), null)
				.Build();

			CheckValidViewModel(viewModelType, ViewModelPatchingType.All, (executeMethodName, null, propertyName, fieldName));
			CheckValidViewModel(viewModelType, ViewModelPatchingType.Selectively);
		}
	}
}