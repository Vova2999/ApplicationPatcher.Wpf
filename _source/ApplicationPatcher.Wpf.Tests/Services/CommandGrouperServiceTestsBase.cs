using System;
using System.Linq;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Core.Types.CommonMembers;
using ApplicationPatcher.Tests;
using ApplicationPatcher.Wpf.Configurations;
using ApplicationPatcher.Wpf.Exceptions;
using ApplicationPatcher.Wpf.Services.CommandGrouper;
using ApplicationPatcher.Wpf.Tests.Helpers;
using ApplicationPatcher.Wpf.Types.Enums;
using FluentAssertions;
using NUnit.Framework;

namespace ApplicationPatcher.Wpf.Tests.Services {
	[TestFixture]
	public abstract class CommandGrouperServiceTestsBase {
		private ApplicationPatcherWpfConfiguration applicationPatcherWpfConfiguration;
		private FakeCommonAssemblyBuilder fakeCommonAssemblyBuilder;
		private CommandGrouperService commandGrouperService;

		protected CommonType CommandType;
		protected CommonType RelayCommand;

		[OneTimeSetUp]
		public void OneTimeSetUp() {
			applicationPatcherWpfConfiguration = new ApplicationPatcherWpfConfiguration {
				CommandFieldNameRules = new Configurations.NameRules { Suffix = "Command", Type = NameRulesType.lowerCamelCase },
				CommandPropertyNameRules = new Configurations.NameRules { Suffix = "Command", Type = NameRulesType.UpperCamelCase },
				CommandExecuteMethodNameRules = new Configurations.NameRules { Prefix = "Execute", Suffix = "Method", Type = NameRulesType.UpperCamelCase },
				CommandCanExecuteMethodNameRules = new Configurations.NameRules { Prefix = "CanExecute", Suffix = "Method", Type = NameRulesType.UpperCamelCase }
			};

			var nameRulesService = NameRulesServiceHelper.CreateService(applicationPatcherWpfConfiguration);
			commandGrouperService = new CommandGrouperService(applicationPatcherWpfConfiguration, nameRulesService);

			CommandType = FakeCommonTypeBuilder.Create(KnownTypeNames.ICommand).Build();
			RelayCommand = FakeCommonTypeBuilder.Create(KnownTypeNames.RelayCommand, CommandType).Build();
			fakeCommonAssemblyBuilder = FakeCommonAssemblyBuilder.Create().AddCommonType(CommandType, false).AddCommonType(RelayCommand, false);
			FakeCommonTypeBuilder.ClearCreatedTypes();
		}

		[TearDown]
		public void ClearCreatedCommonTypes() {
			FakeCommonTypeBuilder.ClearCreatedTypes();
		}

		protected void CheckValidViewModel(CommonType viewModelType,
										   ViewModelPatchingType viewModelPatchingType,
										   bool skipConnectingByNameIfNameIsInvalid,
										   bool connectByNameIfExsistConnectAttribute,
										   params (string ExecuteMethodName, string CanExecuteMethodName, string PropertyName, string FieldName)[] expectedGroups) {
			applicationPatcherWpfConfiguration.SkipConnectingByNameIfNameIsInvalid = skipConnectingByNameIfNameIsInvalid;
			applicationPatcherWpfConfiguration.ConnectByNameIfExsistConnectAttribute = connectByNameIfExsistConnectAttribute;
			var groups = commandGrouperService.GetGroups(fakeCommonAssemblyBuilder.CommonAssembly, viewModelType, viewModelPatchingType);

			if (groups.Any())
				Console.WriteLine(groups.Select((group, i) => $"{i}) " +
						$"ExecuteMethod: {group.CommandExecuteMethod?.Name ?? "null"}, " +
						$"CanExecuteMethod: {group.CommandCanExecuteMethod?.Name ?? "null"}, " +
						$"Property: {group.CommandProperty?.Name ?? "null"}, " +
						$"Field: {group.CommandField?.Name ?? "null"}")
					.JoinToString("\n"));
			else
				Console.WriteLine("Groups missing");

			Console.WriteLine();

			groups.Length.Should().Be(expectedGroups.Length);
			for (var i = 0; i < groups.Length; i++) {
				CheckNames(groups[i].CommandField?.Name, expectedGroups[i].FieldName);
				CheckNames(groups[i].CommandProperty?.Name, expectedGroups[i].PropertyName);
				CheckNames(groups[i].CommandExecuteMethod?.Name, expectedGroups[i].ExecuteMethodName);
				CheckNames(groups[i].CommandCanExecuteMethod?.Name, expectedGroups[i].CanExecuteMethodName);
			}
		}

		private static void CheckNames(string actualName, string expectedName) {
			if (expectedName.IsNullOrEmpty())
				actualName.NullIfEmpty().Should().BeNull();
			else
				actualName.Should().Be(expectedName);
		}

		protected void CheckInvalidViewModel(CommonType viewModelType,
											 ViewModelPatchingType viewModelPatchingType,
											 string errorMessage,
											 bool skipConnectingByNameIfNameIsInvalid = false,
											 bool connectByNameIfExsistConnectAttribute = false) {
			applicationPatcherWpfConfiguration.SkipConnectingByNameIfNameIsInvalid = skipConnectingByNameIfNameIsInvalid;
			applicationPatcherWpfConfiguration.ConnectByNameIfExsistConnectAttribute = connectByNameIfExsistConnectAttribute;

			try {
				commandGrouperService.GetGroups(fakeCommonAssemblyBuilder.CommonAssembly, viewModelType, viewModelPatchingType);
				Assert.Fail($"Expected a '{nameof(ViewModelCommandPatchingException)}' to be thrown, but no exception was thrown");
			}
			catch (ViewModelCommandPatchingException exception) {
				Console.WriteLine(exception.Message);
				Console.WriteLine();

				exception.Message.Should().Contain(errorMessage);
				exception.Message.Should().NotContain("  2) ");
			}
		}
	}
}