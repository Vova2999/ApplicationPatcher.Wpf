using ApplicationPatcher.Tests;
using ApplicationPatcher.Tests.FakeTypes;
using ApplicationPatcher.Wpf.Types.Attributes.Connect;
using ApplicationPatcher.Wpf.Types.Attributes.SelectPatching;
using ApplicationPatcher.Wpf.Types.Enums;
using NUnit.Framework;

namespace ApplicationPatcher.Wpf.Tests.Services.Groupers.Dependency {
	[TestFixture]
	public class DependencyGrouperServiceAttributesTests : DependencyGrouperServiceTestsBase {
		[Test]
		public void PatchingPropertyCanNotHavePatchingAndNotPatchingDependencyAttribute_InvalidFrameworkElement() {
			const string patchingPropertyName = "AnyValue";

			var frameworkElementType = FakeCommonTypeBuilder.Create("FrameworkElement")
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new PatchingPropertyAttribute(), new NotPatchingPropertyAttribute())
				.Build();

			CheckInvalidFrameworkElement(frameworkElementType,
				FrameworkElementPatchingType.All,
				$"Patching property '{patchingPropertyName}' can not have '{nameof(PatchingPropertyAttribute)}' and '{nameof(NotPatchingPropertyAttribute)}' at the same time");

			CheckInvalidFrameworkElement(frameworkElementType,
				FrameworkElementPatchingType.Selectively,
				$"Patching property '{patchingPropertyName}' can not have '{nameof(PatchingPropertyAttribute)}' and '{nameof(NotPatchingPropertyAttribute)}' at the same time");
		}

		[Test]
		public void PatchingPropertyCanNotHavePatchingAndNotPatchingDependencyAttribute_ValidFrameworkElement() {
			const string patchingPropertyName = "AnyValue";

			var frameworkElementType = FakeCommonTypeBuilder.Create("FrameworkElement")
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new PatchingPropertyAttribute())
				.Build();

			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.All, (patchingPropertyName, null));
			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.Selectively, (patchingPropertyName, null));
		}

		[Test]
		public void PatchingPropertyCanNotHaveNotPatchingPropertyAttribute_InvalidFrameworkElement() {
			const string patchingFieldName = "AnyValueProperty";
			const string patchingPropertyName = "AnyValue";

			var frameworkElementType = FakeCommonTypeBuilder.Create("FrameworkElement")
				.AddField(patchingFieldName, DependencyPropertyType.Type)
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new NotPatchingPropertyAttribute(), new ConnectPropertyToFieldAttribute(patchingFieldName))
				.Build();

			SetStaticField(frameworkElementType, patchingFieldName);

			CheckInvalidFrameworkElement(frameworkElementType,
				FrameworkElementPatchingType.All,
				$"Patching property '{patchingPropertyName}' can not have '{nameof(NotPatchingPropertyAttribute)}', connection in '{nameof(ConnectPropertyToFieldAttribute)}' at property '{patchingPropertyName}'");

			CheckInvalidFrameworkElement(frameworkElementType,
				FrameworkElementPatchingType.Selectively,
				$"Patching property '{patchingPropertyName}' can not have '{nameof(NotPatchingPropertyAttribute)}', connection in '{nameof(ConnectPropertyToFieldAttribute)}' at property '{patchingPropertyName}'");
		}

		[Test]
		public void PatchingPropertyCanNotHaveNotPatchingPropertyAttribute_ValidFrameworkElement() {
			const string patchingFieldName = "AnyValueProperty";
			const string patchingPropertyName = "AnyValue";

			var frameworkElementType = FakeCommonTypeBuilder.Create("FrameworkElement")
				.AddField(patchingFieldName, DependencyPropertyType.Type)
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new ConnectPropertyToFieldAttribute(patchingFieldName))
				.Build();

			SetStaticField(frameworkElementType, patchingFieldName);

			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.All, (patchingPropertyName, patchingFieldName));
			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.Selectively, (patchingPropertyName, patchingFieldName));
		}

		[Test]
		public void NotFoundDependencyFieldSpecifiedInConnectPropertyToFieldAttribute_InvalidFrameworkElement() {
			const string patchingFieldName = "AnyValueProperty";
			const string patchingPropertyName = "AnyValue";

			var frameworkElementType = FakeCommonTypeBuilder.Create("FrameworkElement")
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new ConnectPropertyToFieldAttribute(patchingFieldName))
				.Build();

			CheckInvalidFrameworkElement(frameworkElementType,
				FrameworkElementPatchingType.All,
				$"Not found field with name '{patchingFieldName}', specified in '{nameof(ConnectPropertyToFieldAttribute)}' at property '{patchingPropertyName}'");

			CheckInvalidFrameworkElement(frameworkElementType,
				FrameworkElementPatchingType.Selectively,
				$"Not found field with name '{patchingFieldName}', specified in '{nameof(ConnectPropertyToFieldAttribute)}' at property '{patchingPropertyName}'");
		}

		[Test]
		public void NotFoundDependencyFieldSpecifiedInConnectPropertyToFieldAttribute_ValidFrameworkElement() {
			const string patchingFieldName = "AnyValueProperty";
			const string patchingPropertyName = "AnyValue";

			var frameworkElementType = FakeCommonTypeBuilder.Create("FrameworkElement")
				.AddField(patchingFieldName, DependencyPropertyType.Type)
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new ConnectPropertyToFieldAttribute(patchingFieldName))
				.Build();

			SetStaticField(frameworkElementType, patchingFieldName);

			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.All, (patchingPropertyName, patchingFieldName));
			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.Selectively, (patchingPropertyName, patchingFieldName));
		}

		[Test]
		public void PatchingDependencyFieldCanNotBeNonStaticSpecifiedInConnectPropertyToFieldAttribute_InvalidFrameworkElement() {
			const string patchingFieldName = "AnyValueProperty";
			const string patchingPropertyName = "AnyValue";

			var frameworkElementType = FakeCommonTypeBuilder.Create("FrameworkElement")
				.AddField(patchingFieldName, DependencyPropertyType.Type)
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new ConnectPropertyToFieldAttribute(patchingFieldName))
				.Build();

			CheckInvalidFrameworkElement(frameworkElementType,
				FrameworkElementPatchingType.All,
				$"Patching field '{patchingFieldName}' can not be non static, connection in '{nameof(ConnectPropertyToFieldAttribute)}' at property '{patchingPropertyName}'");

			CheckInvalidFrameworkElement(frameworkElementType,
				FrameworkElementPatchingType.Selectively,
				$"Patching field '{patchingFieldName}' can not be non static, connection in '{nameof(ConnectPropertyToFieldAttribute)}' at property '{patchingPropertyName}'");
		}

		[Test]
		public void PatchingDependencyFieldCanNotBeNonStaticSpecifiedInConnectPropertyToFieldAttribute_ValidFrameworkElement() {
			const string patchingFieldName = "AnyValueProperty";
			const string patchingPropertyName = "AnyValue";

			var frameworkElementType = FakeCommonTypeBuilder.Create("FrameworkElement")
				.AddField(patchingFieldName, DependencyPropertyType.Type)
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new ConnectPropertyToFieldAttribute(patchingFieldName))
				.Build();

			SetStaticField(frameworkElementType, patchingFieldName);

			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.All, (patchingPropertyName, patchingFieldName));
			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.Selectively, (patchingPropertyName, patchingFieldName));
		}

		[Test]
		public void PatchingDependencyFieldCanNotHaveSuchTypeSpecifiedInConnectPropertyToFieldAttribute_InvalidFrameworkElement() {
			const string patchingFieldName = "AnyValueProperty";
			const string patchingPropertyName = "AnyValue";

			var frameworkElementType = FakeCommonTypeBuilder.Create("FrameworkElement")
				.AddField(patchingFieldName, typeof(int))
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new ConnectPropertyToFieldAttribute(patchingFieldName))
				.Build();

			SetStaticField(frameworkElementType, patchingFieldName);

			CheckInvalidFrameworkElement(frameworkElementType,
				FrameworkElementPatchingType.All,
				$"Patching field '{patchingFieldName}' can not have '{typeof(int).FullName}' type, allowable types: '{DependencyPropertyType.FullName}', connection in '{nameof(ConnectPropertyToFieldAttribute)}' at property '{patchingPropertyName}'");

			CheckInvalidFrameworkElement(frameworkElementType,
				FrameworkElementPatchingType.Selectively,
				$"Patching field '{patchingFieldName}' can not have '{typeof(int).FullName}' type, allowable types: '{DependencyPropertyType.FullName}', connection in '{nameof(ConnectPropertyToFieldAttribute)}' at property '{patchingPropertyName}'");
		}

		[Test]
		public void PatchingDependencyFieldCanNotHaveSuchTypeSpecifiedInConnectPropertyToFieldAttribute_ValidFrameworkElement() {
			const string patchingFieldName = "AnyValueProperty";
			const string patchingPropertyName = "AnyValue";

			var frameworkElementType = FakeCommonTypeBuilder.Create("FrameworkElement")
				.AddField(patchingFieldName, DependencyPropertyType.Type)
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new ConnectPropertyToFieldAttribute(patchingFieldName))
				.Build();

			SetStaticField(frameworkElementType, patchingFieldName);

			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.All, (patchingPropertyName, patchingFieldName));
			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.Selectively, (patchingPropertyName, patchingFieldName));
		}

		[Test]
		public void NotFoundPropertySpecifiedInConnectFieldToPropertyAttribute_InvalidFrameworkElement() {
			const string patchingFieldName = "AnyValueProperty";
			const string patchingPropertyName = "AnyValue";

			var frameworkElementType = FakeCommonTypeBuilder.Create("FrameworkElement")
				.AddField(patchingFieldName, DependencyPropertyType.Type, new ConnectFieldToPropertyAttribute(patchingPropertyName))
				.Build();

			SetStaticField(frameworkElementType, patchingFieldName);

			CheckInvalidFrameworkElement(frameworkElementType,
				FrameworkElementPatchingType.All,
				$"Not found property with name '{patchingPropertyName}', specified in '{nameof(ConnectFieldToPropertyAttribute)}' at field '{patchingFieldName}'");

			CheckInvalidFrameworkElement(frameworkElementType,
				FrameworkElementPatchingType.Selectively,
				$"Not found property with name '{patchingPropertyName}', specified in '{nameof(ConnectFieldToPropertyAttribute)}' at field '{patchingFieldName}'");
		}

		[Test]
		public void NotFoundPropertySpecifiedInConnectFieldToPropertyAttribute_ValidFrameworkElement() {
			const string patchingFieldName = "AnyValueProperty";
			const string patchingPropertyName = "AnyValue";

			var frameworkElementType = FakeCommonTypeBuilder.Create("FrameworkElement")
				.AddField(patchingFieldName, DependencyPropertyType.Type, new ConnectFieldToPropertyAttribute(patchingPropertyName))
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet)
				.Build();

			SetStaticField(frameworkElementType, patchingFieldName);

			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.All, (patchingPropertyName, patchingFieldName));
			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.Selectively, (patchingPropertyName, patchingFieldName));
		}

		[Test]
		public void PatchingPropertyCanNotHaveNotPatchingPropertyAttributeSpecifiedInConnectFieldToPropertyAttribute_InvalidFrameworkElement() {
			const string patchingFieldName = "AnyValueProperty";
			const string patchingPropertyName = "AnyValue";

			var frameworkElementType = FakeCommonTypeBuilder.Create("FrameworkElement")
				.AddField(patchingFieldName, DependencyPropertyType.Type, new ConnectFieldToPropertyAttribute(patchingPropertyName))
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new NotPatchingPropertyAttribute())
				.Build();

			SetStaticField(frameworkElementType, patchingFieldName);

			CheckInvalidFrameworkElement(frameworkElementType,
				FrameworkElementPatchingType.All,
				$"Patching property '{patchingPropertyName}' can not have '{nameof(NotPatchingPropertyAttribute)}', connection in '{nameof(ConnectFieldToPropertyAttribute)}' at field '{patchingFieldName}'");

			CheckInvalidFrameworkElement(frameworkElementType,
				FrameworkElementPatchingType.Selectively,
				$"Patching property '{patchingPropertyName}' can not have '{nameof(NotPatchingPropertyAttribute)}', connection in '{nameof(ConnectFieldToPropertyAttribute)}' at field '{patchingFieldName}'");
		}

		[Test]
		public void PatchingPropertyCanNotHaveNotPatchingPropertyAttributeSpecifiedInConnectFieldToPropertyAttribute_ValidFrameworkElement() {
			const string patchingFieldName = "AnyValueProperty";
			const string patchingPropertyName = "AnyValue";

			var frameworkElementType = FakeCommonTypeBuilder.Create("FrameworkElement")
				.AddField(patchingFieldName, DependencyPropertyType.Type, new ConnectFieldToPropertyAttribute(patchingPropertyName))
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet)
				.Build();

			SetStaticField(frameworkElementType, patchingFieldName);

			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.All, (patchingPropertyName, patchingFieldName));
			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.Selectively, (patchingPropertyName, patchingFieldName));
		}

		[Test]
		public void PatchingDependencyFieldCanNotHaveSuchTypeSpecifiedInConnectFieldToPropertyAttribute_InvalidFrameworkElement() {
			const string patchingFieldName = "AnyValueProperty";
			const string patchingPropertyName = "AnyValue";

			var frameworkElementType = FakeCommonTypeBuilder.Create("FrameworkElement")
				.AddField(patchingFieldName, typeof(int), new ConnectFieldToPropertyAttribute(patchingPropertyName))
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet)
				.Build();

			SetStaticField(frameworkElementType, patchingFieldName);

			CheckInvalidFrameworkElement(frameworkElementType,
				FrameworkElementPatchingType.All,
				$"Patching field '{patchingFieldName}' can not have '{typeof(int).FullName}' type, allowable types: '{DependencyPropertyType.FullName}', connection in '{nameof(ConnectFieldToPropertyAttribute)}' at field '{patchingFieldName}'");

			CheckInvalidFrameworkElement(frameworkElementType,
				FrameworkElementPatchingType.Selectively,
				$"Patching field '{patchingFieldName}' can not have '{typeof(int).FullName}' type, allowable types: '{DependencyPropertyType.FullName}', connection in '{nameof(ConnectFieldToPropertyAttribute)}' at field '{patchingFieldName}'");
		}

		[Test]
		public void PatchingDependencyFieldCanNotHaveSuchTypeSpecifiedInConnectFieldToPropertyAttribute_ValidFrameworkElement() {
			const string patchingFieldName = "AnyValueProperty";
			const string patchingPropertyName = "AnyValue";

			var frameworkElementType = FakeCommonTypeBuilder.Create("FrameworkElement")
				.AddField(patchingFieldName, DependencyPropertyType.Type, new ConnectFieldToPropertyAttribute(patchingPropertyName))
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet)
				.Build();

			SetStaticField(frameworkElementType, patchingFieldName);

			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.All, (patchingPropertyName, patchingFieldName));
			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.Selectively, (patchingPropertyName, patchingFieldName));
		}
	}
}