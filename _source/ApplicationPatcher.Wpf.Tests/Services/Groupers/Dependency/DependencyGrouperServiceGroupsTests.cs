using ApplicationPatcher.Tests;
using ApplicationPatcher.Tests.FakeTypes;
using ApplicationPatcher.Wpf.Types.Attributes.Connect;
using ApplicationPatcher.Wpf.Types.Enums;
using NUnit.Framework;

namespace ApplicationPatcher.Wpf.Tests.Services.Groupers.Dependency {
	[TestFixture]
	public class DependencyGrouperServiceGroupsTests : DependencyGrouperServiceTestsBase {
		[Test]
		public void MultiConnectPropertyToDependencyFieldFound_InvalidFrameworkElement() {
			const string patchingFieldName = "AnyValueProperty";
			const string patchingSecondFieldName = "AnySecondValueProperty";
			const string patchingPropertyName = "AnyValue";

			var firstFrameworkElementType = FakeCommonTypeBuilder.Create("FirstFrameworkElement")
				.AddField(patchingFieldName, DependencyPropertyType.Type)
				.AddField(patchingSecondFieldName, DependencyPropertyType.Type, new ConnectFieldToPropertyAttribute(patchingPropertyName))
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet)
				.Build();

			var secondFrameworkElementType = FakeCommonTypeBuilder.Create("SecondFrameworkElement")
				.AddField(patchingFieldName, DependencyPropertyType.Type, new ConnectFieldToPropertyAttribute(patchingPropertyName))
				.AddField(patchingSecondFieldName, DependencyPropertyType.Type, new ConnectFieldToPropertyAttribute(patchingPropertyName))
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet)
				.Build();

			var thirdFrameworkElementType = FakeCommonTypeBuilder.Create("ThirdFrameworkElement")
				.AddField(patchingFieldName, DependencyPropertyType.Type, new ConnectFieldToPropertyAttribute(patchingPropertyName))
				.AddField(patchingSecondFieldName, DependencyPropertyType.Type)
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new ConnectPropertyToFieldAttribute(patchingSecondFieldName))
				.Build();

			SetStaticField(firstFrameworkElementType, patchingFieldName);
			SetStaticField(firstFrameworkElementType, patchingSecondFieldName);
			SetStaticField(secondFrameworkElementType, patchingFieldName);
			SetStaticField(secondFrameworkElementType, patchingSecondFieldName);
			SetStaticField(thirdFrameworkElementType, patchingFieldName);
			SetStaticField(thirdFrameworkElementType, patchingSecondFieldName);

			CheckInvalidFrameworkElement(firstFrameworkElementType,
				FrameworkElementPatchingType.All,
				$"Multi-connect property to field found: property '{patchingPropertyName}', fields: '{patchingFieldName}', '{patchingSecondFieldName}'",
				false,
				true);

			CheckValidFrameworkElement(firstFrameworkElementType, FrameworkElementPatchingType.Selectively, false, true, (patchingPropertyName, patchingSecondFieldName));

			CheckInvalidFrameworkElement(secondFrameworkElementType,
				FrameworkElementPatchingType.All,
				$"Multi-connect property to field found: property '{patchingPropertyName}', fields: '{patchingFieldName}', '{patchingSecondFieldName}'");

			CheckInvalidFrameworkElement(secondFrameworkElementType,
				FrameworkElementPatchingType.Selectively,
				$"Multi-connect property to field found: property '{patchingPropertyName}', fields: '{patchingFieldName}', '{patchingSecondFieldName}'");

			CheckInvalidFrameworkElement(thirdFrameworkElementType,
				FrameworkElementPatchingType.All,
				$"Multi-connect property to field found: property '{patchingPropertyName}', fields: '{patchingSecondFieldName}', '{patchingFieldName}'");

			CheckInvalidFrameworkElement(thirdFrameworkElementType,
				FrameworkElementPatchingType.Selectively,
				$"Multi-connect property to field found: property '{patchingPropertyName}', fields: '{patchingSecondFieldName}', '{patchingFieldName}'");
		}

		[Test]
		public void MultiConnectPropertyToDependencyFieldFound_ValidFrameworkElement() {
			const string patchingFieldName = "AnyValueProperty";
			const string patchingPropertyName = "AnyValue";

			var frameworkElementType = FakeCommonTypeBuilder.Create("FrameworkElement")
				.AddField(patchingFieldName, DependencyPropertyType.Type)
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet)
				.Build();

			SetStaticField(frameworkElementType, patchingFieldName);

			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.All, false, false, (patchingPropertyName, patchingFieldName));
			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.Selectively, false, false);
		}

		[Test]
		public void PatchingDependencyFieldCanNotBeNonStatic_InvalidFrameworkElement() {
			const string patchingFieldName = "AnyValueProperty";
			const string patchingPropertyName = "AnyValue";

			var frameworkElementType = FakeCommonTypeBuilder.Create("FrameworkElement")
				.AddField(patchingFieldName, DependencyPropertyType.Type)
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet)
				.Build();

			CheckInvalidFrameworkElement(frameworkElementType,
				FrameworkElementPatchingType.All,
				$"Patching field '{patchingFieldName}' can not be non static");

			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.Selectively, false, false);
		}

		[Test]
		public void PatchingDependencyFieldCanNotBeNonStatic_ValidFrameworkElement() {
			const string patchingFieldName = "AnyValueProperty";
			const string patchingPropertyName = "AnyValue";

			var frameworkElementType = FakeCommonTypeBuilder.Create("FrameworkElement")
				.AddField(patchingFieldName, DependencyPropertyType.Type)
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet)
				.Build();

			SetStaticField(frameworkElementType, patchingFieldName);

			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.All, false, false, (patchingPropertyName, patchingFieldName));
			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.Selectively, false, false);
		}

		[Test]
		public void PatchingDependencyFieldCanNotHaveSuchType_InvalidFrameworkElement() {
			const string patchingFieldName = "AnyValueProperty";
			const string patchingPropertyName = "AnyValue";

			var frameworkElementType = FakeCommonTypeBuilder.Create("FrameworkElement")
				.AddField(patchingFieldName, typeof(int))
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet)
				.Build();

			SetStaticField(frameworkElementType, patchingFieldName);

			CheckInvalidFrameworkElement(frameworkElementType,
				FrameworkElementPatchingType.All,
				$"Patching field '{patchingFieldName}' can not have '{typeof(int).FullName}' type, allowable types: '{DependencyPropertyType.FullName}'");

			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.Selectively, false, false);
		}

		[Test]
		public void PatchingDependencyFieldCanNotHaveSuchType_ValidFrameworkElement() {
			const string patchingFieldName = "AnyValueProperty";
			const string patchingPropertyName = "AnyValue";

			var frameworkElementType = FakeCommonTypeBuilder.Create("FrameworkElement")
				.AddField(patchingFieldName, DependencyPropertyType.Type)
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet)
				.Build();

			SetStaticField(frameworkElementType, patchingFieldName);

			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.All, false, false, (patchingPropertyName, patchingFieldName));
			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.Selectively, false, false);
		}

		[Test]
		public void NotFoundDependencyFieldWhenUsingNotUseSearchByNameAttribute_InvalidFrameworkElement() {
			const string patchingPropertyName = "AnyValue";

			var frameworkElementType = FakeCommonTypeBuilder.Create("FrameworkElement")
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new NotUseSearchByNameAttribute())
				.Build();

			CheckInvalidFrameworkElement(frameworkElementType,
				FrameworkElementPatchingType.All,
				$"Not found field for property '{patchingPropertyName}' when using '{nameof(NotUseSearchByNameAttribute)}'");

			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.Selectively, false, false);
		}

		[Test]
		public void NotFoundDependencyFieldWhenUsingNotUseSearchByNameAttribute_ValidFrameworkElement() {
			const string patchingFieldName = "AnyValueProperty";
			const string patchingPropertyName = "AnyValue";

			var frameworkElementType = FakeCommonTypeBuilder.Create("FrameworkElement")
				.AddField(patchingFieldName, DependencyPropertyType.Type, new ConnectFieldToPropertyAttribute(patchingPropertyName))
				.AddProperty(patchingPropertyName, typeof(int), PropertyMethods.HasGetAndSet, new NotUseSearchByNameAttribute())
				.Build();

			SetStaticField(frameworkElementType, patchingFieldName);

			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.All, false, false, (patchingPropertyName, patchingFieldName));
			CheckValidFrameworkElement(frameworkElementType, FrameworkElementPatchingType.Selectively, false, false, (patchingPropertyName, patchingFieldName));
		}
	}
}