using ApplicationPatcher.Tests;
using ApplicationPatcher.Tests.FakeTypes;
using ApplicationPatcher.Wpf.Types.Attributes.ViewModel;
using ApplicationPatcher.Wpf.Types.Enums;
using NUnit.Framework;

namespace ApplicationPatcher.Wpf.Tests.Services.Groupers.Command {
	[TestFixture]
	public class CommandGrouperServiceAttributesTests : CommandGrouperServiceTestsBase {
		[Test]
		public void PatchingMethodCanNotHaveParameters_InvalidViewModel() {
			const string executeMethodName = "ExecuteAnyActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddMethod(executeMethodName, typeof(void), new[] { new FakeParameter("parameter", typeof(int)) }, new PatchingCommandAttribute())
				.Build();

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.All,
				$"Patching method '{executeMethodName}' can not have parameters");

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.Selectively,
				$"Patching method '{executeMethodName}' can not have parameters");
		}

		[Test]
		public void PatchingMethodCanNotHaveParameters_ValidViewModel() {
			const string executeMethodName = "ExecuteAnyActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddMethod(executeMethodName, typeof(void), null, new PatchingCommandAttribute())
				.Build();

			CheckValidViewModel(viewModelType, ViewModelPatchingType.All, false, false, (executeMethodName, null, null, null));
			CheckValidViewModel(viewModelType, ViewModelPatchingType.Selectively, false, false, (executeMethodName, null, null, null));
		}

		[Test]
		public void PatchingMethodCannotHavePatchingAndNotPatchingCommandAttribute_InvalidViewModel() {
			const string executeMethodName = "ExecuteAnyActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddMethod(executeMethodName, typeof(void), null, new PatchingCommandAttribute(), new NotPatchingCommandAttribute())
				.Build();

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.All,
				$"Patching method '{executeMethodName}' can not have '{nameof(PatchingCommandAttribute)}' and '{nameof(NotPatchingCommandAttribute)}' at the same time");

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.Selectively,
				$"Patching method '{executeMethodName}' can not have '{nameof(PatchingCommandAttribute)}' and '{nameof(NotPatchingCommandAttribute)}' at the same time");
		}

		[Test]
		public void PatchingMethodCannotHavePatchingAndNotPatchingCommandAttribute_ValidViewModel() {
			const string executeMethodName = "ExecuteAnyActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddMethod(executeMethodName, typeof(void), null, new PatchingCommandAttribute())
				.Build();

			CheckValidViewModel(viewModelType, ViewModelPatchingType.All, false, false, (executeMethodName, null, null, null));
			CheckValidViewModel(viewModelType, ViewModelPatchingType.Selectively, false, false, (executeMethodName, null, null, null));
		}

		[Test]
		public void PatchingMethodCanNotHaveSuchReturnType_InvalidViewModel() {
			const string executeMethodName = "ExecuteAnyActionMethod";

			var firstViewModelType = FakeCommonTypeBuilder.Create("FirstViewModel")
				.AddMethod(executeMethodName, typeof(int), null, new PatchingCommandAttribute())
				.Build();

			var secondViewModelType = FakeCommonTypeBuilder.Create("SecondViewModel")
				.AddMethod(executeMethodName, typeof(bool), null, new PatchingCommandAttribute())
				.Build();

			CheckInvalidViewModel(firstViewModelType,
				ViewModelPatchingType.All,
				$"Patching method '{executeMethodName}' can not have '{typeof(int).FullName}' return type, allowable types: '{typeof(void).FullName}'");

			CheckInvalidViewModel(firstViewModelType,
				ViewModelPatchingType.Selectively,
				$"Patching method '{executeMethodName}' can not have '{typeof(int).FullName}' return type, allowable types: '{typeof(void).FullName}'");

			CheckInvalidViewModel(secondViewModelType,
				ViewModelPatchingType.All,
				$"Patching method '{executeMethodName}' can not have '{typeof(bool).FullName}' return type, allowable types: '{typeof(void).FullName}'");

			CheckInvalidViewModel(secondViewModelType,
				ViewModelPatchingType.Selectively,
				$"Patching method '{executeMethodName}' can not have '{typeof(bool).FullName}' return type, allowable types: '{typeof(void).FullName}'");
		}

		[Test]
		public void PatchingMethodCanNotHaveSuchReturnType_ValidViewModel() {
			const string executeMethodName = "ExecuteAnyActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddMethod(executeMethodName, typeof(void), null, new PatchingCommandAttribute())
				.Build();

			CheckValidViewModel(viewModelType, ViewModelPatchingType.All, false, false, (executeMethodName, null, null, null));
			CheckValidViewModel(viewModelType, ViewModelPatchingType.Selectively, false, false, (executeMethodName, null, null, null));
		}

		[Test]
		public void PatchingMethodWithConnectMethodToMethodAttributeCanNotHaveParameters_InvalidViewModel() {
			const string executeMethodName = "ExecuteAnyActionMethod";
			const string canExecuteMethodName = "CanExecuteAnyActionMethod";

			var firstViewModelType = FakeCommonTypeBuilder.Create("FirstViewModel")
				.AddMethod(executeMethodName, typeof(void), new[] { new FakeParameter("parameter", typeof(int)) }, new ConnectMethodToMethodAttribute(canExecuteMethodName))
				.AddMethod(canExecuteMethodName, typeof(bool), null)
				.Build();

			var secondViewModelType = FakeCommonTypeBuilder.Create("SecondViewModel")
				.AddMethod(executeMethodName, typeof(void), null)
				.AddMethod(canExecuteMethodName, typeof(bool), new[] { new FakeParameter("parameter", typeof(int)) }, new ConnectMethodToMethodAttribute(executeMethodName))
				.Build();

			CheckInvalidViewModel(firstViewModelType,
				ViewModelPatchingType.All,
				$"Patching method '{executeMethodName}' can not have parameters");

			CheckInvalidViewModel(firstViewModelType,
				ViewModelPatchingType.Selectively,
				$"Patching method '{executeMethodName}' can not have parameters");

			CheckInvalidViewModel(secondViewModelType,
				ViewModelPatchingType.All,
				$"Patching method '{canExecuteMethodName}' can not have parameters");

			CheckInvalidViewModel(secondViewModelType,
				ViewModelPatchingType.Selectively,
				$"Patching method '{canExecuteMethodName}' can not have parameters");
		}

		[Test]
		public void PatchingMethodWithConnectMethodToMethodAttributeCanNotHaveParameters_ValidViewModel() {
			const string executeMethodName = "ExecuteAnyActionMethod";
			const string canExecuteMethodName = "CanExecuteAnyActionMethod";

			var firstViewModelType = FakeCommonTypeBuilder.Create("FirstViewModel")
				.AddMethod(executeMethodName, typeof(void), null, new ConnectMethodToMethodAttribute(canExecuteMethodName))
				.AddMethod(canExecuteMethodName, typeof(bool), null)
				.Build();

			var secondViewModelType = FakeCommonTypeBuilder.Create("SecondViewModel")
				.AddMethod(executeMethodName, typeof(void), null)
				.AddMethod(canExecuteMethodName, typeof(bool), null, new ConnectMethodToMethodAttribute(executeMethodName))
				.Build();

			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.All, false, false, (executeMethodName, canExecuteMethodName, null, null));
			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.Selectively, false, false, (executeMethodName, canExecuteMethodName, null, null));

			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.All, false, false, (executeMethodName, canExecuteMethodName, null, null));
			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.Selectively, false, false, (executeMethodName, canExecuteMethodName, null, null));
		}

		[Test]
		public void PatchingMethodWithConnectMethodToMethodAttributeCanNotHaveNotPatchingCommandAttribute_InvalidViewModel() {
			const string executeMethodName = "ExecuteAnyActionMethod";
			const string canExecuteMethodName = "CanExecuteAnyActionMethod";

			var firstViewModelType = FakeCommonTypeBuilder.Create("FirstViewModel")
				.AddMethod(executeMethodName, typeof(void), null, new ConnectMethodToMethodAttribute(canExecuteMethodName), new NotPatchingCommandAttribute())
				.AddMethod(canExecuteMethodName, typeof(bool), null)
				.Build();

			var secondViewModelType = FakeCommonTypeBuilder.Create("SecondViewModel")
				.AddMethod(executeMethodName, typeof(void), null)
				.AddMethod(canExecuteMethodName, typeof(bool), null, new ConnectMethodToMethodAttribute(executeMethodName), new NotPatchingCommandAttribute())
				.Build();

			CheckInvalidViewModel(firstViewModelType,
				ViewModelPatchingType.All,
				$"Patching method '{executeMethodName}' can not have '{nameof(NotPatchingCommandAttribute)}'");

			CheckInvalidViewModel(firstViewModelType,
				ViewModelPatchingType.Selectively,
				$"Patching method '{executeMethodName}' can not have '{nameof(NotPatchingCommandAttribute)}'");

			CheckInvalidViewModel(secondViewModelType,
				ViewModelPatchingType.All,
				$"Patching method '{canExecuteMethodName}' can not have '{nameof(NotPatchingCommandAttribute)}'");

			CheckInvalidViewModel(secondViewModelType,
				ViewModelPatchingType.Selectively,
				$"Patching method '{canExecuteMethodName}' can not have '{nameof(NotPatchingCommandAttribute)}'");
		}

		[Test]
		public void PatchingMethodWithConnectMethodToMethodAttributeCanNotHaveNotPatchingCommandAttribute_ValidViewModel() {
			const string executeMethodName = "ExecuteAnyActionMethod";
			const string canExecuteMethodName = "CanExecuteAnyActionMethod";

			var firstViewModelType = FakeCommonTypeBuilder.Create("FirstViewModel")
				.AddMethod(executeMethodName, typeof(void), null, new ConnectMethodToMethodAttribute(canExecuteMethodName))
				.AddMethod(canExecuteMethodName, typeof(bool), null)
				.Build();

			var secondViewModelType = FakeCommonTypeBuilder.Create("SecondViewModel")
				.AddMethod(executeMethodName, typeof(void), null)
				.AddMethod(canExecuteMethodName, typeof(bool), null, new ConnectMethodToMethodAttribute(executeMethodName))
				.Build();

			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.All, false, false, (executeMethodName, canExecuteMethodName, null, null));
			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.Selectively, false, false, (executeMethodName, canExecuteMethodName, null, null));

			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.All, false, false, (executeMethodName, canExecuteMethodName, null, null));
			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.Selectively, false, false, (executeMethodName, canExecuteMethodName, null, null));
		}

		[Test]
		public void PatchingMethodWithConnectMethodToMethodAttributeCanNotHaveSuchReturnType_InvalidViewModel() {
			const string executeMethodName = "ExecuteAnyActionMethod";
			const string canExecuteMethodName = "CanExecuteAnyActionMethod";

			var firstViewModelType = FakeCommonTypeBuilder.Create("FirstViewModel")
				.AddMethod(executeMethodName, typeof(int), null, new ConnectMethodToMethodAttribute(canExecuteMethodName))
				.AddMethod(canExecuteMethodName, typeof(bool), null)
				.Build();

			var secondViewModelType = FakeCommonTypeBuilder.Create("SecondViewModel")
				.AddMethod(executeMethodName, typeof(void), null)
				.AddMethod(canExecuteMethodName, typeof(int), null, new ConnectMethodToMethodAttribute(executeMethodName))
				.Build();

			CheckInvalidViewModel(firstViewModelType,
				ViewModelPatchingType.All,
				$"Patching method '{executeMethodName}' can not have '{typeof(int).FullName}' return type, allowable types: '{typeof(void).FullName}', '{typeof(bool).FullName}'");

			CheckInvalidViewModel(firstViewModelType,
				ViewModelPatchingType.Selectively,
				$"Patching method '{executeMethodName}' can not have '{typeof(int).FullName}' return type, allowable types: '{typeof(void).FullName}', '{typeof(bool).FullName}'");

			CheckInvalidViewModel(secondViewModelType,
				ViewModelPatchingType.All,
				$"Patching method '{canExecuteMethodName}' can not have '{typeof(int).FullName}' return type, allowable types: '{typeof(void).FullName}', '{typeof(bool).FullName}'");

			CheckInvalidViewModel(secondViewModelType,
				ViewModelPatchingType.Selectively,
				$"Patching method '{canExecuteMethodName}' can not have '{typeof(int).FullName}' return type, allowable types: '{typeof(void).FullName}', '{typeof(bool).FullName}'");
		}

		[Test]
		public void PatchingMethodWithConnectMethodToMethodAttributeCanNotHaveSuchReturnType_ValidViewModel() {
			const string executeMethodName = "ExecuteAnyActionMethod";
			const string canExecuteMethodName = "CanExecuteAnyActionMethod";

			var firstViewModelType = FakeCommonTypeBuilder.Create("FirstViewModel")
				.AddMethod(executeMethodName, typeof(void), null, new ConnectMethodToMethodAttribute(canExecuteMethodName))
				.AddMethod(canExecuteMethodName, typeof(bool), null)
				.Build();

			var secondViewModelType = FakeCommonTypeBuilder.Create("SecondViewModel")
				.AddMethod(executeMethodName, typeof(void), null)
				.AddMethod(canExecuteMethodName, typeof(bool), null, new ConnectMethodToMethodAttribute(executeMethodName))
				.Build();

			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.All, false, false, (executeMethodName, canExecuteMethodName, null, null));
			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.Selectively, false, false, (executeMethodName, canExecuteMethodName, null, null));

			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.All, false, false, (executeMethodName, canExecuteMethodName, null, null));
			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.Selectively, false, false, (executeMethodName, canExecuteMethodName, null, null));
		}

		[Test]
		public void NotFoundMethodSpecifiedInConnectMethodToMethodAttribute_InvalidViewModel() {
			const string executeMethodName = "ExecuteAnyActionMethod";
			const string canExecuteMethodName = "CanExecuteAnyActionMethod";

			var firstViewModelType = FakeCommonTypeBuilder.Create("FirstViewModel")
				.AddMethod(executeMethodName, typeof(void), null, new ConnectMethodToMethodAttribute(canExecuteMethodName))
				.Build();

			CheckInvalidViewModel(firstViewModelType,
				ViewModelPatchingType.All,
				$"Not found method with name '{canExecuteMethodName}', specified in '{nameof(ConnectMethodToMethodAttribute)}' at method '{executeMethodName}'");

			CheckInvalidViewModel(firstViewModelType,
				ViewModelPatchingType.Selectively,
				$"Not found method with name '{canExecuteMethodName}', specified in '{nameof(ConnectMethodToMethodAttribute)}' at method '{executeMethodName}'");
		}

		[Test]
		public void NotFoundMethodSpecifiedInConnectMethodToMethodAttribute_ValidViewModel() {
			const string executeMethodName = "ExecuteAnyActionMethod";
			const string canExecuteMethodName = "CanExecuteAnyActionMethod";

			var firstViewModelType = FakeCommonTypeBuilder.Create("FirstViewModel")
				.AddMethod(executeMethodName, typeof(void), null, new ConnectMethodToMethodAttribute(canExecuteMethodName))
				.AddMethod(canExecuteMethodName, typeof(bool), null)
				.Build();

			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.All, false, false, (executeMethodName, canExecuteMethodName, null, null));
			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.Selectively, false, false, (executeMethodName, canExecuteMethodName, null, null));
		}

		[Test]
		public void FoundSeveralMethodsSpecifiedInConnectMethodToMethodAttribute_InvalidViewModel() {
			const string executeMethodName = "ExecuteAnyActionMethod";
			const string canExecuteMethodName = "CanExecuteAnyActionMethod";

			var firstViewModelType = FakeCommonTypeBuilder.Create("FirstViewModel")
				.AddMethod(executeMethodName, typeof(void), null, new ConnectMethodToMethodAttribute(canExecuteMethodName))
				.AddMethod(canExecuteMethodName, typeof(bool), new[] { new FakeParameter("parameter", typeof(int)) })
				.AddMethod(canExecuteMethodName, typeof(bool), null)
				.Build();

			CheckInvalidViewModel(firstViewModelType,
				ViewModelPatchingType.All,
				$"Found several methods with name '{canExecuteMethodName}', specified in '{nameof(ConnectMethodToMethodAttribute)}' at method '{executeMethodName}'");

			CheckInvalidViewModel(firstViewModelType,
				ViewModelPatchingType.Selectively,
				$"Found several methods with name '{canExecuteMethodName}', specified in '{nameof(ConnectMethodToMethodAttribute)}' at method '{executeMethodName}'");
		}

		[Test]
		public void FoundSeveralMethodsSpecifiedInConnectMethodToMethodAttribute_ValidViewModel() {
			const string executeMethodName = "ExecuteAnyActionMethod";
			const string canExecuteMethodName = "CanExecuteAnyActionMethod";

			var firstViewModelType = FakeCommonTypeBuilder.Create("FirstViewModel")
				.AddMethod(executeMethodName, typeof(void), null, new ConnectMethodToMethodAttribute(canExecuteMethodName))
				.AddMethod(canExecuteMethodName, typeof(bool), null)
				.Build();

			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.All, false, false, (executeMethodName, canExecuteMethodName, null, null));
			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.Selectively, false, false, (executeMethodName, canExecuteMethodName, null, null));
		}

		[Test]
		public void PatchingMethodSpecifiedInConnectMethodToMethodAttributeCanNotHaveParameters_InvalidViewModel() {
			const string executeMethodName = "ExecuteAnyActionMethod";
			const string canExecuteMethodName = "CanExecuteAnyActionMethod";

			var firstViewModelType = FakeCommonTypeBuilder.Create("FirstViewModel")
				.AddMethod(executeMethodName, typeof(void), null, new ConnectMethodToMethodAttribute(canExecuteMethodName))
				.AddMethod(canExecuteMethodName, typeof(bool), new[] { new FakeParameter("parameter", typeof(int)) })
				.Build();

			var secondViewModelType = FakeCommonTypeBuilder.Create("SecondViewModel")
				.AddMethod(executeMethodName, typeof(void), new[] { new FakeParameter("parameter", typeof(int)) })
				.AddMethod(canExecuteMethodName, typeof(bool), null, new ConnectMethodToMethodAttribute(executeMethodName))
				.Build();

			CheckInvalidViewModel(firstViewModelType,
				ViewModelPatchingType.All,
				$"Patching method '{canExecuteMethodName}' can not have parameters, connection in '{nameof(ConnectMethodToMethodAttribute)}' at method '{executeMethodName}'");

			CheckInvalidViewModel(firstViewModelType,
				ViewModelPatchingType.Selectively,
				$"Patching method '{canExecuteMethodName}' can not have parameters, connection in '{nameof(ConnectMethodToMethodAttribute)}' at method '{executeMethodName}'");

			CheckInvalidViewModel(secondViewModelType,
				ViewModelPatchingType.All,
				$"Patching method '{executeMethodName}' can not have parameters, connection in '{nameof(ConnectMethodToMethodAttribute)}' at method '{canExecuteMethodName}'");

			CheckInvalidViewModel(secondViewModelType,
				ViewModelPatchingType.Selectively,
				$"Patching method '{executeMethodName}' can not have parameters, connection in '{nameof(ConnectMethodToMethodAttribute)}' at method '{canExecuteMethodName}'");
		}

		[Test]
		public void PatchingMethodSpecifiedInConnectMethodToMethodAttributeCanNotHaveParameters_ValidViewModel() {
			const string executeMethodName = "ExecuteAnyActionMethod";
			const string canExecuteMethodName = "CanExecuteAnyActionMethod";

			var firstViewModelType = FakeCommonTypeBuilder.Create("FirstViewModel")
				.AddMethod(executeMethodName, typeof(void), null, new ConnectMethodToMethodAttribute(canExecuteMethodName))
				.AddMethod(canExecuteMethodName, typeof(bool), null)
				.Build();

			var secondViewModelType = FakeCommonTypeBuilder.Create("SecondViewModel")
				.AddMethod(executeMethodName, typeof(void), null)
				.AddMethod(canExecuteMethodName, typeof(bool), null, new ConnectMethodToMethodAttribute(executeMethodName))
				.Build();

			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.All, false, false, (executeMethodName, canExecuteMethodName, null, null));
			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.Selectively, false, false, (executeMethodName, canExecuteMethodName, null, null));

			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.All, false, false, (executeMethodName, canExecuteMethodName, null, null));
			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.Selectively, false, false, (executeMethodName, canExecuteMethodName, null, null));
		}

		[Test]
		public void PatchingMethodSpecifiedInConnectMethodToMethodAttributeCanNotHaveNotPatchingCommandAttribute_InvalidViewModel() {
			const string executeMethodName = "ExecuteAnyActionMethod";
			const string canExecuteMethodName = "CanExecuteAnyActionMethod";

			var firstViewModelType = FakeCommonTypeBuilder.Create("FirstViewModel")
				.AddMethod(executeMethodName, typeof(void), null, new ConnectMethodToMethodAttribute(canExecuteMethodName))
				.AddMethod(canExecuteMethodName, typeof(bool), null, new NotPatchingCommandAttribute())
				.Build();

			var secondViewModelType = FakeCommonTypeBuilder.Create("SecondViewModel")
				.AddMethod(executeMethodName, typeof(void), null, new NotPatchingCommandAttribute())
				.AddMethod(canExecuteMethodName, typeof(bool), null, new ConnectMethodToMethodAttribute(executeMethodName))
				.Build();

			CheckInvalidViewModel(firstViewModelType,
				ViewModelPatchingType.All,
				$"Patching method '{canExecuteMethodName}' can not have '{nameof(NotPatchingCommandAttribute)}', connection in '{nameof(ConnectMethodToMethodAttribute)}' at method '{executeMethodName}'");

			CheckInvalidViewModel(firstViewModelType,
				ViewModelPatchingType.Selectively,
				$"Patching method '{canExecuteMethodName}' can not have '{nameof(NotPatchingCommandAttribute)}', connection in '{nameof(ConnectMethodToMethodAttribute)}' at method '{executeMethodName}'");

			CheckInvalidViewModel(secondViewModelType,
				ViewModelPatchingType.All,
				$"Patching method '{executeMethodName}' can not have '{nameof(NotPatchingCommandAttribute)}', connection in '{nameof(ConnectMethodToMethodAttribute)}' at method '{canExecuteMethodName}'");

			CheckInvalidViewModel(secondViewModelType,
				ViewModelPatchingType.Selectively,
				$"Patching method '{executeMethodName}' can not have '{nameof(NotPatchingCommandAttribute)}', connection in '{nameof(ConnectMethodToMethodAttribute)}' at method '{canExecuteMethodName}'");
		}

		[Test]
		public void PatchingMethodSpecifiedInConnectMethodToMethodAttributeCanNotHaveNotPatchingCommandAttribute_ValidViewModel() {
			const string executeMethodName = "ExecuteAnyActionMethod";
			const string canExecuteMethodName = "CanExecuteAnyActionMethod";

			var firstViewModelType = FakeCommonTypeBuilder.Create("FirstViewModel")
				.AddMethod(executeMethodName, typeof(void), null, new ConnectMethodToMethodAttribute(canExecuteMethodName))
				.AddMethod(canExecuteMethodName, typeof(bool), null)
				.Build();

			var secondViewModelType = FakeCommonTypeBuilder.Create("SecondViewModel")
				.AddMethod(executeMethodName, typeof(void), null)
				.AddMethod(canExecuteMethodName, typeof(bool), null, new ConnectMethodToMethodAttribute(executeMethodName))
				.Build();

			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.All, false, false, (executeMethodName, canExecuteMethodName, null, null));
			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.Selectively, false, false, (executeMethodName, canExecuteMethodName, null, null));

			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.All, false, false, (executeMethodName, canExecuteMethodName, null, null));
			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.Selectively, false, false, (executeMethodName, canExecuteMethodName, null, null));
		}

		[Test]
		public void PatchingMethodSpecifiedInConnectMethodToMethodAttributeCanNotHaveSuchReturnType_InvalidViewModel() {
			const string executeMethodName = "ExecuteAnyActionMethod";
			const string canExecuteMethodName = "CanExecuteAnyActionMethod";

			var firstViewModelType = FakeCommonTypeBuilder.Create("FirstViewModel")
				.AddMethod(executeMethodName, typeof(void), null, new ConnectMethodToMethodAttribute(canExecuteMethodName))
				.AddMethod(canExecuteMethodName, typeof(int), null)
				.Build();

			var secondViewModelType = FakeCommonTypeBuilder.Create("SecondViewModel")
				.AddMethod(executeMethodName, typeof(int), null)
				.AddMethod(canExecuteMethodName, typeof(bool), null, new ConnectMethodToMethodAttribute(executeMethodName))
				.Build();

			CheckInvalidViewModel(firstViewModelType,
				ViewModelPatchingType.All,
				$"Patching method '{canExecuteMethodName}' can not have '{typeof(int).FullName}' return type, allowable types: '{typeof(void).FullName}', '{typeof(bool).FullName}', connection in '{nameof(ConnectMethodToMethodAttribute)}' at method '{executeMethodName}'");

			CheckInvalidViewModel(firstViewModelType,
				ViewModelPatchingType.Selectively,
				$"Patching method '{canExecuteMethodName}' can not have '{typeof(int).FullName}' return type, allowable types: '{typeof(void).FullName}', '{typeof(bool).FullName}', connection in '{nameof(ConnectMethodToMethodAttribute)}' at method '{executeMethodName}'");

			CheckInvalidViewModel(secondViewModelType,
				ViewModelPatchingType.All,
				$"Patching method '{executeMethodName}' can not have '{typeof(int).FullName}' return type, allowable types: '{typeof(void).FullName}', '{typeof(bool).FullName}', connection in '{nameof(ConnectMethodToMethodAttribute)}' at method '{canExecuteMethodName}'");

			CheckInvalidViewModel(secondViewModelType,
				ViewModelPatchingType.Selectively,
				$"Patching method '{executeMethodName}' can not have '{typeof(int).FullName}' return type, allowable types: '{typeof(void).FullName}', '{typeof(bool).FullName}', connection in '{nameof(ConnectMethodToMethodAttribute)}' at method '{canExecuteMethodName}'");
		}

		[Test]
		public void PatchingMethodSpecifiedInConnectMethodToMethodAttributeCanNotHaveSuchReturnType_ValidViewModel() {
			const string executeMethodName = "ExecuteAnyActionMethod";
			const string canExecuteMethodName = "CanExecuteAnyActionMethod";

			var firstViewModelType = FakeCommonTypeBuilder.Create("FirstViewModel")
				.AddMethod(executeMethodName, typeof(void), null, new ConnectMethodToMethodAttribute(canExecuteMethodName))
				.AddMethod(canExecuteMethodName, typeof(bool), null)
				.Build();

			var secondViewModelType = FakeCommonTypeBuilder.Create("SecondViewModel")
				.AddMethod(executeMethodName, typeof(void), null)
				.AddMethod(canExecuteMethodName, typeof(bool), null, new ConnectMethodToMethodAttribute(executeMethodName))
				.Build();

			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.All, false, false, (executeMethodName, canExecuteMethodName, null, null));
			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.Selectively, false, false, (executeMethodName, canExecuteMethodName, null, null));

			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.All, false, false, (executeMethodName, canExecuteMethodName, null, null));
			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.Selectively, false, false, (executeMethodName, canExecuteMethodName, null, null));
		}

		[Test]
		public void CanNotBeConnectTwoExecuteMethods_InvalidViewModel() {
			const string firstExecuteMethodName = "ExecuteAnyFirstActionMethod";
			const string secondExecuteMethodName = "ExecuteAnySecondActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddMethod(firstExecuteMethodName, typeof(void), null, new ConnectMethodToMethodAttribute(secondExecuteMethodName))
				.AddMethod(secondExecuteMethodName, typeof(void), null)
				.Build();

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.All,
				$"Can not be connect two execute methods: '{firstExecuteMethodName}' and '{secondExecuteMethodName}', connection in '{nameof(ConnectMethodToMethodAttribute)}' at method '{firstExecuteMethodName}'");

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.Selectively,
				$"Can not be connect two execute methods: '{firstExecuteMethodName}' and '{secondExecuteMethodName}', connection in '{nameof(ConnectMethodToMethodAttribute)}' at method '{firstExecuteMethodName}'");
		}

		[Test]
		public void CanNotBeConnectTwoExecuteMethods_ValidViewModel() {
			const string firstExecuteMethodName = "ExecuteAnyFirstActionMethod";
			const string secondExecuteMethodName = "ExecuteAnySecondActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddMethod(firstExecuteMethodName, typeof(void), null, new ConnectMethodToMethodAttribute(secondExecuteMethodName))
				.AddMethod(secondExecuteMethodName, typeof(bool), null)
				.Build();

			CheckValidViewModel(viewModelType, ViewModelPatchingType.All, false, false, (firstExecuteMethodName, secondExecuteMethodName, null, null));
			CheckValidViewModel(viewModelType, ViewModelPatchingType.Selectively, false, false, (firstExecuteMethodName, secondExecuteMethodName, null, null));
		}

		[Test]
		public void CanNotBeConnectTwoCanExecuteMethods_InvalidViewModel() {
			const string firstCanExecuteMethodName = "CanExecuteAnyFirstActionMethod";
			const string secondCanExecuteMethodName = "CanExecuteAnySecondActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddMethod(firstCanExecuteMethodName, typeof(bool), null, new ConnectMethodToMethodAttribute(secondCanExecuteMethodName))
				.AddMethod(secondCanExecuteMethodName, typeof(bool), null)
				.Build();

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.All,
				$"Can not be connect two can execute methods: '{firstCanExecuteMethodName}' and '{secondCanExecuteMethodName}', connection in '{nameof(ConnectMethodToMethodAttribute)}' at method '{firstCanExecuteMethodName}'");

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.Selectively,
				$"Can not be connect two can execute methods: '{firstCanExecuteMethodName}' and '{secondCanExecuteMethodName}', connection in '{nameof(ConnectMethodToMethodAttribute)}' at method '{firstCanExecuteMethodName}'");
		}

		[Test]
		public void CanNotBeConnectTwoCanExecuteMethods_ValidViewModel() {
			const string firstExecuteMethodName = "ExecuteAnyFirstActionMethod";
			const string secondExecuteMethodName = "ExecuteAnySecondActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddMethod(firstExecuteMethodName, typeof(void), null, new ConnectMethodToMethodAttribute(secondExecuteMethodName))
				.AddMethod(secondExecuteMethodName, typeof(bool), null)
				.Build();

			CheckValidViewModel(viewModelType, ViewModelPatchingType.All, false, false, (firstExecuteMethodName, secondExecuteMethodName, null, null));
			CheckValidViewModel(viewModelType, ViewModelPatchingType.Selectively, false, false, (firstExecuteMethodName, secondExecuteMethodName, null, null));
		}

		[Test]
		public void PatchingMethodWithConnectMethodToPropertyAttributeCanNotHaveParameters_InvalidViewModel() {
			const string propertyName = "AnyActionCommand";
			const string executeMethodName = "ExecuteAnyActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet)
				.AddMethod(executeMethodName, typeof(void), new[] { new FakeParameter("parameter", typeof(int)) }, new ConnectMethodToPropertyAttribute(propertyName))
				.Build();

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.All,
				$"Patching method '{executeMethodName}' can not have parameters");

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.Selectively,
				$"Patching method '{executeMethodName}' can not have parameters");
		}

		[Test]
		public void PatchingMethodWithConnectMethodToPropertyAttributeCanNotHaveParameters_ValidViewModel() {
			const string propertyName = "AnyActionCommand";
			const string executeMethodName = "ExecuteAnyActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet)
				.AddMethod(executeMethodName, typeof(void), null, new ConnectMethodToPropertyAttribute(propertyName))
				.Build();

			CheckValidViewModel(viewModelType, ViewModelPatchingType.All, false, false, (executeMethodName, null, propertyName, null));
			CheckValidViewModel(viewModelType, ViewModelPatchingType.Selectively, false, false, (executeMethodName, null, propertyName, null));
		}

		[Test]
		public void PatchingMethodWithConnectMethodToPropertyAttributeCanNotHaveNotPatchingCommandAttribute_InvalidViewModel() {
			const string propertyName = "AnyActionCommand";
			const string executeMethodName = "ExecuteAnyActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet)
				.AddMethod(executeMethodName, typeof(void), null, new ConnectMethodToPropertyAttribute(propertyName), new NotPatchingCommandAttribute())
				.Build();

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.All,
				$"Patching method '{executeMethodName}' can not have '{nameof(NotPatchingCommandAttribute)}'");

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.Selectively,
				$"Patching method '{executeMethodName}' can not have '{nameof(NotPatchingCommandAttribute)}'");
		}

		[Test]
		public void PatchingMethodWithConnectMethodToPropertyAttributeCanNotHaveNotPatchingCommandAttribute_ValidViewModel() {
			const string propertyName = "AnyActionCommand";
			const string executeMethodName = "ExecuteAnyActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet)
				.AddMethod(executeMethodName, typeof(void), null, new ConnectMethodToPropertyAttribute(propertyName))
				.Build();

			CheckValidViewModel(viewModelType, ViewModelPatchingType.All, false, false, (executeMethodName, null, propertyName, null));
			CheckValidViewModel(viewModelType, ViewModelPatchingType.Selectively, false, false, (executeMethodName, null, propertyName, null));
		}

		[Test]
		public void PatchingMethodWithConnectMethodToPropertyAttributeCanNotHaveSuchReturnType_InvalidViewModel() {
			const string propertyName = "AnyActionCommand";
			const string executeMethodName = "ExecuteAnyActionMethod";

			var firstViewModelType = FakeCommonTypeBuilder.Create("FirstViewModel")
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet)
				.AddMethod(executeMethodName, typeof(bool), null, new ConnectMethodToPropertyAttribute(propertyName))
				.Build();

			var secondViewModelType = FakeCommonTypeBuilder.Create("SecondViewModel")
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet)
				.AddMethod(executeMethodName, typeof(int), null, new ConnectMethodToPropertyAttribute(propertyName))
				.Build();

			CheckInvalidViewModel(firstViewModelType,
				ViewModelPatchingType.All,
				$"Patching method '{executeMethodName}' can not have '{typeof(bool).FullName}' return type, allowable types: '{typeof(void).FullName}'");

			CheckInvalidViewModel(firstViewModelType,
				ViewModelPatchingType.Selectively,
				$"Patching method '{executeMethodName}' can not have '{typeof(bool).FullName}' return type, allowable types: '{typeof(void).FullName}'");

			CheckInvalidViewModel(secondViewModelType,
				ViewModelPatchingType.All,
				$"Patching method '{executeMethodName}' can not have '{typeof(int).FullName}' return type, allowable types: '{typeof(void).FullName}'");

			CheckInvalidViewModel(secondViewModelType,
				ViewModelPatchingType.Selectively,
				$"Patching method '{executeMethodName}' can not have '{typeof(int).FullName}' return type, allowable types: '{typeof(void).FullName}'");
		}

		[Test]
		public void PatchingMethodWithConnectMethodToPropertyAttributeCanNotHaveSuchReturnType_ValidViewModel() {
			const string propertyName = "AnyActionCommand";
			const string executeMethodName = "ExecuteAnyActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet)
				.AddMethod(executeMethodName, typeof(void), null, new ConnectMethodToPropertyAttribute(propertyName))
				.Build();

			CheckValidViewModel(viewModelType, ViewModelPatchingType.All, false, false, (executeMethodName, null, propertyName, null));
			CheckValidViewModel(viewModelType, ViewModelPatchingType.Selectively, false, false, (executeMethodName, null, propertyName, null));
		}

		[Test]
		public void NotFoundPropertySpecifiedInConnectMethodToPropertyAttribute_InvalidViewModel() {
			const string propertyName = "AnyActionCommand";
			const string executeMethodName = "ExecuteAnyActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddMethod(executeMethodName, typeof(void), null, new ConnectMethodToPropertyAttribute(propertyName))
				.Build();

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.All,
				$"Not found property with name '{propertyName}', specified in '{nameof(ConnectMethodToPropertyAttribute)}' at method '{executeMethodName}'");

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.Selectively,
				$"Not found property with name '{propertyName}', specified in '{nameof(ConnectMethodToPropertyAttribute)}' at method '{executeMethodName}'");
		}

		[Test]
		public void NotFoundPropertySpecifiedInConnectMethodToPropertyAttribute_ValidViewModel() {
			const string propertyName = "AnyActionCommand";
			const string executeMethodName = "ExecuteAnyActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet)
				.AddMethod(executeMethodName, typeof(void), null, new ConnectMethodToPropertyAttribute(propertyName))
				.Build();

			CheckValidViewModel(viewModelType, ViewModelPatchingType.All, false, false, (executeMethodName, null, propertyName, null));
			CheckValidViewModel(viewModelType, ViewModelPatchingType.Selectively, false, false, (executeMethodName, null, propertyName, null));
		}

		[Test]
		public void PropertySpecifiedInConnectMethodToPropertyAttributeCanNotHaveSuchType_InvalidViewModel() {
			const string propertyName = "AnyActionCommand";
			const string executeMethodName = "ExecuteAnyActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddProperty(propertyName, typeof(int), PropertyMethods.HasGetAndSet)
				.AddMethod(executeMethodName, typeof(void), null, new ConnectMethodToPropertyAttribute(propertyName))
				.Build();

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.All,
				$"Property '{propertyName}' can not have '{typeof(int).FullName}' type, allowable types: all inherited from '{KnownTypeNames.ICommand}', connection in '{nameof(ConnectMethodToPropertyAttribute)}' at method '{executeMethodName}'");

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.Selectively,
				$"Property '{propertyName}' can not have '{typeof(int).FullName}' type, allowable types: all inherited from '{KnownTypeNames.ICommand}', connection in '{nameof(ConnectMethodToPropertyAttribute)}' at method '{executeMethodName}'");
		}

		[Test]
		public void PropertySpecifiedInConnectMethodToPropertyAttributeCanNotHaveSuchType_ValidViewModel() {
			const string propertyName = "AnyActionCommand";
			const string executeMethodName = "ExecuteAnyActionMethod";

			var firstViewModelType = FakeCommonTypeBuilder.Create("FirstViewModel")
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet)
				.AddMethod(executeMethodName, typeof(void), null, new ConnectMethodToPropertyAttribute(propertyName))
				.Build();

			var secondViewModelType = FakeCommonTypeBuilder.Create("SecondViewModel")
				.AddProperty(propertyName, RelayCommand.Type, PropertyMethods.HasGetAndSet)
				.AddMethod(executeMethodName, typeof(void), null, new ConnectMethodToPropertyAttribute(propertyName))
				.Build();

			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.All, false, false, (executeMethodName, null, propertyName, null));
			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.Selectively, false, false, (executeMethodName, null, propertyName, null));

			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.All, false, false, (executeMethodName, null, propertyName, null));
			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.Selectively, false, false, (executeMethodName, null, propertyName, null));
		}

		[Test]
		public void PropertyWithConnectPropertyToMethodAttributeCanNotHaveSuchType_InvalidViewModel() {
			const string propertyName = "AnyActionCommand";
			const string executeMethodName = "ExecuteAnyActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddProperty(propertyName, typeof(int), PropertyMethods.HasGetAndSet, new ConnectPropertyToMethodAttribute(executeMethodName))
				.AddMethod(executeMethodName, typeof(void), null)
				.Build();

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.All,
				$"Patching property '{propertyName}' can not have '{typeof(int).FullName}' type, allowable types: all inherited from '{KnownTypeNames.ICommand}'");

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.Selectively,
				$"Patching property '{propertyName}' can not have '{typeof(int).FullName}' type, allowable types: all inherited from '{KnownTypeNames.ICommand}'");
		}

		[Test]
		public void PropertyWithConnectPropertyToMethodAttributeCanNotHaveSuchType_ValidViewModel() {
			const string propertyName = "AnyActionCommand";
			const string executeMethodName = "ExecuteAnyActionMethod";

			var firstViewModelType = FakeCommonTypeBuilder.Create("FirstViewModel")
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet, new ConnectPropertyToMethodAttribute(executeMethodName))
				.AddMethod(executeMethodName, typeof(void), null)
				.Build();

			var secondViewModelType = FakeCommonTypeBuilder.Create("SecondViewModel")
				.AddProperty(propertyName, RelayCommand.Type, PropertyMethods.HasGetAndSet, new ConnectPropertyToMethodAttribute(executeMethodName))
				.AddMethod(executeMethodName, typeof(void), null)
				.Build();

			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.All, false, false, (executeMethodName, null, propertyName, null));
			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.Selectively, false, false, (executeMethodName, null, propertyName, null));

			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.All, false, false, (executeMethodName, null, propertyName, null));
			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.Selectively, false, false, (executeMethodName, null, propertyName, null));
		}

		[Test]
		public void NotFoundMethodSpecifiedInConnectPropertyToMethodAttribute_InvalidViewModel() {
			const string propertyName = "AnyActionCommand";
			const string executeMethodName = "ExecuteAnyActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet, new ConnectPropertyToMethodAttribute(executeMethodName))
				.Build();

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.All,
				$"Not found method with name '{executeMethodName}', specified in '{nameof(ConnectPropertyToMethodAttribute)}' at property '{propertyName}'");

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.Selectively,
				$"Not found method with name '{executeMethodName}', specified in '{nameof(ConnectPropertyToMethodAttribute)}' at property '{propertyName}'");
		}

		[Test]
		public void NotFoundMethodSpecifiedInConnectPropertyToMethodAttribute_ValidViewModel() {
			const string propertyName = "AnyActionCommand";
			const string executeMethodName = "ExecuteAnyActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet, new ConnectPropertyToMethodAttribute(executeMethodName))
				.AddMethod(executeMethodName, typeof(void), null)
				.Build();

			CheckValidViewModel(viewModelType, ViewModelPatchingType.All, false, false, (executeMethodName, null, propertyName, null));
			CheckValidViewModel(viewModelType, ViewModelPatchingType.Selectively, false, false, (executeMethodName, null, propertyName, null));
		}

		[Test]
		public void FoundSeveralMethodsSpecifiedInConnectPropertyToMethodAttribute_InvalidViewModel() {
			const string propertyName = "AnyActionCommand";
			const string executeMethodName = "ExecuteAnyActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet, new ConnectPropertyToMethodAttribute(executeMethodName))
				.AddMethod(executeMethodName, typeof(void), null)
				.AddMethod(executeMethodName, typeof(void), new[] { new FakeParameter("parameter", typeof(int)) })
				.Build();

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.All,
				$"Found several methods with name '{executeMethodName}', specified in '{nameof(ConnectPropertyToMethodAttribute)}' at property '{propertyName}'");

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.Selectively,
				$"Found several methods with name '{executeMethodName}', specified in '{nameof(ConnectPropertyToMethodAttribute)}' at property '{propertyName}'");
		}

		[Test]
		public void FoundSeveralMethodsSpecifiedInConnectPropertyToMethodAttribute_ValidViewModel() {
			const string propertyName = "AnyActionCommand";
			const string executeMethodName = "ExecuteAnyActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet, new ConnectPropertyToMethodAttribute(executeMethodName))
				.AddMethod(executeMethodName, typeof(void), null)
				.Build();

			CheckValidViewModel(viewModelType, ViewModelPatchingType.All, false, false, (executeMethodName, null, propertyName, null));
			CheckValidViewModel(viewModelType, ViewModelPatchingType.Selectively, false, false, (executeMethodName, null, propertyName, null));
		}

		[Test]
		public void PatchingMethodSpecifiedInConnectPropertyToMethodAttributeCanNotHaveParameters_InvalidViewModel() {
			const string propertyName = "AnyActionCommand";
			const string executeMethodName = "ExecuteAnyActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet, new ConnectPropertyToMethodAttribute(executeMethodName))
				.AddMethod(executeMethodName, typeof(void), new[] { new FakeParameter("parameter", typeof(int)) })
				.Build();

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.All,
				$"Patching method '{executeMethodName}' can not have parameters, connection in '{nameof(ConnectPropertyToMethodAttribute)}' at property '{propertyName}'");

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.Selectively,
				$"Patching method '{executeMethodName}' can not have parameters, connection in '{nameof(ConnectPropertyToMethodAttribute)}' at property '{propertyName}'");
		}

		[Test]
		public void PatchingMethodSpecifiedInConnectPropertyToMethodAttributeCanNotHaveParameters_ValidViewModel() {
			const string propertyName = "AnyActionCommand";
			const string executeMethodName = "ExecuteAnyActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet, new ConnectPropertyToMethodAttribute(executeMethodName))
				.AddMethod(executeMethodName, typeof(void), null)
				.Build();

			CheckValidViewModel(viewModelType, ViewModelPatchingType.All, false, false, (executeMethodName, null, propertyName, null));
			CheckValidViewModel(viewModelType, ViewModelPatchingType.Selectively, false, false, (executeMethodName, null, propertyName, null));
		}

		[Test]
		public void PatchingMethodSpecifiedInConnectPropertyToMethodAttributeCanNotHaveNotPatchingCommandAttribute_InvalidViewModel() {
			const string propertyName = "AnyActionCommand";
			const string executeMethodName = "ExecuteAnyActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet, new ConnectPropertyToMethodAttribute(executeMethodName))
				.AddMethod(executeMethodName, typeof(void), null, new NotPatchingCommandAttribute())
				.Build();

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.All,
				$"Patching method '{executeMethodName}' can not have '{nameof(NotPatchingCommandAttribute)}', connection in '{nameof(ConnectPropertyToMethodAttribute)}' at property '{propertyName}'");

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.Selectively,
				$"Patching method '{executeMethodName}' can not have '{nameof(NotPatchingCommandAttribute)}', connection in '{nameof(ConnectPropertyToMethodAttribute)}' at property '{propertyName}'");
		}

		[Test]
		public void PatchingMethodSpecifiedInConnectPropertyToMethodAttributeCanNotHaveNotPatchingCommandAttribute_ValidViewModel() {
			const string propertyName = "AnyActionCommand";
			const string executeMethodName = "ExecuteAnyActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet, new ConnectPropertyToMethodAttribute(executeMethodName))
				.AddMethod(executeMethodName, typeof(void), null)
				.Build();

			CheckValidViewModel(viewModelType, ViewModelPatchingType.All, false, false, (executeMethodName, null, propertyName, null));
			CheckValidViewModel(viewModelType, ViewModelPatchingType.Selectively, false, false, (executeMethodName, null, propertyName, null));
		}

		[Test]
		public void PatchingMethodSpecifiedInConnectPropertyToMethodAttributeCanNotHaveSuchReturnType_InvalidViewModel() {
			const string propertyName = "AnyActionCommand";
			const string executeMethodName = "ExecuteAnyActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet, new ConnectPropertyToMethodAttribute(executeMethodName))
				.AddMethod(executeMethodName, typeof(int), null)
				.Build();

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.All,
				$"Patching method '{executeMethodName}' can not have '{typeof(int).FullName}' return type, allowable types: '{typeof(void).FullName}', '{typeof(bool).FullName}', connection in '{nameof(ConnectPropertyToMethodAttribute)}' at property '{propertyName}'");

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.Selectively,
				$"Patching method '{executeMethodName}' can not have '{typeof(int).FullName}' return type, allowable types: '{typeof(void).FullName}', '{typeof(bool).FullName}', connection in '{nameof(ConnectPropertyToMethodAttribute)}' at property '{propertyName}'");
		}

		[Test]
		public void PatchingMethodSpecifiedInConnectPropertyToMethodAttributeCanNotHaveSuchReturnType_ValidViewModel() {
			const string propertyName = "AnyActionCommand";
			const string executeMethodName = "ExecuteAnyActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet, new ConnectPropertyToMethodAttribute(executeMethodName))
				.AddMethod(executeMethodName, typeof(void), null)
				.Build();

			CheckValidViewModel(viewModelType, ViewModelPatchingType.All, false, false, (executeMethodName, null, propertyName, null));
			CheckValidViewModel(viewModelType, ViewModelPatchingType.Selectively, false, false, (executeMethodName, null, propertyName, null));
		}

		[Test]
		public void CanNotBeConnectToCanExecuteMethod_InvalidViewModel() {
			const string propertyName = "AnyActionCommand";
			const string executeMethodName = "ExecuteAnyActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet, new ConnectPropertyToMethodAttribute(executeMethodName))
				.AddMethod(executeMethodName, typeof(bool), null)
				.Build();

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.All,
				$"Can not be connect to can execute method '{executeMethodName}', connection in '{nameof(ConnectPropertyToMethodAttribute)}' at property '{propertyName}'");

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.Selectively,
				$"Can not be connect to can execute method '{executeMethodName}', connection in '{nameof(ConnectPropertyToMethodAttribute)}' at property '{propertyName}'");
		}

		[Test]
		public void CanNotBeConnectToCanExecuteMethod_ValidViewModel() {
			const string propertyName = "AnyActionCommand";
			const string executeMethodName = "ExecuteAnyActionMethod";
			const string canExecuteMethodName = "CanExecuteAnyActionMethod";

			var firstViewModelType = FakeCommonTypeBuilder.Create("FirstViewModel")
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet, new ConnectPropertyToMethodAttribute(executeMethodName))
				.AddMethod(executeMethodName, typeof(void), null)
				.Build();

			var secondViewModelType = FakeCommonTypeBuilder.Create("SecondViewModel")
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet, new ConnectPropertyToMethodAttribute(executeMethodName, canExecuteMethodName))
				.AddMethod(executeMethodName, typeof(void), null)
				.AddMethod(canExecuteMethodName, typeof(bool), null)
				.Build();

			var thirdViewModelType = FakeCommonTypeBuilder.Create("ThirdViewModel")
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet, new ConnectPropertyToMethodAttribute(canExecuteMethodName, executeMethodName))
				.AddMethod(executeMethodName, typeof(void), null)
				.AddMethod(canExecuteMethodName, typeof(bool), null)
				.Build();

			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.All, false, false, (executeMethodName, null, propertyName, null));
			CheckValidViewModel(firstViewModelType, ViewModelPatchingType.Selectively, false, false, (executeMethodName, null, propertyName, null));

			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.All, false, false, (executeMethodName, canExecuteMethodName, propertyName, null));
			CheckValidViewModel(secondViewModelType, ViewModelPatchingType.Selectively, false, false, (executeMethodName, canExecuteMethodName, propertyName, null));

			CheckValidViewModel(thirdViewModelType, ViewModelPatchingType.All, false, false, (executeMethodName, canExecuteMethodName, propertyName, null));
			CheckValidViewModel(thirdViewModelType, ViewModelPatchingType.Selectively, false, false, (executeMethodName, canExecuteMethodName, propertyName, null));
		}

		[Test]
		public void CanNotBeConnectToTwoExecuteMethods_InvalidViewModel() {
			const string propertyName = "AnyActionCommand";
			const string firstExecuteMethodName = "ExecuteAnyFirstActionMethod";
			const string secondExecuteMethodName = "ExecuteAnySecondActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet, new ConnectPropertyToMethodAttribute(firstExecuteMethodName, secondExecuteMethodName))
				.AddMethod(firstExecuteMethodName, typeof(void), null)
				.AddMethod(secondExecuteMethodName, typeof(void), null)
				.Build();

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.All,
				$"Can not be connect to two execute methods: '{firstExecuteMethodName}' and '{secondExecuteMethodName}', connection in '{nameof(ConnectPropertyToMethodAttribute)}' at property '{propertyName}'");

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.Selectively,
				$"Can not be connect to two execute methods: '{firstExecuteMethodName}' and '{secondExecuteMethodName}', connection in '{nameof(ConnectPropertyToMethodAttribute)}' at property '{propertyName}'");
		}

		[Test]
		public void CanNotBeConnectToTwoExecuteMethods_ValidViewModel() {
			const string propertyName = "AnyActionCommand";
			const string executeMethodName = "ExecuteAnyActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet, new ConnectPropertyToMethodAttribute(executeMethodName))
				.AddMethod(executeMethodName, typeof(void), null)
				.Build();

			CheckValidViewModel(viewModelType, ViewModelPatchingType.All, false, false, (executeMethodName, null, propertyName, null));
			CheckValidViewModel(viewModelType, ViewModelPatchingType.Selectively, false, false, (executeMethodName, null, propertyName, null));
		}

		[Test]
		public void CanNotBeConnectToTwoCanExecuteMethods_InvalidViewModel() {
			const string propertyName = "AnyActionCommand";
			const string firstCanExecuteMethodName = "CanExecuteAnyFirstActionMethod";
			const string secondCanExecuteMethodName = "CanExecuteAnySecondActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet, new ConnectPropertyToMethodAttribute(firstCanExecuteMethodName, secondCanExecuteMethodName))
				.AddMethod(firstCanExecuteMethodName, typeof(bool), null)
				.AddMethod(secondCanExecuteMethodName, typeof(bool), null)
				.Build();

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.All,
				$"Can not be connect to two can execute methods: '{firstCanExecuteMethodName}' and '{secondCanExecuteMethodName}', connection in '{nameof(ConnectPropertyToMethodAttribute)}' at property '{propertyName}'");

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.Selectively,
				$"Can not be connect to two can execute methods: '{firstCanExecuteMethodName}' and '{secondCanExecuteMethodName}', connection in '{nameof(ConnectPropertyToMethodAttribute)}' at property '{propertyName}'");
		}

		[Test]
		public void CanNotBeConnectToTwoCanExecuteMethods_ValidViewModel() {
			const string propertyName = "AnyActionCommand";
			const string executeMethodName = "ExecuteAnyActionMethod";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet, new ConnectPropertyToMethodAttribute(executeMethodName))
				.AddMethod(executeMethodName, typeof(void), null)
				.Build();

			CheckValidViewModel(viewModelType, ViewModelPatchingType.All, false, false, (executeMethodName, null, propertyName, null));
			CheckValidViewModel(viewModelType, ViewModelPatchingType.Selectively, false, false, (executeMethodName, null, propertyName, null));
		}

		[Test]
		public void NotFoundFieldSpecifiedInConnectPropertyToFieldAttribute_InvalidViewModel() {
			const string fieldName = "anyActionCommand";
			const string propertyName = "AnyActionCommand";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet, new ConnectPropertyToFieldAttribute(fieldName))
				.Build();

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.All,
				$"Not found field with name '{fieldName}', specified in '{nameof(ConnectPropertyToFieldAttribute)}' at property '{propertyName}'");

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.Selectively,
				$"Not found field with name '{fieldName}', specified in '{nameof(ConnectPropertyToFieldAttribute)}' at property '{propertyName}'");
		}

		[Test]
		public void NotFoundFieldSpecifiedInConnectPropertyToFieldAttribute_ValidViewModel() {
			const string fieldName = "anyActionCommand";
			const string propertyName = "AnyActionCommand";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddField(fieldName, CommandType.Type)
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet, new ConnectPropertyToFieldAttribute(fieldName))
				.Build();

			CheckValidViewModel(viewModelType, ViewModelPatchingType.All, false, false);
			CheckValidViewModel(viewModelType, ViewModelPatchingType.Selectively, false, false);
		}

		[Test]
		public void TypesDoNotMatchBetweenPropertyAndField_InvalidViewModel() {
			const string fieldName = "anyActionCommand";
			const string propertyName = "AnyActionCommand";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddField(fieldName, RelayCommand.Type)
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet, new ConnectPropertyToFieldAttribute(fieldName))
				.Build();

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.All,
				$"Types do not match between property '{propertyName}' and field '{fieldName}', connection in '{nameof(ConnectPropertyToFieldAttribute)}' at property '{propertyName}'");

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.Selectively,
				$"Types do not match between property '{propertyName}' and field '{fieldName}', connection in '{nameof(ConnectPropertyToFieldAttribute)}' at property '{propertyName}'");
		}

		[Test]
		public void TypesDoNotMatchBetweenPropertyAndField_ValidViewModel() {
			const string fieldName = "anyActionCommand";
			const string propertyName = "AnyActionCommand";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddField(fieldName, CommandType.Type)
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet, new ConnectPropertyToFieldAttribute(fieldName))
				.Build();

			CheckValidViewModel(viewModelType, ViewModelPatchingType.All, false, false);
			CheckValidViewModel(viewModelType, ViewModelPatchingType.Selectively, false, false);
		}

		[Test]
		public void NotFoundPropertySpecifiedInConnectFieldToPropertyAttribute_InvalidViewModel() {
			const string fieldName = "anyActionCommand";
			const string propertyName = "AnyActionCommand";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddField(fieldName, CommandType.Type, new ConnectFieldToPropertyAttribute(propertyName))
				.Build();

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.All,
				$"Not found property with name '{propertyName}', specified in '{nameof(ConnectFieldToPropertyAttribute)}' at field '{fieldName}'");

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.Selectively,
				$"Not found property with name '{propertyName}', specified in '{nameof(ConnectFieldToPropertyAttribute)}' at field '{fieldName}'");
		}

		[Test]
		public void NotFoundPropertySpecifiedInConnectFieldToPropertyAttribute_ValidViewModel() {
			const string fieldName = "anyActionCommand";
			const string propertyName = "AnyActionCommand";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddField(fieldName, CommandType.Type, new ConnectFieldToPropertyAttribute(propertyName))
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet)
				.Build();

			CheckValidViewModel(viewModelType, ViewModelPatchingType.All, false, false);
			CheckValidViewModel(viewModelType, ViewModelPatchingType.Selectively, false, false);
		}

		[Test]
		public void TypesDoNotMatchBetweenFieldAndProperty_InvalidViewModel() {
			const string fieldName = "anyActionCommand";
			const string propertyName = "AnyActionCommand";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddField(fieldName, RelayCommand.Type, new ConnectFieldToPropertyAttribute(propertyName))
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet)
				.Build();

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.All,
				$"Types do not match between field '{fieldName}' and property '{propertyName}', connection in '{nameof(ConnectFieldToPropertyAttribute)}' at field '{fieldName}'");

			CheckInvalidViewModel(viewModelType,
				ViewModelPatchingType.Selectively,
				$"Types do not match between field '{fieldName}' and property '{propertyName}', connection in '{nameof(ConnectFieldToPropertyAttribute)}' at field '{fieldName}'");
		}

		[Test]
		public void TypesDoNotMatchBetweenFieldAndProperty_ValidViewModel() {
			const string fieldName = "anyActionCommand";
			const string propertyName = "AnyActionCommand";

			var viewModelType = FakeCommonTypeBuilder.Create("ViewModel")
				.AddField(fieldName, CommandType.Type, new ConnectFieldToPropertyAttribute(propertyName))
				.AddProperty(propertyName, CommandType.Type, PropertyMethods.HasGetAndSet)
				.Build();

			CheckValidViewModel(viewModelType, ViewModelPatchingType.All, false, false);
			CheckValidViewModel(viewModelType, ViewModelPatchingType.Selectively, false, false);
		}
	}
}