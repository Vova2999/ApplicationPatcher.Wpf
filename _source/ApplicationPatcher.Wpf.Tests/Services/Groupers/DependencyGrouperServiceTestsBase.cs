using System;
using System.Linq;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Core.Types.CommonInterfaces;
using ApplicationPatcher.Tests;
using ApplicationPatcher.Wpf.Configurations;
using ApplicationPatcher.Wpf.Exceptions;
using ApplicationPatcher.Wpf.Services.Groupers.Dependency;
using ApplicationPatcher.Wpf.Tests.Helpers;
using ApplicationPatcher.Wpf.Types.Enums;
using FluentAssertions;
using NUnit.Framework;

namespace ApplicationPatcher.Wpf.Tests.Services.Groupers {
	[TestFixture]
	public abstract class DependencyGrouperServiceTestsBase {
		private ApplicationPatcherWpfConfiguration applicationPatcherWpfConfiguration;
		private FakeCommonAssemblyBuilder fakeCommonAssemblyBuilder;
		private DependencyGrouperService dependencyGrouperService;

		protected ICommonType DependencyPropertyType;

		[OneTimeSetUp]
		public void OneTimeSetUp() {
			applicationPatcherWpfConfiguration = new ApplicationPatcherWpfConfiguration {
				DependencyFieldNameRules = new Configurations.NameRules { Suffix = "Property", Type = NameRulesType.UpperCamelCase },
				DependencyPropertyNameRules = new Configurations.NameRules { Type = NameRulesType.UpperCamelCase }
			};

			var nameRulesService = NameRulesServiceHelper.CreateService(applicationPatcherWpfConfiguration);
			dependencyGrouperService = new DependencyGrouperService(applicationPatcherWpfConfiguration, nameRulesService);

			DependencyPropertyType = FakeCommonTypeBuilder.Create(KnownTypeNames.DependencyProperty).Build();
			fakeCommonAssemblyBuilder = FakeCommonAssemblyBuilder.Create().AddCommonType(DependencyPropertyType, false);
			FakeCommonTypeBuilder.ClearCreatedTypes();
		}

		[TearDown]
		public void ClearCreatedCommonTypes() {
			FakeCommonTypeBuilder.ClearCreatedTypes();
		}

		protected static void SetStaticField(ICommonType frameworkElementType, string patchingFieldName) {
			FakeCommonTypeBuilder.GetMockFor(frameworkElementType.GetField(patchingFieldName, true).MonoCecil).Setup(field => field.IsStatic).Returns(true);
		}

		protected void CheckValidFrameworkElement(ICommonType frameworkElementType, FrameworkElementPatchingType frameworkElementPatchingType, params (string PropertyName, string FieldName)[] expectedGroups) {
			CheckValidFrameworkElement(frameworkElementType, frameworkElementPatchingType, false, false, expectedGroups);
		}

		protected void CheckValidFrameworkElement(ICommonType frameworkElementType,
												  FrameworkElementPatchingType frameworkElementPatchingType,
												  bool skipConnectingByNameIfNameIsInvalid,
												  bool connectByNameIfExistConnectAttribute,
												  params (string PropertyName, string FieldName)[] expectedGroups) {
			applicationPatcherWpfConfiguration.SkipConnectingByNameIfNameIsInvalid = skipConnectingByNameIfNameIsInvalid;
			applicationPatcherWpfConfiguration.ConnectByNameIfExistConnectAttribute = connectByNameIfExistConnectAttribute;
			var groups = dependencyGrouperService.GetGroups(fakeCommonAssemblyBuilder.CommonAssembly, frameworkElementType, frameworkElementPatchingType);

			if (groups.Any())
				Console.WriteLine(groups.Select((group, i) => $"{i}) " +
						$"Property: {group.Property?.Name ?? "null"}, " +
						$"Field: {group.Field?.Name ?? "null"}")
					.JoinToString("\n"));
			else
				Console.WriteLine("Groups missing");

			Console.WriteLine();

			groups.Length.Should().Be(expectedGroups.Length);
			for (var i = 0; i < groups.Length; i++) {
				CheckNames(groups[i].Field?.Name, expectedGroups[i].FieldName);
				CheckNames(groups[i].Property?.Name, expectedGroups[i].PropertyName);
			}
		}

		private static void CheckNames(string actualName, string expectedName) {
			if (expectedName.IsNullOrEmpty())
				actualName.NullIfEmpty().Should().BeNull();
			else
				actualName.Should().Be(expectedName);
		}

		protected void CheckInvalidFrameworkElement(ICommonType frameworkElementType, FrameworkElementPatchingType frameworkElementPatchingType, string errorMessage) {
			CheckInvalidFrameworkElement(frameworkElementType, frameworkElementPatchingType, errorMessage, false, false);
		}

		protected void CheckInvalidFrameworkElement(ICommonType frameworkElementType,
													FrameworkElementPatchingType frameworkElementPatchingType,
													string errorMessage,
													bool skipConnectingByNameIfNameIsInvalid,
													bool connectByNameIfExistConnectAttribute) {
			applicationPatcherWpfConfiguration.SkipConnectingByNameIfNameIsInvalid = skipConnectingByNameIfNameIsInvalid;
			applicationPatcherWpfConfiguration.ConnectByNameIfExistConnectAttribute = connectByNameIfExistConnectAttribute;

			try {
				dependencyGrouperService.GetGroups(fakeCommonAssemblyBuilder.CommonAssembly, frameworkElementType, frameworkElementPatchingType);
				Assert.Fail($"Expected a '{nameof(FrameworkElementDependencyPatchingException)}' to be thrown, but no exception was thrown");
			}
			catch (FrameworkElementDependencyPatchingException exception) {
				Console.WriteLine(exception.Message);
				Console.WriteLine();

				exception.Message.Should().Contain(errorMessage);
				exception.Message.Should().NotContain("  2) ");
			}
		}
	}
}