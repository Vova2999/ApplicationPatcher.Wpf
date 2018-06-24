using System.Linq;
using ApplicationPatcher.Tests;
using ApplicationPatcher.Tests.FakeTypes;
using ApplicationPatcher.Wpf.Configurations;
using ApplicationPatcher.Wpf.Services.NameRules;
using ApplicationPatcher.Wpf.Services.PropertyGrouper;
using ApplicationPatcher.Wpf.Types.Attributes;
using ApplicationPatcher.Wpf.Types.Enums;
using NUnit.Framework;

namespace ApplicationPatcher.Wpf.Tests.Services.PropertyGrouper {
	[TestFixture]
	public class PropertyGrouperServiceTests {
		[Test]
		public void A() {
			var applicationPatcherWpfConfiguration = new ApplicationPatcherWpfConfiguration {
				FieldNameRules = new Configurations.NameRules { Type = NameRulesType.lowerCamelCase },
				PropertyNameRules = new Configurations.NameRules { Type = NameRulesType.UpperCamelCase },
				CommandFieldNameRules = new Configurations.NameRules { Suffix = "Command", Type = NameRulesType.lowerCamelCase },
				CommandPropertyNameRules = new Configurations.NameRules { Suffix = "Command", Type = NameRulesType.UpperCamelCase },
				CommandExecuteMethodNameRules = new Configurations.NameRules { Prefix = "Execute", Suffix = "Method", Type = NameRulesType.UpperCamelCase },
				CommandCanExecuteMethodNameRules = new Configurations.NameRules { Prefix = "CanExecute", Suffix = "Method", Type = NameRulesType.UpperCamelCase }
			};

			var propertyGrouperService = new PropertyGrouperService(applicationPatcherWpfConfiguration, new NameRulesService(applicationPatcherWpfConfiguration));

			var assembly = FakeCommonAssemblyBuilder.Create();
			var commandType = FakeCommonTypeBuilder.Create(KnownTypeNames.ICommand).Build();
			var viewModelBaseType = FakeCommonTypeBuilder.Create(KnownTypeNames.ViewModelBase).Build();
			var viewModelType = FakeCommonTypeBuilder.Create("MainViewModel", viewModelBaseType)
				.AddProperty("AnyProperty", typeof(int), PropertyMethods.HasGetAndSet, new NotUseSearchByName())
				.AddProperty("AnyProperty1", typeof(int), PropertyMethods.HasGetAndSet, new PatchingPropertyAttribute())
				.AddProperty("AnyProperty2", typeof(int), PropertyMethods.HasGetAndSet, new ConnectPropertyToFieldAttribute("anyProperty2aaa"))
				.AddProperty("AnyProperty3", typeof(int), PropertyMethods.HasGetAndSet)
				.AddProperty("AnyProperty4", typeof(int), PropertyMethods.HasGetAndSet)
				.AddField("anyProperty", typeof(int))
				//.AddField("anyPropertyX", typeof(int), new ConnectFieldToPropertyAttribute("AnyProperty"))
				.AddField("anyProperty1", typeof(int))
				.AddField("anyProperty2aaa", typeof(int))
				.AddField("anyProperty3aaa", typeof(int), new ConnectFieldToPropertyAttribute("AnyProperty3"))
				.AddField("anyProperty4", typeof(int))
				.Build();

			assembly.AddCommonType(commandType, false);
			assembly.AddCommonType(viewModelBaseType, false);
			assembly.AddCommonType(viewModelType);

			var propertyGroups = propertyGrouperService.GetGroups(assembly.CommonAssembly, viewModelType, ViewModelPatchingType.All);
			var dictionary = propertyGroups.ToDictionary(group => group.Property.Name, group => group.Field.Name);
		}
	}
}