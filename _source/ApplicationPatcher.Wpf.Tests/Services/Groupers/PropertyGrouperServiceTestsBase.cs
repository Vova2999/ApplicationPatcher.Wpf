using System;
using System.Linq;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Core.Types.CommonInterfaces;
using ApplicationPatcher.Tests;
using ApplicationPatcher.Wpf.Configurations;
using ApplicationPatcher.Wpf.Exceptions;
using ApplicationPatcher.Wpf.Services.Groupers.Property;
using ApplicationPatcher.Wpf.Tests.Helpers;
using ApplicationPatcher.Wpf.Types.Enums;
using FluentAssertions;
using NUnit.Framework;

namespace ApplicationPatcher.Wpf.Tests.Services.Groupers {
	[TestFixture]
	public abstract class PropertyGrouperServiceTestsBase {
		private ApplicationPatcherWpfConfiguration applicationPatcherWpfConfiguration;
		private FakeCommonAssemblyBuilder fakeCommonAssemblyBuilder;
		private PropertyGrouperService propertyGrouperService;

		protected ICommonType CommandType;

		[OneTimeSetUp]
		public void OneTimeSetUp() {
			applicationPatcherWpfConfiguration = new ApplicationPatcherWpfConfiguration {
				FieldNameRules = new Configurations.NameRules { Type = NameRulesType.lowerCamelCase },
				PropertyNameRules = new Configurations.NameRules { Type = NameRulesType.UpperCamelCase }
			};

			var nameRulesService = NameRulesServiceHelper.CreateService(applicationPatcherWpfConfiguration);
			propertyGrouperService = new PropertyGrouperService(applicationPatcherWpfConfiguration, nameRulesService);

			CommandType = FakeCommonTypeBuilder.Create(KnownTypeNames.ICommand).Build();
			fakeCommonAssemblyBuilder = FakeCommonAssemblyBuilder.Create().AddCommonType(CommandType, false);
			FakeCommonTypeBuilder.ClearCreatedTypes();
		}

		[TearDown]
		public void ClearCreatedCommonTypes() {
			FakeCommonTypeBuilder.ClearCreatedTypes();
		}

		protected void CheckValidViewModel(ICommonType viewModelType, ViewModelPatchingType viewModelPatchingType, params (string PropertyName, string FieldName)[] expectedGroups) {
			CheckValidViewModel(viewModelType, viewModelPatchingType, false, false, expectedGroups);
		}

		protected void CheckValidViewModel(ICommonType viewModelType,
										   ViewModelPatchingType viewModelPatchingType,
										   bool skipConnectingByNameIfNameIsInvalid,
										   bool connectByNameIfExistConnectAttribute,
										   params (string PropertyName, string FieldName)[] expectedGroups) {
			applicationPatcherWpfConfiguration.SkipConnectingByNameIfNameIsInvalid = skipConnectingByNameIfNameIsInvalid;
			applicationPatcherWpfConfiguration.ConnectByNameIfExistConnectAttribute = connectByNameIfExistConnectAttribute;
			var groups = propertyGrouperService.GetGroups(fakeCommonAssemblyBuilder.CommonAssembly, viewModelType, viewModelPatchingType);

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

		protected void CheckInvalidViewModel(ICommonType viewModelType, ViewModelPatchingType viewModelPatchingType, string errorMessage) {
			CheckInvalidViewModel(viewModelType, viewModelPatchingType, errorMessage, false, false);
		}

		protected void CheckInvalidViewModel(ICommonType viewModelType,
											 ViewModelPatchingType viewModelPatchingType,
											 string errorMessage,
											 bool skipConnectingByNameIfNameIsInvalid,
											 bool connectByNameIfExistConnectAttribute) {
			applicationPatcherWpfConfiguration.SkipConnectingByNameIfNameIsInvalid = skipConnectingByNameIfNameIsInvalid;
			applicationPatcherWpfConfiguration.ConnectByNameIfExistConnectAttribute = connectByNameIfExistConnectAttribute;

			try {
				propertyGrouperService.GetGroups(fakeCommonAssemblyBuilder.CommonAssembly, viewModelType, viewModelPatchingType);
				Assert.Fail($"Expected a '{nameof(ViewModelPropertyPatchingException)}' to be thrown, but no exception was thrown");
			}
			catch (ViewModelPropertyPatchingException exception) {
				Console.WriteLine(exception.Message);
				Console.WriteLine();

				exception.Message.Should().Contain(errorMessage);
				exception.Message.Should().NotContain("  2) ");
			}
		}
	}
}