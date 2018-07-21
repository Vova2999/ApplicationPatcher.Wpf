using ApplicationPatcher.Tests;
using ApplicationPatcher.Tests.FakeTypes;
using ApplicationPatcher.Wpf.Types.Attributes.FrameworkElement;
using ApplicationPatcher.Wpf.Types.Enums;
using NUnit.Framework;

namespace ApplicationPatcher.Wpf.Tests.Services.Groupers.Dependency {
	[TestFixture]
	public class DependencyGrouperServiceAttributesTests : DependencyGrouperServiceTestsBase {
		[Test]
		public void PatchingPropertyCanNotHavePatchingAndNotPatchingDependencyAttribute_InvalidFrameworkElement() {
			const string patchingPropertyName = "AnyValue";

			var frameworkElementType = FakeCommonTypeBuilder.Create("FrameworkElement")
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new PatchingDependencyPropertyAttribute(), new NotPatchingDependencyPropertyAttribute())
				.Build();

			CheckInvalidFrameworkElement(frameworkElementType,
				FrameworkElementPatchingType.All,
				$"Patching property '{patchingPropertyName}' can not have '{nameof(PatchingDependencyPropertyAttribute)}' and '{nameof(NotPatchingDependencyPropertyAttribute)}' at the same time");

			CheckInvalidFrameworkElement(frameworkElementType,
				FrameworkElementPatchingType.Selectively,
				$"Patching property '{patchingPropertyName}' can not have '{nameof(PatchingDependencyPropertyAttribute)}' and '{nameof(NotPatchingDependencyPropertyAttribute)}' at the same time");
		}

		[Test]
		public void PatchingPropertyCanNotHavePatchingAndNotPatchingDependencyAttribute_ValidFrameworkElement() {
			const string patchingPropertyName = "AnyValue";

			var frameworkElementType = FakeCommonTypeBuilder.Create("FrameworkElement")
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new PatchingDependencyPropertyAttribute())
				.Build();

			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.All, false, false, (patchingPropertyName, null));
			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.Selectively, false, false, (patchingPropertyName, null));
		}

		[Test]
		public void PatchingPropertyCanNotHaveNotPatchingDependencyPropertyAttribute_InvalidFrameworkElement() {
			const string patchingFieldName = "AnyValueProperty";
			const string patchingPropertyName = "AnyValue";

			var frameworkElementType = FakeCommonTypeBuilder.Create("FrameworkElement")
				.AddField(patchingFieldName, DependencyPropertyType.Type)
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new NotPatchingDependencyPropertyAttribute(), new ConnectPropertyToDependencyAttribute(patchingFieldName))
				.Build();

			SetStaticField(frameworkElementType, patchingFieldName);

			CheckInvalidFrameworkElement(frameworkElementType,
				FrameworkElementPatchingType.All,
				$"Patching property '{patchingPropertyName}' can not have '{nameof(NotPatchingDependencyPropertyAttribute)}', connection in '{nameof(ConnectPropertyToDependencyAttribute)}' at property '{patchingPropertyName}'");

			CheckInvalidFrameworkElement(frameworkElementType,
				FrameworkElementPatchingType.Selectively,
				$"Patching property '{patchingPropertyName}' can not have '{nameof(NotPatchingDependencyPropertyAttribute)}', connection in '{nameof(ConnectPropertyToDependencyAttribute)}' at property '{patchingPropertyName}'");
		}

		[Test]
		public void PatchingPropertyCanNotHaveNotPatchingDependencyPropertyAttribute_ValidFrameworkElement() {
			const string patchingFieldName = "AnyValueProperty";
			const string patchingPropertyName = "AnyValue";

			var frameworkElementType = FakeCommonTypeBuilder.Create("FrameworkElement")
				.AddField(patchingFieldName, DependencyPropertyType.Type)
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new ConnectPropertyToDependencyAttribute(patchingFieldName))
				.Build();

			SetStaticField(frameworkElementType, patchingFieldName);

			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.All, false, false, (patchingPropertyName, patchingFieldName));
			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.Selectively, false, false, (patchingPropertyName, patchingFieldName));
		}

		[Test]
		public void NotFoundDependencyFieldSpecifiedInConnectPropertyToDependencyAttribute_InvalidFrameworkElement() {
			const string patchingFieldName = "AnyValueProperty";
			const string patchingPropertyName = "AnyValue";

			var frameworkElementType = FakeCommonTypeBuilder.Create("FrameworkElement")
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new ConnectPropertyToDependencyAttribute(patchingFieldName))
				.Build();

			CheckInvalidFrameworkElement(frameworkElementType,
				FrameworkElementPatchingType.All,
				$"Not found dependency field with name '{patchingFieldName}', specified in '{nameof(ConnectPropertyToDependencyAttribute)}' at property '{patchingPropertyName}'");

			CheckInvalidFrameworkElement(frameworkElementType,
				FrameworkElementPatchingType.Selectively,
				$"Not found dependency field with name '{patchingFieldName}', specified in '{nameof(ConnectPropertyToDependencyAttribute)}' at property '{patchingPropertyName}'");
		}

		[Test]
		public void NotFoundDependencyFieldSpecifiedInConnectPropertyToDependencyAttribute_ValidFrameworkElement() {
			const string patchingFieldName = "AnyValueProperty";
			const string patchingPropertyName = "AnyValue";

			var frameworkElementType = FakeCommonTypeBuilder.Create("FrameworkElement")
				.AddField(patchingFieldName, DependencyPropertyType.Type)
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new ConnectPropertyToDependencyAttribute(patchingFieldName))
				.Build();

			SetStaticField(frameworkElementType, patchingFieldName);

			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.All, false, false, (patchingPropertyName, patchingFieldName));
			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.Selectively, false, false, (patchingPropertyName, patchingFieldName));
		}

		[Test]
		public void PatchingDependencyFieldCanNotBeNonStaticSpecifiedInConnectPropertyToDependencyAttribute_InvalidFrameworkElement() {
			const string patchingFieldName = "AnyValueProperty";
			const string patchingPropertyName = "AnyValue";

			var frameworkElementType = FakeCommonTypeBuilder.Create("FrameworkElement")
				.AddField(patchingFieldName, DependencyPropertyType.Type)
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new ConnectPropertyToDependencyAttribute(patchingFieldName))
				.Build();

			CheckInvalidFrameworkElement(frameworkElementType,
				FrameworkElementPatchingType.All,
				$"Patching dependency field '{patchingFieldName}' can not be non static, connection in '{nameof(ConnectPropertyToDependencyAttribute)}' at property '{patchingPropertyName}'");

			CheckInvalidFrameworkElement(frameworkElementType,
				FrameworkElementPatchingType.Selectively,
				$"Patching dependency field '{patchingFieldName}' can not be non static, connection in '{nameof(ConnectPropertyToDependencyAttribute)}' at property '{patchingPropertyName}'");
		}

		[Test]
		public void PatchingDependencyFieldCanNotBeNonStaticSpecifiedInConnectPropertyToDependencyAttribute_ValidFrameworkElement() {
			const string patchingFieldName = "AnyValueProperty";
			const string patchingPropertyName = "AnyValue";

			var frameworkElementType = FakeCommonTypeBuilder.Create("FrameworkElement")
				.AddField(patchingFieldName, DependencyPropertyType.Type)
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new ConnectPropertyToDependencyAttribute(patchingFieldName))
				.Build();

			SetStaticField(frameworkElementType, patchingFieldName);

			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.All, false, false, (patchingPropertyName, patchingFieldName));
			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.Selectively, false, false, (patchingPropertyName, patchingFieldName));
		}

		[Test]
		public void PatchingDependencyFieldCanNotHaveSuchTypeSpecifiedInConnectPropertyToDependencyAttribute_InvalidFrameworkElement() {
			const string patchingFieldName = "AnyValueProperty";
			const string patchingPropertyName = "AnyValue";

			var frameworkElementType = FakeCommonTypeBuilder.Create("FrameworkElement")
				.AddField(patchingFieldName, typeof(int))
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new ConnectPropertyToDependencyAttribute(patchingFieldName))
				.Build();

			SetStaticField(frameworkElementType, patchingFieldName);

			CheckInvalidFrameworkElement(frameworkElementType,
				FrameworkElementPatchingType.All,
				$"Patching dependency field '{patchingFieldName}' can not have '{typeof(int).FullName}' type, allowable types: '{DependencyPropertyType.FullName}', connection in '{nameof(ConnectPropertyToDependencyAttribute)}' at property '{patchingPropertyName}'");

			CheckInvalidFrameworkElement(frameworkElementType,
				FrameworkElementPatchingType.Selectively,
				$"Patching dependency field '{patchingFieldName}' can not have '{typeof(int).FullName}' type, allowable types: '{DependencyPropertyType.FullName}', connection in '{nameof(ConnectPropertyToDependencyAttribute)}' at property '{patchingPropertyName}'");
		}

		[Test]
		public void PatchingDependencyFieldCanNotHaveSuchTypeSpecifiedInConnectPropertyToDependencyAttribute_ValidFrameworkElement() {
			const string patchingFieldName = "AnyValueProperty";
			const string patchingPropertyName = "AnyValue";

			var frameworkElementType = FakeCommonTypeBuilder.Create("FrameworkElement")
				.AddField(patchingFieldName, DependencyPropertyType.Type)
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new ConnectPropertyToDependencyAttribute(patchingFieldName))
				.Build();

			SetStaticField(frameworkElementType, patchingFieldName);

			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.All, false, false, (patchingPropertyName, patchingFieldName));
			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.Selectively, false, false, (patchingPropertyName, patchingFieldName));
		}

		[Test]
		public void NotFoundPropertySpecifiedInConnectDependencyToPropertyAttribute_InvalidFrameworkElement() {
			const string patchingFieldName = "AnyValueProperty";
			const string patchingPropertyName = "AnyValue";

			var frameworkElementType = FakeCommonTypeBuilder.Create("FrameworkElement")
				.AddField(patchingFieldName, DependencyPropertyType.Type, new ConnectDependencyToPropertyAttribute(patchingPropertyName))
				.Build();

			SetStaticField(frameworkElementType, patchingFieldName);

			CheckInvalidFrameworkElement(frameworkElementType,
				FrameworkElementPatchingType.All,
				$"Not found property with name '{patchingPropertyName}', specified in '{nameof(ConnectDependencyToPropertyAttribute)}' at dependency field '{patchingFieldName}'");

			CheckInvalidFrameworkElement(frameworkElementType,
				FrameworkElementPatchingType.Selectively,
				$"Not found property with name '{patchingPropertyName}', specified in '{nameof(ConnectDependencyToPropertyAttribute)}' at dependency field '{patchingFieldName}'");
		}

		[Test]
		public void NotFoundPropertySpecifiedInConnectDependencyToPropertyAttribute_ValidFrameworkElement() {
			const string patchingFieldName = "AnyValueProperty";
			const string patchingPropertyName = "AnyValue";

			var frameworkElementType = FakeCommonTypeBuilder.Create("FrameworkElement")
				.AddField(patchingFieldName, DependencyPropertyType.Type, new ConnectDependencyToPropertyAttribute(patchingPropertyName))
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet)
				.Build();

			SetStaticField(frameworkElementType, patchingFieldName);

			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.All, false, false, (patchingPropertyName, patchingFieldName));
			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.Selectively, false, false, (patchingPropertyName, patchingFieldName));
		}

		[Test]
		public void PatchingPropertyCanNotHaveNotPatchingPropertyAttributeSpecifiedInConnectDependencyToPropertyAttribute_InvalidFrameworkElement() {
			const string patchingFieldName = "AnyValueProperty";
			const string patchingPropertyName = "AnyValue";

			var frameworkElementType = FakeCommonTypeBuilder.Create("FrameworkElement")
				.AddField(patchingFieldName, DependencyPropertyType.Type, new ConnectDependencyToPropertyAttribute(patchingPropertyName))
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new NotPatchingDependencyPropertyAttribute())
				.Build();

			SetStaticField(frameworkElementType, patchingFieldName);

			CheckInvalidFrameworkElement(frameworkElementType,
				FrameworkElementPatchingType.All,
				$"Patching property '{patchingPropertyName}' can not have '{nameof(NotPatchingDependencyPropertyAttribute)}', connection in '{nameof(ConnectDependencyToPropertyAttribute)}' at dependency field '{patchingFieldName}'");

			CheckInvalidFrameworkElement(frameworkElementType,
				FrameworkElementPatchingType.Selectively,
				$"Patching property '{patchingPropertyName}' can not have '{nameof(NotPatchingDependencyPropertyAttribute)}', connection in '{nameof(ConnectDependencyToPropertyAttribute)}' at dependency field '{patchingFieldName}'");
		}

		[Test]
		public void PatchingPropertyCanNotHaveNotPatchingPropertyAttributeSpecifiedInConnectDependencyToPropertyAttribute_ValidFrameworkElement() {
			const string patchingFieldName = "AnyValueProperty";
			const string patchingPropertyName = "AnyValue";

			var frameworkElementType = FakeCommonTypeBuilder.Create("FrameworkElement")
				.AddField(patchingFieldName, DependencyPropertyType.Type, new ConnectDependencyToPropertyAttribute(patchingPropertyName))
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet)
				.Build();

			SetStaticField(frameworkElementType, patchingFieldName);

			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.All, false, false, (patchingPropertyName, patchingFieldName));
			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.Selectively, false, false, (patchingPropertyName, patchingFieldName));
		}

		[Test]
		public void PatchingDependencyFieldCanNotHaveSuchTypeSpecifiedInConnectDependencyToPropertyAttribute_InvalidFrameworkElement() {
			const string patchingFieldName = "AnyValueProperty";
			const string patchingPropertyName = "AnyValue";

			var frameworkElementType = FakeCommonTypeBuilder.Create("FrameworkElement")
				.AddField(patchingFieldName, typeof(int), new ConnectDependencyToPropertyAttribute(patchingPropertyName))
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet)
				.Build();

			SetStaticField(frameworkElementType, patchingFieldName);

			CheckInvalidFrameworkElement(frameworkElementType,
				FrameworkElementPatchingType.All,
				$"Patching dependency field '{patchingFieldName}' can not have '{typeof(int).FullName}' type, allowable types: '{DependencyPropertyType.FullName}', connection in '{nameof(ConnectDependencyToPropertyAttribute)}' at dependency field '{patchingFieldName}'");

			CheckInvalidFrameworkElement(frameworkElementType,
				FrameworkElementPatchingType.Selectively,
				$"Patching dependency field '{patchingFieldName}' can not have '{typeof(int).FullName}' type, allowable types: '{DependencyPropertyType.FullName}', connection in '{nameof(ConnectDependencyToPropertyAttribute)}' at dependency field '{patchingFieldName}'");
		}

		[Test]
		public void PatchingDependencyFieldCanNotHaveSuchTypeSpecifiedInConnectDependencyToPropertyAttribute_ValidFrameworkElement() {
			const string patchingFieldName = "AnyValueProperty";
			const string patchingPropertyName = "AnyValue";

			var frameworkElementType = FakeCommonTypeBuilder.Create("FrameworkElement")
				.AddField(patchingFieldName, DependencyPropertyType.Type, new ConnectDependencyToPropertyAttribute(patchingPropertyName))
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet)
				.Build();

			SetStaticField(frameworkElementType, patchingFieldName);

			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.All, false, false, (patchingPropertyName, patchingFieldName));
			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.Selectively, false, false, (patchingPropertyName, patchingFieldName));
		}
	}
}