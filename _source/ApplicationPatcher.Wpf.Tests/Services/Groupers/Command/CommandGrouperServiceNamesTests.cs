using System.Diagnostics.CodeAnalysis;
using ApplicationPatcher.Tests;
using ApplicationPatcher.Tests.FakeTypes;
using ApplicationPatcher.Wpf.Types.Attributes.Connect;
using ApplicationPatcher.Wpf.Types.Enums;
using NUnit.Framework;

namespace ApplicationPatcher.Wpf.Tests.Services.Groupers.Command {
	[TestFixture]
	[SuppressMessage("ReSharper", "StringLiteralTypo")]
	public class CommandGrouperServiceNamesTests : CommandGrouperServiceTestsBase {
		[Test]
		public void NotValidPatchingCommandMethodName_InvalidViewModel() {
			const string propertyName = "AnyActionCommand";
			const string firstExecuteMethodName = "ExecuteAnyAcTIonMethod";
			const string secondExecuteMethodName = "ExecutAnyActionMethod";
			const string thirdExecuteMethodName = "ExecuteAnyActionMetho";

			var firstViewModelType = FakeCommonTypeBuilder.Create("FirstViewModel")
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet)
				.AddMethod(firstExecuteMethodName, typeof(void), null)
				.Build();

			var secondViewModelType = FakeCommonTypeBuilder.Create("SecondViewModel")
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet)
				.AddMethod(secondExecuteMethodName, typeof(void), null)
				.Build();

			var thirdViewModelType = FakeCommonTypeBuilder.Create("ThirdViewModel")
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet)
				.AddMethod(thirdExecuteMethodName, typeof(void), null)
				.Build();

			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.All, true, false);
			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.Selectively, true, false);

			CheckInvalidViewModel(firstViewModelType,
				ViewModelPatchingType.All,
				$"Not valid patching command method name '{firstExecuteMethodName}'");

			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.Selectively);

			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.All, true, false);
			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.Selectively, true, false);

			CheckInvalidViewModel(secondViewModelType,
				ViewModelPatchingType.All,
				$"Not valid patching command method name '{secondExecuteMethodName}'");

			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.Selectively);

			CheckValidViewModel(thirdViewModelType, ViewModelPatchingType.All, true, false);
			CheckValidViewModel(thirdViewModelType, ViewModelPatchingType.Selectively, true, false);

			CheckInvalidViewModel(thirdViewModelType,
				ViewModelPatchingType.All,
				$"Not valid patching command method name '{thirdExecuteMethodName}'");

			CheckValidViewModel(thirdViewModelType, ViewModelPatchingType.Selectively);
		}

		[Test]
		public void NotValidPatchingCommandMethodName_ValidViewModel() {
			const string propertyName = "AnyActionCommand";
			const string executeMethodName = "ExecuteAnyActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet)
				.AddMethod(executeMethodName, typeof(void), null)
				.Build();

			CheckValidViewModel(viewModelType, ViewModelPatchingType.All, (executeMethodName, null, propertyName, null));
			CheckValidViewModel(viewModelType, ViewModelPatchingType.Selectively);
		}

		[Test]
		public void NotValidPatchingCommandPropertyName_InvalidViewModel() {
			const string fieldName = "anyActionCommand";
			const string firstPropertyName = "AnyAcTIonCommand";
			const string secondPropertyName = "AnyActionComand";
			const string thirdPropertyName = "AnyActionCommands";
			const string executeMethodName = "ExecuteAnyActionMethod";

			var firstViewModelType = FakeCommonTypeBuilder.Create("FirstViewModel")
				.AddField(fieldName, CommandType.Type)
				.AddProperty(firstPropertyName, CommandType.Type, PropertyMethods.HasGetAndSet)
				.AddMethod(executeMethodName, typeof(void), null, new ConnectMethodToPropertyAttribute(firstPropertyName))
				.Build();

			var secondViewModelType = FakeCommonTypeBuilder.Create("SecondViewModel")
				.AddField(fieldName, CommandType.Type)
				.AddProperty(secondPropertyName, CommandType.Type, PropertyMethods.HasGetAndSet)
				.AddMethod(executeMethodName, typeof(void), null, new ConnectMethodToPropertyAttribute(secondPropertyName))
				.Build();

			var thirdViewModelType = FakeCommonTypeBuilder.Create("ThirdViewModel")
				.AddField(fieldName, CommandType.Type)
				.AddProperty(thirdPropertyName, CommandType.Type, PropertyMethods.HasGetAndSet)
				.AddMethod(executeMethodName, typeof(void), null, new ConnectMethodToPropertyAttribute(thirdPropertyName))
				.Build();

			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.All, true, false, (executeMethodName, null, firstPropertyName, null));
			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.Selectively, true, false, (executeMethodName, null, firstPropertyName, null));

			CheckInvalidViewModel(firstViewModelType,
				ViewModelPatchingType.All,
				$"Not valid patching command property name '{firstPropertyName}'");

			CheckInvalidViewModel(firstViewModelType,
				ViewModelPatchingType.Selectively,
				$"Not valid patching command property name '{firstPropertyName}'");

			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.All, true, false, (executeMethodName, null, secondPropertyName, null));
			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.Selectively, true, false, (executeMethodName, null, secondPropertyName, null));

			CheckInvalidViewModel(secondViewModelType,
				ViewModelPatchingType.All,
				$"Not valid patching command property name '{secondPropertyName}'");

			CheckInvalidViewModel(secondViewModelType,
				ViewModelPatchingType.Selectively,
				$"Not valid patching command property name '{secondPropertyName}'");

			CheckValidViewModel(thirdViewModelType, ViewModelPatchingType.All, true, false, (executeMethodName, null, thirdPropertyName, null));
			CheckValidViewModel(thirdViewModelType, ViewModelPatchingType.Selectively, true, false, (executeMethodName, null, thirdPropertyName, null));

			CheckInvalidViewModel(thirdViewModelType,
				ViewModelPatchingType.All,
				$"Not valid patching command property name '{thirdPropertyName}'");

			CheckInvalidViewModel(thirdViewModelType,
				ViewModelPatchingType.Selectively,
				$"Not valid patching command property name '{thirdPropertyName}'");
		}

		[Test]
		public void NotValidPatchingCommandPropertyName_ValidViewModel() {
			const string fieldName = "anyActionCommand";
			const string propertyName = "AnyActionCommand";
			const string executeMethodName = "ExecuteAnyActionMethod";
			const string canExecuteMethodName = "CanExecuteAnyActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddField(fieldName, CommandType.Type)
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet)
				.AddMethod(executeMethodName, typeof(void), null)
				.AddMethod(canExecuteMethodName, typeof(bool), null)
				.Build();

			CheckValidViewModel(viewModelType, ViewModelPatchingType.All, (executeMethodName, canExecuteMethodName, propertyName, fieldName));
			CheckValidViewModel(viewModelType, ViewModelPatchingType.Selectively);
		}

		[Test]
		public void HaveExecuteMethodAndFieldWithoutProperty() {
			const string fieldName = "anyActionCommand";
			const string executeMethodName = "ExecuteAnyActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddField(fieldName, CommandType.Type)
				.AddMethod(executeMethodName, typeof(void), null)
				.Build();

			CheckValidViewModel(viewModelType, ViewModelPatchingType.All, (executeMethodName, null, null, fieldName));
			CheckValidViewModel(viewModelType, ViewModelPatchingType.Selectively);
		}
	}
}