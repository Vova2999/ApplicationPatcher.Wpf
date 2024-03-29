﻿using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Core.Types.BaseInterfaces;
using ApplicationPatcher.Core.Types.CommonInterfaces;
using ApplicationPatcher.Wpf.Configurations;
using ApplicationPatcher.Wpf.Exceptions;
using ApplicationPatcher.Wpf.Extensions;
using ApplicationPatcher.Wpf.Services.NameRules;
using ApplicationPatcher.Wpf.Types.Attributes.Connect;
using ApplicationPatcher.Wpf.Types.Attributes.SelectPatching;
using ApplicationPatcher.Wpf.Types.Enums;

namespace ApplicationPatcher.Wpf.Services.Groupers.Command {
	public class CommandGrouperService {
		private readonly ApplicationPatcherWpfConfiguration applicationPatcherWpfConfiguration;
		private readonly NameRulesService nameRulesService;

		public CommandGrouperService(ApplicationPatcherWpfConfiguration applicationPatcherWpfConfiguration, NameRulesService nameRulesService) {
			this.applicationPatcherWpfConfiguration = applicationPatcherWpfConfiguration;
			this.nameRulesService = nameRulesService;
		}

		public CommandGroup[] GetGroups(ICommonAssembly assembly, ICommonType viewModelType, ViewModelPatchingType patchingType) {
			var commandType = assembly.GetCommonType(KnownTypeNames.ICommand, true);
			CheckViewModel(commandType, viewModelType);

			var patchingMethods = GetPatchingMethods(viewModelType, patchingType);
			CheckPatchingMethodNames(patchingMethods);

			var patchingCommandGroups = GetPatchingCommandGroups(patchingMethods, viewModelType, commandType);
			CheckPatchingCommandGroups(patchingCommandGroups);

			return patchingCommandGroups.Select(group => new CommandGroup(group.Fields.SingleOrDefault(), group.Properties.SingleOrDefault(), group.ExecuteMethod, group.CanExecuteMethods.SingleOrDefault())).ToArray();
		}

		private static void CheckViewModel(IHasType commandType, ICommonType viewModelType) {
			var errorsService = new ErrorsService();
			foreach (var method in viewModelType.Methods) {
				if (method.ContainsAttribute<PatchingCommandAttribute>()) {
					if (method.ParameterTypes.Length > 1)
						errorsService.AddError($"Patching method '{method.Name}' can not have more than one parameter");

					if (method.ContainsAttribute<NotPatchingCommandAttribute>())
						errorsService.AddError($"Patching method '{method.Name}' can not have " +
							$"'{nameof(PatchingCommandAttribute)}' and '{nameof(NotPatchingCommandAttribute)}' at the same time");

					if (method.ReturnType != typeof(void))
						errorsService.AddError($"Patching method '{method.Name}' can not have " +
							$"'{method.ReturnType.FullName}' return type, allowable types: '{typeof(void).FullName}'");
				}

				foreach (var attribute in method.GetCastedAttributes<ConnectMethodToMethodAttribute>()) {
					if (method.ParameterTypes.Length > 1)
						errorsService.AddError($"Patching method '{method.Name}' can not have more than one parameter");

					if (method.ContainsAttribute<NotPatchingCommandAttribute>())
						errorsService.AddError($"Patching method '{method.Name}' can not have '{nameof(NotPatchingCommandAttribute)}'");

					if (method.ReturnType != typeof(void) && method.ReturnType != typeof(bool))
						errorsService.AddError($"Patching method '{method.Name}' can not have " +
							$"'{method.ReturnType.FullName}' return type, allowable types: '{typeof(void).FullName}', '{typeof(bool).FullName}'");

					var methods = viewModelType.GetMethods(attribute.ConnectingMethodName).ToArray();
					switch (methods.Length) {
						case 0:
							errorsService.AddError($"Not found method with name '{attribute.ConnectingMethodName}', " +
								$"specified in '{nameof(ConnectMethodToMethodAttribute)}' at method '{method.Name}'");
							break;

						case 1:
							var singleMethod = methods.Single();

							if (singleMethod.ParameterTypes.Length > 1)
								errorsService.AddError($"Patching method '{singleMethod.Name}' can not have more than one parameter, " +
									$"connection in '{nameof(ConnectMethodToMethodAttribute)}' at method '{method.Name}'");

							if (singleMethod.ContainsAttribute<NotPatchingCommandAttribute>())
								errorsService.AddError($"Patching method '{singleMethod.Name}' can not have '{nameof(NotPatchingCommandAttribute)}', " +
									$"connection in '{nameof(ConnectMethodToMethodAttribute)}' at method '{method.Name}'");

							if (!method.ParameterTypes.Any() && singleMethod.ParameterTypes.Any())
								errorsService.AddError($"Patching method '{singleMethod.Name}' can not have parameter because method {method.Name} has no parameters, " +
									$"connection in '{nameof(ConnectMethodToMethodAttribute)}' at method '{method.Name}'");

							if (method.ParameterTypes.Any() && !singleMethod.ParameterTypes.Any())
								errorsService.AddError($"Patching method '{singleMethod.Name}' must have one parameter with type {method.ParameterTypes.First().FullName}, " +
									$"connection in '{nameof(ConnectMethodToMethodAttribute)}' at method '{method.Name}'");

							if (singleMethod.ReturnType != typeof(void) && singleMethod.ReturnType != typeof(bool)) {
								errorsService.AddError($"Patching method '{singleMethod.Name}' can not have " +
									$"'{singleMethod.ReturnType.FullName}' return type, allowable types: '{typeof(void).FullName}', '{typeof(bool).FullName}', " +
									$"connection in '{nameof(ConnectMethodToMethodAttribute)}' at method '{method.Name}'");
							}
							else {
								if (method.ReturnType == typeof(void) && singleMethod.ReturnType == typeof(void))
									errorsService.AddError($"Can not be connect two execute methods: '{method.Name}' and '{singleMethod.Name}', " +
										$"connection in '{nameof(ConnectMethodToMethodAttribute)}' at method '{method.Name}'");

								if (method.ReturnType == typeof(bool) && singleMethod.ReturnType == typeof(bool))
									errorsService.AddError($"Can not be connect two can execute methods: '{method.Name}' and '{singleMethod.Name}', " +
										$"connection in '{nameof(ConnectMethodToMethodAttribute)}' at method '{method.Name}'");
							}

							break;

						default:
							errorsService.AddError($"Found several methods with name '{attribute.ConnectingMethodName}', " +
								$"specified in '{nameof(ConnectMethodToMethodAttribute)}' at method '{method.Name}'");
							break;
					}
				}

				foreach (var attribute in method.GetCastedAttributes<ConnectMethodToPropertyAttribute>()) {
					if (method.ParameterTypes.Length > 1)
						errorsService.AddError($"Patching method '{method.Name}' can not have more than one parameter");

					if (method.ContainsAttribute<NotPatchingCommandAttribute>())
						errorsService.AddError($"Patching method '{method.Name}' can not have '{nameof(NotPatchingCommandAttribute)}'");

					if (method.ReturnType != typeof(void))
						errorsService.AddError($"Patching method '{method.Name}' can not have " +
							$"'{method.ReturnType.FullName}' return type, allowable types: '{typeof(void).FullName}'");

					var property = viewModelType.GetProperty(attribute.ConnectingPropertyName);

					if (property == null)
						errorsService.AddError($"Not found property with name '{attribute.ConnectingPropertyName}', " +
							$"specified in '{nameof(ConnectMethodToPropertyAttribute)}' at method '{method.Name}'");
					else if (property.IsNotInheritedFrom(commandType))
						errorsService.AddError($"Property '{property.Name}' can not have '{property.Type.FullName}' type, " +
							$"allowable types: all inherited from '{KnownTypeNames.ICommand}', " +
							$"connection in '{nameof(ConnectMethodToPropertyAttribute)}' at method '{method.Name}'");
				}
			}

			foreach (var property in viewModelType.Properties) {
				if (property.IsInheritedFrom(commandType))
					foreach (var attribute in property.GetCastedAttributes<ConnectPropertyToFieldAttribute>()) {
						var field = viewModelType.GetField(attribute.ConnectingFieldName);

						if (field == null)
							errorsService.AddError($"Not found field with name '{attribute.ConnectingFieldName}', " +
								$"specified in '{nameof(ConnectPropertyToFieldAttribute)}' at property '{property.Name}'");
						else if (property.IsNot(field))
							errorsService.AddError($"Types do not match between property '{property.Name}' and field '{field.Name}', " +
								$"connection in '{nameof(ConnectPropertyToFieldAttribute)}' at property '{property.Name}'");
					}

				var connectPropertyToMethodAttributes = property.GetCastedAttributes<ConnectPropertyToMethodAttribute>().ToArray();
				if (!connectPropertyToMethodAttributes.Any())
					continue;

				if (property.IsNotInheritedFrom(commandType))
					errorsService.AddError($"Patching property '{property.Name}' can not have '{property.Type.FullName}' type, " +
						$"allowable types: all inherited from '{KnownTypeNames.ICommand}'");

				foreach (var namesToMethods in connectPropertyToMethodAttributes.Select(attribute => attribute.ConnectingMethodNames.Select(name => new { Name = name, Methods = viewModelType.GetMethods(name).ToArray() }).ToArray())) {
					foreach (var nameToMethods in namesToMethods) {
						switch (nameToMethods.Methods.Length) {
							case 0:
								errorsService.AddError($"Not found method with name '{nameToMethods.Name}', " +
									$"specified in '{nameof(ConnectPropertyToMethodAttribute)}' at property '{property.Name}'");
								break;

							case 1:
								var singleMethod = nameToMethods.Methods.Single();

								if (singleMethod.ParameterTypes.Length > 1)
									errorsService.AddError($"Patching method '{singleMethod.Name}' can not have more than one parameter, " +
										$"connection in '{nameof(ConnectPropertyToMethodAttribute)}' at property '{property.Name}'");

								if (singleMethod.ContainsAttribute<NotPatchingCommandAttribute>())
									errorsService.AddError($"Patching method '{singleMethod.Name}' can not have '{nameof(NotPatchingCommandAttribute)}', " +
										$"connection in '{nameof(ConnectPropertyToMethodAttribute)}' at property '{property.Name}'");

								if (singleMethod.ReturnType != typeof(void) && singleMethod.ReturnType != typeof(bool))
									errorsService.AddError($"Patching method '{singleMethod.Name}' can not have " +
										$"'{singleMethod.ReturnType.FullName}' return type, allowable types: '{typeof(void).FullName}', '{typeof(bool).FullName}', " +
										$"connection in '{nameof(ConnectPropertyToMethodAttribute)}' at property '{property.Name}'");
								break;

							default:
								errorsService.AddError($"Found several methods with name '{nameToMethods.Name}', " +
									$"specified in '{nameof(ConnectPropertyToMethodAttribute)}' at property '{property.Name}'");
								break;
						}
					}

					switch (namesToMethods.Length) {
						case 1:
							var singleNameToMethods = namesToMethods.Single();

							if (singleNameToMethods.Methods.Length == 1 && singleNameToMethods.Methods.Single().ReturnType == typeof(bool))
								errorsService.AddError($"Can not be connect to can execute method '{singleNameToMethods.Methods.Single().Name}', " +
									$"connection in '{nameof(ConnectPropertyToMethodAttribute)}' at property '{property.Name}'");
							break;

						case 2:
							if (namesToMethods[0].Methods.Length == 1 && namesToMethods[1].Methods.Length == 1) {
								var singleFirstMethod = namesToMethods[0].Methods.Single();
								var singleSecondMethod = namesToMethods[1].Methods.Single();

								if (singleFirstMethod.ReturnType == typeof(void) && singleSecondMethod.ReturnType == typeof(void))
									errorsService.AddError($"Can not be connect to two execute methods: '{singleFirstMethod.Name}' and '{singleSecondMethod.Name}', " +
										$"connection in '{nameof(ConnectPropertyToMethodAttribute)}' at property '{property.Name}'");

								if (singleFirstMethod.ReturnType == typeof(bool) && singleSecondMethod.ReturnType == typeof(bool))
									errorsService.AddError($"Can not be connect to two can execute methods: '{singleFirstMethod.Name}' and '{singleSecondMethod.Name}', " +
										$"connection in '{nameof(ConnectPropertyToMethodAttribute)}' at property '{property.Name}'");
							}

							break;
					}
				}
			}

			foreach (var field in viewModelType.Fields.Where(field => field.IsInheritedFrom(commandType))) {
				foreach (var attribute in field.GetCastedAttributes<ConnectFieldToPropertyAttribute>()) {
					var property = viewModelType.GetProperty(attribute.ConnectingPropertyName);

					if (property == null)
						errorsService.AddError($"Not found property with name '{attribute.ConnectingPropertyName}', " +
							$"specified in '{nameof(ConnectFieldToPropertyAttribute)}' at field '{field.Name}'");
					else if (field.IsNot(property))
						errorsService.AddError($"Types do not match between field '{field.Name}' and property '{property.Name}', " +
							$"connection in '{nameof(ConnectFieldToPropertyAttribute)}' at field '{field.Name}'");
				}
			}

			if (errorsService.HasErrors)
				throw new ViewModelCommandPatchingException(errorsService);
		}

		private ICommonMethod[] GetPatchingMethods(IHasMethods viewModelType, ViewModelPatchingType patchingType) {
			return viewModelType.Methods
				.Where(method => method.NotContainsAttribute<NotPatchingCommandAttribute>() && method.ReturnType == typeof(void) && method.ParameterTypes.Length <= 1 &&
					(!applicationPatcherWpfConfiguration.SkipConnectingByNameIfNameIsInvalid || nameRulesService.IsNameValid(method.Name, UseNameRulesFor.CommandExecuteMethod)) && (
						patchingType == ViewModelPatchingType.All ||
						method.ContainsAttribute<PatchingCommandAttribute>() ||
						method.ContainsAttribute<ConnectMethodToMethodAttribute>() ||
						method.ContainsAttribute<ConnectMethodToPropertyAttribute>()))
				.ToArray();
		}

		private void CheckPatchingMethodNames(IEnumerable<ICommonMethod> patchingMethods) {
			var methodsWithUseSearchByName = patchingMethods
				.Where(method => method.NotContainsAttribute<NotUseSearchByNameAttribute>() &&
					(applicationPatcherWpfConfiguration.ConnectByNameIfExistConnectAttribute || method.NotContainsAttribute<ConnectMethodToMethodAttribute>() && method.NotContainsAttribute<ConnectMethodToPropertyAttribute>()));

			var errorsService = new ErrorsService()
				.AddErrors(methodsWithUseSearchByName
					.Where(method => !nameRulesService.IsNameValid(method.Name, UseNameRulesFor.CommandExecuteMethod))
					.Select(method => $"Not valid patching command method name '{method.Name}'"));

			if (errorsService.HasErrors)
				throw new ViewModelCommandPatchingException(errorsService);
		}

		private List<(ICommonMethod ExecuteMethod, List<ICommonMethod> CanExecuteMethods, List<ICommonProperty> Properties, List<ICommonField> Fields)> GetPatchingCommandGroups(IEnumerable<ICommonMethod> patchingMethods, ICommonType viewModelType, IHasType commandType) {
			var patchingCommandGroups = patchingMethods.Select(CreateEmptyPatchingCommandGroup).ToList();

			AddGroupsByCommandName(patchingCommandGroups, viewModelType);
			AddGroupsFromConnectMethodToMethodAttribute(patchingCommandGroups, viewModelType);
			AddGroupsFromConnectMethodToPropertyAttribute(patchingCommandGroups, viewModelType);
			AddGroupsFromConnectPropertyToMethodAttribute(patchingCommandGroups, viewModelType);

			patchingCommandGroups = SplitGroups(patchingCommandGroups);
			CheckPatchingPropertyNames(patchingCommandGroups.SelectMany(group => group.Properties));

			FillGroupsByPropertyName(patchingCommandGroups, viewModelType, commandType);
			FillGroupsFromConnectPropertyToFieldAttribute(patchingCommandGroups, viewModelType);
			FillGroupsFromConnectFieldToPropertyAttribute(patchingCommandGroups, viewModelType, commandType);

			FillGroupsWithoutProperty(patchingCommandGroups, viewModelType);

			return patchingCommandGroups;
		}
		private static (ICommonMethod ExecuteMethod, List<ICommonMethod> CanExecuteMethods, List<ICommonProperty> Properties, List<ICommonField> Fields) CreateEmptyPatchingCommandGroup(ICommonMethod executeMethod) {
			return (executeMethod, new List<ICommonMethod>(), new List<ICommonProperty>(), new List<ICommonField>());
		}
		private void AddGroupsByCommandName(ICollection<(ICommonMethod ExecuteMethod, List<ICommonMethod> CanExecuteMethods, List<ICommonProperty> Properties, List<ICommonField> Fields)> patchingCommandGroups, ICommonType viewModelType) {
			var methodsWithUseSearchByName = patchingCommandGroups.Select(group => group.ExecuteMethod)
				.Where(method => method.NotContainsAttribute<NotUseSearchByNameAttribute>() &&
					(applicationPatcherWpfConfiguration.ConnectByNameIfExistConnectAttribute || method.NotContainsAttribute<ConnectMethodToMethodAttribute>() && method.NotContainsAttribute<ConnectMethodToPropertyAttribute>()));

			foreach (var method in methodsWithUseSearchByName) {
				var propertyName = nameRulesService.ConvertName(method.Name, UseNameRulesFor.CommandExecuteMethod, UseNameRulesFor.CommandProperty);
				var canExecuteMethodName = nameRulesService.ConvertName(method.Name, UseNameRulesFor.CommandExecuteMethod, UseNameRulesFor.CommandCanExecuteMethod);

				if (viewModelType.TryGetProperty(propertyName, out var property))
					patchingCommandGroups.GetOrAdd(group => group.ExecuteMethod == method, () => CreateEmptyPatchingCommandGroup(method)).Properties.AddIfNotContains(property);
				foreach (var canExecuteMethod in viewModelType.GetMethods(canExecuteMethodName))
					patchingCommandGroups.GetOrAdd(group => group.ExecuteMethod == method, () => CreateEmptyPatchingCommandGroup(method)).CanExecuteMethods.AddIfNotContains(canExecuteMethod);
			}
		}
		private static void AddGroupsFromConnectMethodToMethodAttribute(ICollection<(ICommonMethod ExecuteMethod, List<ICommonMethod> CanExecuteMethods, List<ICommonProperty> Properties, List<ICommonField> Fields)> patchingCommandGroups, IHasMethods viewModelType) {
			foreach (var method in viewModelType.Methods) {
				foreach (var connectedMethod in method.GetCastedAttributes<ConnectMethodToMethodAttribute>().SelectMany(attribute => viewModelType.GetMethods(attribute.ConnectingMethodName))) {
					var executeMethod = method.ReturnType == typeof(void) ? method : connectedMethod.ReturnType == typeof(void) ? connectedMethod : throw new Exception();
					var canExecuteMethod = method.ReturnType == typeof(bool) ? method : connectedMethod.ReturnType == typeof(bool) ? connectedMethod : throw new Exception();
					patchingCommandGroups.GetOrAdd(group => group.ExecuteMethod == executeMethod, () => CreateEmptyPatchingCommandGroup(executeMethod)).CanExecuteMethods.AddIfNotContains(canExecuteMethod);
				}
			}
		}
		private static void AddGroupsFromConnectMethodToPropertyAttribute(ICollection<(ICommonMethod ExecuteMethod, List<ICommonMethod> CanExecuteMethods, List<ICommonProperty> Properties, List<ICommonField> Fields)> patchingCommandGroups, ICommonType viewModelType) {
			foreach (var method in viewModelType.Methods) {
				foreach (var property in method.GetCastedAttributes<ConnectMethodToPropertyAttribute>().Select(attribute => viewModelType.GetProperty(attribute.ConnectingPropertyName, true)))
					patchingCommandGroups.GetOrAdd(group => group.ExecuteMethod == method, () => CreateEmptyPatchingCommandGroup(method)).Properties.AddIfNotContains(property);
			}
		}
		private static void AddGroupsFromConnectPropertyToMethodAttribute(ICollection<(ICommonMethod ExecuteMethod, List<ICommonMethod> CanExecuteMethods, List<ICommonProperty> Properties, List<ICommonField> Fields)> patchingCommandGroups, ICommonType viewModelType) {
			foreach (var property in viewModelType.Properties) {
				foreach (var methods in property.GetCastedAttributes<ConnectPropertyToMethodAttribute>().Select(attribute => attribute.ConnectingMethodNames.SelectMany(viewModelType.GetMethods).ToArray())) {
					var executeMethod = methods.First(method => method.ReturnType == typeof(void));
					var canExecuteMethod = methods.FirstOrDefault(method => method.ReturnType == typeof(bool));

					var (_, canExecuteMethods, properties, _) = patchingCommandGroups.GetOrAdd(group => group.ExecuteMethod == executeMethod, () => CreateEmptyPatchingCommandGroup(executeMethod));
					properties.AddIfNotContains(property);

					if (canExecuteMethod != null)
						canExecuteMethods.AddIfNotContains(canExecuteMethod);
				}
			}
		}

		private static List<(ICommonMethod ExecuteMethod, List<ICommonMethod> CanExecuteMethods, List<ICommonProperty> Properties, List<ICommonField> Fields)> SplitGroups(IEnumerable<(ICommonMethod ExecuteMethod, List<ICommonMethod> CanExecuteMethods, List<ICommonProperty> Properties, List<ICommonField> Fields)> patchingCommandGroups) {
			return patchingCommandGroups.SelectMany(group => group.Properties.Any()
					? group.Properties.Select(property => {
						var newGroup = CreateEmptyPatchingCommandGroup(group.ExecuteMethod);
						newGroup.CanExecuteMethods.AddRange(group.CanExecuteMethods);
						newGroup.Properties.Add(property);
						return newGroup;
					})
					: new[] { group })
				.ToList();
		}
		private void CheckPatchingPropertyNames(IEnumerable<ICommonProperty> patchingProperties) {
			if (applicationPatcherWpfConfiguration.SkipConnectingByNameIfNameIsInvalid)
				return;

			var propertiesWithUseSearchByName = patchingProperties
				.Where(property => property.NotContainsAttribute<NotUseSearchByNameAttribute>() &&
					(applicationPatcherWpfConfiguration.ConnectByNameIfExistConnectAttribute || property.NotContainsAttribute<ConnectPropertyToFieldAttribute>()));

			var errorsService = new ErrorsService()
				.AddErrors(propertiesWithUseSearchByName
					.Where(method => !nameRulesService.IsNameValid(method.Name, UseNameRulesFor.CommandProperty))
					.Select(method => $"Not valid patching command property name '{method.Name}'"));

			if (errorsService.HasErrors)
				throw new ViewModelCommandPatchingException(errorsService);
		}

		private void FillGroupsByPropertyName(ICollection<(ICommonMethod ExecuteMethod, List<ICommonMethod> CanExecuteMethods, List<ICommonProperty> Properties, List<ICommonField> Fields)> patchingCommandGroups, IHasFields viewModelType, IHasType commandType) {
			var propertiesWithUseSearchByName = patchingCommandGroups.SelectMany(group => group.Properties)
				.Where(property => property.IsInheritedFrom(commandType) && property.NotContainsAttribute<NotUseSearchByNameAttribute>() &&
					(applicationPatcherWpfConfiguration.ConnectByNameIfExistConnectAttribute || property.NotContainsAttribute<ConnectPropertyToFieldAttribute>()));

			if (applicationPatcherWpfConfiguration.SkipConnectingByNameIfNameIsInvalid)
				propertiesWithUseSearchByName = propertiesWithUseSearchByName.Where(property => nameRulesService.IsNameValid(property.Name, UseNameRulesFor.CommandProperty));

			foreach (var property in propertiesWithUseSearchByName) {
				if (!viewModelType.TryGetField(nameRulesService.ConvertName(property.Name, UseNameRulesFor.CommandProperty, UseNameRulesFor.CommandField), out var foundField))
					continue;

				foreach (var (_, _, _, fields) in patchingCommandGroups.Where(group => group.Properties.Contains(property)))
					fields.AddIfNotContains(foundField);
			}
		}
		private static void FillGroupsFromConnectPropertyToFieldAttribute(ICollection<(ICommonMethod ExecuteMethod, List<ICommonMethod> CanExecuteMethods, List<ICommonProperty> Properties, List<ICommonField> Fields)> patchingCommandGroups, IHasFields viewModelType) {
			foreach (var property in patchingCommandGroups.SelectMany(group => group.Properties)) {
				var foundFields = property.GetCastedAttributes<ConnectPropertyToFieldAttribute>().Select(attribute => viewModelType.GetField(attribute.ConnectingFieldName, true)).ToArray();
				if (!foundFields.Any())
					continue;

				foreach (var (_, _, _, fields) in patchingCommandGroups.Where(group => group.Properties.Contains(property)))
					foundFields.ForEach(foundField => fields.AddIfNotContains(foundField));
			}
		}
		private static void FillGroupsFromConnectFieldToPropertyAttribute(ICollection<(ICommonMethod ExecuteMethod, List<ICommonMethod> CanExecuteMethods, List<ICommonProperty> Properties, List<ICommonField> Fields)> patchingCommandGroups, ICommonType viewModelType, IHasType commandType) {
			foreach (var field in viewModelType.Fields.Where(field => field.IsInheritedFrom(commandType))) {
				var properties = field.GetCastedAttributes<ConnectFieldToPropertyAttribute>().Select(attribute => viewModelType.GetProperty(attribute.ConnectingPropertyName, true)).ToArray();
				if (!properties.Any())
					continue;

				foreach (var (_, _, _, fields) in patchingCommandGroups.Where(group => group.Properties.Intersect(properties).Any()))
					fields.AddIfNotContains(field);
			}
		}

		private void FillGroupsWithoutProperty(IEnumerable<(ICommonMethod ExecuteMethod, List<ICommonMethod> CanExecuteMethods, List<ICommonProperty> Properties, List<ICommonField> Fields)> patchingCommandGroups, IHasFields viewModelType) {
			foreach (var (executeMethod, _, _, fields) in patchingCommandGroups.Where(group => !group.Properties.Any())) {
				var fieldName = nameRulesService.ConvertName(executeMethod.Name, UseNameRulesFor.CommandExecuteMethod, UseNameRulesFor.CommandField);
				if (viewModelType.TryGetField(fieldName, out var field))
					fields.AddIfNotContains(field);
			}
		}

		private void CheckPatchingCommandGroups(IEnumerable<(ICommonMethod ExecuteMethod, List<ICommonMethod> CanExecuteMethods, List<ICommonProperty> Properties, List<ICommonField> Fields)> patchingCommandGroups) {
			var errorsService = new ErrorsService();
			foreach (var group in patchingCommandGroups) {
				if (group.ExecuteMethod.ParameterTypes.Length > 1)
					errorsService.AddError($"Execute method '{group.ExecuteMethod.Name}' can not have more than one parameter");

				if (group.ExecuteMethod.ReturnType != typeof(void))
					errorsService.AddError($"Execute method '{group.ExecuteMethod.Name}' can not have " +
						$"'{group.ExecuteMethod.ReturnType.FullName}' return type, allowable types: '{typeof(void).FullName}'");

				switch (group.Properties.Count) {
					case 0:
						if (group.ExecuteMethod.ContainsAttribute<NotUseSearchByNameAttribute>())
							errorsService.AddError($"Not found property for execute method '{group.ExecuteMethod.Name}' when using '{nameof(NotUseSearchByNameAttribute)}'");
						break;

					case 1:
						var singleProperty = group.Properties.Single();

						switch (group.Fields.Count) {
							case 0:
								if (singleProperty.ContainsAttribute<NotUseSearchByNameAttribute>())
									errorsService.AddError($"Not found field for property '{singleProperty.Name}' when using '{nameof(NotUseSearchByNameAttribute)}'");
								break;

							case 1:
								var singleField = group.Fields.Single();

								if (singleProperty.IsNot(singleField))
									errorsService.AddError($"Types do not match inside group: property '{singleProperty.Name}', field '{singleField.Name}'");
								break;

							default:
								errorsService.AddError("Multi-connect property to field found: " +
									$"property '{singleProperty.Name}', fields: {group.Fields.Select(property => $"'{property.Name}'").JoinToString(", ")}");
								break;
						}

						break;

					default:
						errorsService.AddError("Multi-connect execute method to property found: " +
							$"execute method '{group.ExecuteMethod.Name}', properties: {group.Properties.Select(property => $"'{property.Name}'").JoinToString(", ")}");
						break;
				}

				switch (group.CanExecuteMethods.Count) {
					case 0:
						break;

					case 1:
						var singleCanExecuteMethod = group.CanExecuteMethods.Single();

						if (singleCanExecuteMethod.ParameterTypes.Length > 1)
							errorsService.AddError($"Can execute method '{singleCanExecuteMethod.Name}' can not have more than one parameter");

						if (singleCanExecuteMethod.ReturnType != typeof(bool))
							errorsService.AddError($"Can execute method '{singleCanExecuteMethod.Name}' can not have " +
								$"'{singleCanExecuteMethod.ReturnType.FullName}' return type, allowable types: '{typeof(bool).FullName}'");

						if (!group.ExecuteMethod.ParameterTypes.Any() && singleCanExecuteMethod.ParameterTypes.Any())
							errorsService.AddError($"Can execute method '{singleCanExecuteMethod.Name}' can not have parameter because method {group.ExecuteMethod.Name} has no parameters");

						if (group.ExecuteMethod.ParameterTypes.Any() && !singleCanExecuteMethod.ParameterTypes.Any())
							errorsService.AddError($"Patching method '{singleCanExecuteMethod.Name}' must have one parameter with type {group.ExecuteMethod.ParameterTypes.First().FullName}");

						break;

					default:
						errorsService.AddError("Multi-connect execute method to can execute method found: " +
							$"execute method '{group.ExecuteMethod.Name}', can execute methods: {group.CanExecuteMethods.Select(method => $"'{method.Name}'").JoinToString(", ")}");
						break;
				}
			}

			if (errorsService.HasErrors)
				throw new ViewModelCommandPatchingException(errorsService);
		}
	}
}