using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Wpf.Configurations;
using ApplicationPatcher.Wpf.Services.NameRules;
using ApplicationPatcher.Wpf.Tests.Helpers;
using FluentAssertions;
using NUnit.Framework;

namespace ApplicationPatcher.Wpf.Tests.Services.NameRules {
	[TestFixture]
	[SuppressMessage("ReSharper", "StringLiteralTypo")]
	public class NameRulesServiceTests {
		private NameRulesService nameRulesService;

		[OneTimeSetUp]
		public void OneTimeSetUp() {
			var applicationPatcherWpfConfiguration = new ApplicationPatcherWpfConfiguration {
				FieldNameRules = new Configurations.NameRules { Type = NameRulesType.lowerCamelCase },
				PropertyNameRules = new Configurations.NameRules { Type = NameRulesType.UpperCamelCase },
				CommandFieldNameRules = new Configurations.NameRules { Suffix = "Command", Type = NameRulesType.lowerCamelCase },
				CommandPropertyNameRules = new Configurations.NameRules { Suffix = "Command", Type = NameRulesType.UpperCamelCase },
				DependencyFieldNameRules = new Configurations.NameRules { Suffix = "Property", Type = NameRulesType.UpperCamelCase },
				DependencyPropertyNameRules = new Configurations.NameRules { Type = NameRulesType.UpperCamelCase },
				CommandExecuteMethodNameRules = new Configurations.NameRules { Prefix = "Execute", Suffix = "Method", Type = NameRulesType.UpperCamelCase },
				CommandCanExecuteMethodNameRules = new Configurations.NameRules { Prefix = "CanExecute", Suffix = "Method", Type = NameRulesType.UpperCamelCase }
			};

			nameRulesService = NameRulesServiceHelper.CreateService(applicationPatcherWpfConfiguration);
		}


		[Test]
		public void IsNameValid_InvalidNames_FieldName() {
			var invalidNames = new[] { "AnyAction", "_any2Action", "anYACtIon", "a11nYaCtIo11NN" };
			CheckInvalidNames(invalidNames, UseNameRulesFor.Field);
		}

		[Test]
		public void IsNameValid_InvalidNames_PropertyName() {
			var invalidNames = new[] { "anyAction", "Any2AAction", "AnYACtIon", "A11nYaCtIo11NN" };
			CheckInvalidNames(invalidNames, UseNameRulesFor.Property);
		}

		[Test]
		public void IsNameValid_InvalidNames_CommandFieldName() {
			var invalidNames = new[] { "AnyActionCommand", "any2ActiOOonCommand", "anYaCtIonCoand", "a11nYaCtIo11N" };
			CheckInvalidNames(invalidNames, UseNameRulesFor.CommandField);
		}

		[Test]
		public void IsNameValid_InvalidNames_CommandPropertyName() {
			var invalidNames = new[] { "anyActionCommand", "Any2AcCCtionCommand", "AnYaCtIonComand", "A11nYaCtIo11N" };
			CheckInvalidNames(invalidNames, UseNameRulesFor.CommandProperty);
		}

		[Test]
		public void IsNameValid_InvalidNames_DependencyFieldName() {
			var invalidNames = new[] { "anyActionProperty", "anyActiOOonProperty", "anyACctionProperty", "a11nYaCtIo11N" };
			CheckInvalidNames(invalidNames, UseNameRulesFor.DependencyField);
		}

		[Test]
		public void IsNameValid_InvalidNames_DependencyPropertyName() {
			var invalidNames = new[] { "anyActionProperty", "Any2AcCCtionProperty", "AnYaCtIonProertyYY", "A11nYaCtIo11NN" };
			CheckInvalidNames(invalidNames, UseNameRulesFor.DependencyProperty);
		}

		[Test]
		public void IsNameValid_InvalidNames_CommandExecuteMethodName() {
			var invalidNames = new[] { "AnyActionMethod", "ExecuteAny2Action", "EeexecuteAnYaCtIonMethod", "ExecuteA11nYaCtIo11NNnMethod" };
			CheckInvalidNames(invalidNames, UseNameRulesFor.CommandExecuteMethod);
		}

		[Test]
		public void IsNameValid_InvalidNames_CommandCanExecuteMethodName() {
			var invalidNames = new[] { "ExecuteAnyActionMethod", "CanAny2ActionMethod", "CanExecuteAnYaCtIon", "CanExecuteAA11nYaCtIo11nMethod" };
			CheckInvalidNames(invalidNames, UseNameRulesFor.CommandCanExecuteMethod);
		}


		[Test]
		public void IsNameValid_ValidNames_FieldName() {
			var validNames = new[] { "anyAction", "any2Action", "anYaCtIon", "a11nYaCtIo11N" };
			CheckValidNames(validNames, UseNameRulesFor.Field);
		}

		[Test]
		public void IsNameValid_ValidNames_PropertyName() {
			var validNames = new[] { "AnyAction", "Any2Action", "AnYaCtIon", "A11nYaCtIo11N" };
			CheckValidNames(validNames, UseNameRulesFor.Property);
		}

		[Test]
		public void IsNameValid_ValidNames_CommandFieldName() {
			var validNames = new[] { "anyActionCommand", "any2ActionCommand", "anYaCtIonCommand", "a11nYaCtIo11NCommand" };
			CheckValidNames(validNames, UseNameRulesFor.CommandField);
		}

		[Test]
		public void IsNameValid_ValidNames_CommandPropertyName() {
			var validNames = new[] { "AnyActionCommand", "Any2ActionCommand", "AnYaCtIonCommand", "A11nYaCtIo11NCommand" };
			CheckValidNames(validNames, UseNameRulesFor.CommandProperty);
		}

		[Test]
		public void IsNameValid_ValidNames_DependencyFieldName() {
			var validNames = new[] { "AnyActionProperty", "Any2ActionProperty", "AnYaCtIonProperty", "A11nYaCtIo11NProperty" };
			CheckValidNames(validNames, UseNameRulesFor.DependencyField);
		}

		[Test]
		public void IsNameValid_ValidNames_DependencyPropertyName() {
			var validNames = new[] { "AnyActionProperty", "Any2ActionCommand", "AnYaCtIonProperty", "A11nYaCtIo11nCommand" };
			CheckValidNames(validNames, UseNameRulesFor.DependencyProperty);
		}

		[Test]
		public void IsNameValid_ValidNames_CommandExecuteMethodName() {
			var validNames = new[] { "ExecuteAnyActionMethod", "ExecuteAny2ActionMethod", "ExecuteAnYaCtIonMethod", "ExecuteA11nYaCtIo11nMethod" };
			CheckValidNames(validNames, UseNameRulesFor.CommandExecuteMethod);
		}

		[Test]
		public void IsNameValid_ValidNames_CommandCanExecuteMethodName() {
			var validNames = new[] { "CanExecuteAnyActionMethod", "CanExecuteAny2ActionMethod", "CanExecuteAnYaCtIonMethod", "CanExecuteA11nYaCtIo11nMethod" };
			CheckValidNames(validNames, UseNameRulesFor.CommandCanExecuteMethod);
		}


		[Test]
		public void ConvertName_FieldName_To_PropertyName() {
			var names = new[] { "anyAction", "any2Action", "anYaCtIon", "a11nYaCtIo11N" };
			var expectedNames = new[] { "AnyAction", "Any2Action", "AnYaCtIon", "A11nYaCtIo11N" };
			CheckConvertNames(names, UseNameRulesFor.Field, UseNameRulesFor.Property, expectedNames);
		}

		[Test]
		public void ConvertName_PropertyName_To_FieldName() {
			var names = new[] { "AnyAction", "Any2Action", "AnYaCtIon", "A11nYaCtIo11N" };
			var expectedNames = new[] { "anyAction", "any2Action", "anYaCtIon", "a11nYaCtIo11N" };
			CheckConvertNames(names, UseNameRulesFor.Property, UseNameRulesFor.Field, expectedNames);
		}

		[Test]
		public void ConvertName_DependencyFieldName_To_DependencyPropertyName() {
			var names = new[] { "AnyActionProperty", "Any2ActionProperty", "AnYaCtIonProperty", "A11nYaCtIo11NProperty" };
			var expectedNames = new[] { "AnyAction", "Any2Action", "AnYaCtIon", "A11nYaCtIo11N" };
			CheckConvertNames(names, UseNameRulesFor.DependencyField, UseNameRulesFor.DependencyProperty, expectedNames);
		}

		[Test]
		public void ConvertName_DependencyPropertyName_To_DependencyFieldName() {
			var names = new[] { "AnyAction", "Any2Action", "AnYaCtIon", "A11nYaCtIo11N" };
			var expectedNames = new[] { "AnyActionProperty", "Any2ActionProperty", "AnYaCtIonProperty", "A11nYaCtIo11NProperty" };
			CheckConvertNames(names, UseNameRulesFor.DependencyProperty, UseNameRulesFor.DependencyField, expectedNames);
		}

		[Test]
		public void ConvertName_CommandFieldName_To_CommandPropertyName() {
			var names = new[] { "anyActionCommand", "any2ActionCommand", "anYaCtIonCommand", "a11nYaCtIo11NCommand" };
			var expectedNames = new[] { "AnyActionCommand", "Any2ActionCommand", "AnYaCtIonCommand", "A11nYaCtIo11NCommand" };
			CheckConvertNames(names, UseNameRulesFor.CommandField, UseNameRulesFor.CommandProperty, expectedNames);
		}

		[Test]
		public void ConvertName_CommandPropertyName_To_CommandFieldName() {
			var names = new[] { "AnyActionCommand", "Any2ActionCommand", "AnYaCtIonCommand", "A11nYaCtIo11NCommand" };
			var expectedNames = new[] { "anyActionCommand", "any2ActionCommand", "anYaCtIonCommand", "a11nYaCtIo11NCommand" };
			CheckConvertNames(names, UseNameRulesFor.CommandProperty, UseNameRulesFor.CommandField, expectedNames);
		}

		[Test]
		public void ConvertName_CommandPropertyName_To_CommandExecuteMethodName() {
			var names = new[] { "AnyActionCommand", "Any2ActionCommand", "AnYaCtIonCommand", "A11nYaCtIo11NCommand" };
			var expectedNames = new[] { "ExecuteAnyActionMethod", "ExecuteAny2ActionMethod", "ExecuteAnYaCtIonMethod", "ExecuteA11nYaCtIo11NMethod" };
			CheckConvertNames(names, UseNameRulesFor.CommandProperty, UseNameRulesFor.CommandExecuteMethod, expectedNames);
		}

		[Test]
		public void ConvertName_CommandPropertyName_To_CommandCanExecuteMethodName() {
			var names = new[] { "AnyActionCommand", "Any2ActionCommand", "AnYaCtIonCommand", "A11nYaCtIo11NCommand" };
			var expectedNames = new[] { "CanExecuteAnyActionMethod", "CanExecuteAny2ActionMethod", "CanExecuteAnYaCtIonMethod", "CanExecuteA11nYaCtIo11NMethod" };
			CheckConvertNames(names, UseNameRulesFor.CommandProperty, UseNameRulesFor.CommandCanExecuteMethod, expectedNames);
		}

		[Test]
		public void ConvertName_CommandCanExecuteMethodName_To_CommandPropertyName() {
			var names = new[] { "CanExecuteAnyActionMethod", "CanExecuteAny2ActionMethod", "CanExecuteAnYaCtIonMethod", "CanExecuteA11nYaCtIo11nMethod" };
			var expectedNames = new[] { "AnyActionCommand", "Any2ActionCommand", "AnYaCtIonCommand", "A11nYaCtIo11nCommand" };
			CheckConvertNames(names, UseNameRulesFor.CommandCanExecuteMethod, UseNameRulesFor.CommandProperty, expectedNames);
		}

		[Test]
		public void ConvertName_CommandCanExecuteMethodName_To_CommandExecuteMethodName() {
			var names = new[] { "CanExecuteAnyActionMethod", "CanExecuteAny2ActionMethod", "CanExecuteAnYaCtIonMethod", "CanExecuteA11nYaCtIo11nMethod" };
			var expectedNames = new[] { "ExecuteAnyActionMethod", "ExecuteAny2ActionMethod", "ExecuteAnYaCtIonMethod", "ExecuteA11nYaCtIo11nMethod" };
			CheckConvertNames(names, UseNameRulesFor.CommandCanExecuteMethod, UseNameRulesFor.CommandExecuteMethod, expectedNames);
		}

		[Test]
		public void ConvertName_CommandExecuteMethodName_To_CommandPropertyName() {
			var names = new[] { "ExecuteAnyActionMethod", "ExecuteAny2ActionMethod", "ExecuteAnYaCtIonMethod", "ExecuteA11nYaCtIo11nMethod" };
			var expectedNames = new[] { "AnyActionCommand", "Any2ActionCommand", "AnYaCtIonCommand", "A11nYaCtIo11nCommand" };
			CheckConvertNames(names, UseNameRulesFor.CommandExecuteMethod, UseNameRulesFor.CommandProperty, expectedNames);
		}

		[Test]
		public void ConvertName_CommandExecuteMethodName_To_CommandCanExecuteMethodName() {
			var names = new[] { "ExecuteAnyActionMethod", "ExecuteAny2ActionMethod", "ExecuteAnYaCtIonMethod", "ExecuteA11nYaCtIo11nMethod" };
			var expectedNames = new[] { "CanExecuteAnyActionMethod", "CanExecuteAny2ActionMethod", "CanExecuteAnYaCtIonMethod", "CanExecuteA11nYaCtIo11nMethod" };
			CheckConvertNames(names, UseNameRulesFor.CommandExecuteMethod, UseNameRulesFor.CommandCanExecuteMethod, expectedNames);
		}


		private void CheckValidNames(IEnumerable<string> validNames, UseNameRulesFor useNameRulesFor) {
			validNames.ForEach(validName => nameRulesService.IsNameValid(validName, useNameRulesFor).Should().Be(true, $"Name '{validName}' is valid"));
		}

		private void CheckInvalidNames(IEnumerable<string> invalidNames, UseNameRulesFor useNameRulesFor) {
			invalidNames.ForEach(validName => nameRulesService.IsNameValid(validName, useNameRulesFor).Should().Be(false, $"Name '{validName}' is invalid"));
		}

		private void CheckConvertNames(IEnumerable<string> names, UseNameRulesFor from, UseNameRulesFor to, IEnumerable<string> expectedNames) {
			names.Select(name => nameRulesService.ConvertName(name, from, to)).SequenceEqual(expectedNames).Should().Be(true);
		}
	}
}