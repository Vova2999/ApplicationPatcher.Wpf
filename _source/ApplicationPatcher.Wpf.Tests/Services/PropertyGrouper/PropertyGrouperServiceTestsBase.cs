using System;
using System.Linq;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Core.Types.CommonMembers;
using ApplicationPatcher.Tests;
using ApplicationPatcher.Wpf.Configurations;
using ApplicationPatcher.Wpf.Exceptions;
using ApplicationPatcher.Wpf.Services.NameRules;
using ApplicationPatcher.Wpf.Services.NameRules.Specific;
using ApplicationPatcher.Wpf.Services.PropertyGrouper;
using ApplicationPatcher.Wpf.Types.Enums;
using FluentAssertions;
using NUnit.Framework;

namespace ApplicationPatcher.Wpf.Tests.Services.PropertyGrouper {
	[TestFixture]
	public abstract class PropertyGrouperServiceTestsBase {
		private ApplicationPatcherWpfConfiguration applicationPatcherWpfConfiguration;
		private FakeCommonAssemblyBuilder fakeCommonAssemblyBuilder;
		private PropertyGrouperService propertyGrouperService;

		protected CommonType CommandType;

		[OneTimeSetUp]
		public void OneTimeSetUp() {
			applicationPatcherWpfConfiguration = new ApplicationPatcherWpfConfiguration {
				FieldNameRules = new Configurations.NameRules { Type = NameRulesType.lowerCamelCase },
				PropertyNameRules = new Configurations.NameRules { Type = NameRulesType.UpperCamelCase }
			};

			var specificNameRulesServices =
				new SpecificNameRulesService[] {
					new AllLowerNameRules(),
					new AllUpperNameRules(),
					new FirstUpperNameRules(),
					new LowerCamelCaseNameRules(),
					new UpperCamelCaseNameRules()
				};

			var nameRulesService = new NameRulesService(applicationPatcherWpfConfiguration, specificNameRulesServices);
			propertyGrouperService = new PropertyGrouperService(applicationPatcherWpfConfiguration, nameRulesService);

			CommandType = FakeCommonTypeBuilder.Create(KnownTypeNames.ICommand).Build();
			fakeCommonAssemblyBuilder = FakeCommonAssemblyBuilder.Create().AddCommonType(CommandType, false);
			FakeCommonTypeBuilder.ClearCreatedTypes();
		}

		[TearDown]
		public void ClearCreatedCommonTypes() {
			FakeCommonTypeBuilder.ClearCreatedTypes();
		}

		protected void CheckValidViewModel(CommonType viewModelType, ViewModelPatchingType viewModelPatchingType, bool connectByNameIfExsistConnectAttribute = false) {
			applicationPatcherWpfConfiguration.ConnectByNameIfExsistConnectAttribute = connectByNameIfExsistConnectAttribute;
			var groups = propertyGrouperService.GetGroups(fakeCommonAssemblyBuilder.CommonAssembly, viewModelType, viewModelPatchingType);
			Console.WriteLine("Groups missing");

			groups.Should().HaveCount(0);
		}

		protected void CheckValidViewModel(CommonType viewModelType, ViewModelPatchingType viewModelPatchingType, string fieldName, string propertyName, bool connectByNameIfExsistConnectAttribute = false) {
			applicationPatcherWpfConfiguration.ConnectByNameIfExsistConnectAttribute = connectByNameIfExsistConnectAttribute;
			var groups = propertyGrouperService.GetGroups(fakeCommonAssemblyBuilder.CommonAssembly, viewModelType, viewModelPatchingType);
			Console.WriteLine(groups.Select((group, i) => $"{i}) Property: {group.Property?.Name ?? "null"}, Field: {group.Field?.Name ?? "null"}").JoinToString("\n"));

			groups.Should().HaveCount(1);
			groups.First().Field?.Name.Should().Be(fieldName);
			groups.First().Property?.Name.Should().Be(propertyName);
		}

		protected void CheckInvalidViewModel(CommonType viewModelType, ViewModelPatchingType viewModelPatchingType, string errorMessage, bool connectByNameIfExsistConnectAttribute = false) {
			applicationPatcherWpfConfiguration.ConnectByNameIfExsistConnectAttribute = connectByNameIfExsistConnectAttribute;
			try {
				propertyGrouperService.GetGroups(fakeCommonAssemblyBuilder.CommonAssembly, viewModelType, viewModelPatchingType);
				Assert.Fail($"Expected a '{nameof(PropertyPatchingException)}' to be thrown, but no exception was thrown");
			}
			catch (PropertyPatchingException exception) {
				Console.WriteLine(exception.Message);
				exception.Message.Should().Contain(errorMessage);
			}
		}
	}
}