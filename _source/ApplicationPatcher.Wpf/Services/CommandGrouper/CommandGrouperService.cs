using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Core.Types.CommonMembers;
using ApplicationPatcher.Core.Types.Interfaces;
using ApplicationPatcher.Wpf.Configurations;
using ApplicationPatcher.Wpf.Exceptions;
using ApplicationPatcher.Wpf.Extensions;
using ApplicationPatcher.Wpf.Services.NameRules;
using ApplicationPatcher.Wpf.Types.Attributes;
using ApplicationPatcher.Wpf.Types.Enums;

namespace ApplicationPatcher.Wpf.Services.CommandGrouper {
	public class CommandGrouperService {
		private readonly ApplicationPatcherWpfConfiguration applicationPatcherWpfConfiguration;
		private readonly NameRulesService nameRulesService;

		public CommandGrouperService(ApplicationPatcherWpfConfiguration applicationPatcherWpfConfiguration, NameRulesService nameRulesService) {
			this.applicationPatcherWpfConfiguration = applicationPatcherWpfConfiguration;
			this.nameRulesService = nameRulesService;
		}

		public CommandGroup[] GetGroups(IHasTypes commonAssembly, CommonType viewModelType, ViewModelPatchingType patchingType) {
			var commandType = commonAssembly.GetCommonType(KnownTypeNames.ICommand, true);
			CheckViewModel(commandType, viewModelType);

			var patchingMethods = GetPatchingMethods(viewModelType, patchingType);
			CheckPatchingMethodNames(patchingMethods);

			var patchingCommandGroups = GetPatchingCommandGroups(patchingMethods, viewModelType, commandType);
			CheckPatchingCommandGroups(patchingCommandGroups);

			return patchingCommandGroups.Select(group => new CommandGroup(group.Fields.SingleOrDefault(), group.Properties.SingleOrDefault(), group.ExecuteMethod, group.CanExecuteMethods.SingleOrDefault())).ToArray();
		}

		private static void CheckViewModel(IHasType commandType, CommonType viewModelType) {
			var methodsWithPatchingCommandAttribute = viewModelType.Methods.Where(method => method.ContainsAttribute<PatchingCommandAttribute>()).ToArray();

			var errorsService = new ErrorsService()
				.AddErrors(methodsWithPatchingCommandAttribute
					.Where(method => method.ParameterTypes.Any())
					.Select(method => $"Patching method '{method.Name}' can not have parameters"))
				.AddErrors(methodsWithPatchingCommandAttribute
					.Where(method => method.ContainsAttribute<NotPatchingCommandAttribute>())
					.Select(method => $"Patching method '{method.Name}' can not have " +
						$"'{nameof(PatchingCommandAttribute)}' and '{nameof(NotPatchingCommandAttribute)}' at the same time"))
				.AddErrors(methodsWithPatchingCommandAttribute
					.Where(method => method.ReturnType != typeof(void))
					.Select(method => $"Patching method '{method.Name}' can not have " +
						$"'{method.ReturnType.FullName}' return type, allowable types: '{typeof(void).FullName}'"));

			var connectMethodToMethodAttributes = viewModelType.Methods
				.SelectMany(method => method.GetReflectionAttributes<ConnectMethodToMethodAttribute>()
					.Select(attribute => new { Method = method, Attribute = attribute, Methods = viewModelType.GetMethods(attribute.ConnectingMethodName) }))
				.ToArray();

			errorsService
				.AddErrors(connectMethodToMethodAttributes
					.Where(x => x.Method.ParameterTypes.Any())
					.Select(x => $"Patching method '{x.Method.Name}' can not have parameters"))
				.AddErrors(connectMethodToMethodAttributes
					.Where(x => x.Method.ContainsAttribute<NotPatchingCommandAttribute>())
					.Select(x => $"Patching method '{x.Method.Name}' can not have '{nameof(NotPatchingCommandAttribute)}'"))
				.AddErrors(connectMethodToMethodAttributes
					.Where(x => x.Method.ReturnType != typeof(void) && x.Method.ReturnType != typeof(bool))
					.Select(x => $"Patching method '{x.Method.Name}' can not have " +
						$"'{x.Method.ReturnType.FullName}' return type, allowable types: '{typeof(void).FullName}', '{typeof(bool).FullName}'"))
				.AddErrors(connectMethodToMethodAttributes
					.Where(x => !x.Methods.Any())
					.Select(x => $"Not found method with name '{x.Attribute.ConnectingMethodName}', " +
						$"specified in '{nameof(ConnectMethodToMethodAttribute)}' at method '{x.Method.Name}'"))
				.AddErrors(connectMethodToMethodAttributes
					.Where(x => x.Methods.Length > 1)
					.Select(x => $"Found several methods with name '{x.Attribute.ConnectingMethodName}', " +
						$"specified in '{nameof(ConnectMethodToMethodAttribute)}' at method '{x.Method.Name}'"))
				.AddErrors(connectMethodToMethodAttributes
					.Where(x => x.Methods.Length == 1 && x.Methods.Single().ParameterTypes.Any())
					.Select(x => $"Patching method '{x.Methods.Single().Name}' can not have parameters, " +
						$"connection in '{nameof(ConnectMethodToMethodAttribute)}' at method '{x.Method.Name}'"))
				.AddErrors(connectMethodToMethodAttributes
					.Where(x => x.Methods.Length == 1 && x.Methods.Single().ContainsAttribute<NotPatchingCommandAttribute>())
					.Select(x => $"Patching method '{x.Methods.Single().Name}' can not have '{nameof(NotPatchingCommandAttribute)}', " +
						$"connection in '{nameof(ConnectMethodToMethodAttribute)}' at method '{x.Method.Name}'"))
				.AddErrors(connectMethodToMethodAttributes
					.Where(x => x.Methods.Length == 1 && x.Methods.Single().ReturnType != typeof(void) && x.Methods.Single().ReturnType != typeof(bool))
					.Select(x => $"Patching method '{x.Methods.Single().Name}' can not have " +
						$"'{x.Methods.Single().ReturnType.FullName}' return type, allowable types: '{typeof(void).FullName}', '{typeof(bool).FullName}', " +
						$"connection in '{nameof(ConnectMethodToMethodAttribute)}' at method '{x.Method.Name}'"))
				.AddErrors(connectMethodToMethodAttributes
					.Where(x => x.Method.ReturnType == typeof(void) || x.Method.ReturnType == typeof(bool))
					.Where(x => x.Methods.Length == 1 && (x.Methods.Single().ReturnType == typeof(void) || x.Methods.Single().ReturnType == typeof(bool)))
					.Where(x => x.Method.ReturnType == typeof(void) && x.Methods.Single().ReturnType == typeof(void))
					.Select(x => $"Can not be connect two execute methods: '{x.Method.Name}' and '{x.Methods.Single().Name}', " +
						$"connection in '{nameof(ConnectMethodToMethodAttribute)}' at method '{x.Method.Name}'"))
				.AddErrors(connectMethodToMethodAttributes
					.Where(x => x.Method.ReturnType == typeof(void) || x.Method.ReturnType == typeof(bool))
					.Where(x => x.Methods.Length == 1 && (x.Methods.Single().ReturnType == typeof(void) || x.Methods.Single().ReturnType == typeof(bool)))
					.Where(x => x.Method.ReturnType == typeof(bool) && x.Methods.Single().ReturnType == typeof(bool))
					.Select(x => $"Can not be connect two can execute methods: '{x.Method.Name}' and '{x.Methods.Single().Name}', " +
						$"connection in '{nameof(ConnectMethodToMethodAttribute)}' at method '{x.Method.Name}'"));

			var connectMethodToPropertyAttributes = viewModelType.Methods
				.SelectMany(method => method.GetReflectionAttributes<ConnectMethodToPropertyAttribute>()
					.Select(attribute => new { Method = method, Attribute = attribute, Property = viewModelType.GetProperty(attribute.ConnectingPropertyName) }))
				.ToArray();

			errorsService
				.AddErrors(connectMethodToPropertyAttributes
					.Where(x => x.Method.ParameterTypes.Any())
					.Select(x => $"Patching method '{x.Method.Name}' can not have parameters"))
				.AddErrors(connectMethodToPropertyAttributes
					.Where(x => x.Method.ContainsAttribute<NotPatchingCommandAttribute>())
					.Select(x => $"Patching method '{x.Method.Name}' can not have '{nameof(NotPatchingCommandAttribute)}'"))
				.AddErrors(connectMethodToPropertyAttributes
					.Where(x => x.Method.ReturnType != typeof(void))
					.Select(x => $"Patching method '{x.Method.Name}' can not have " +
						$"'{x.Method.ReturnType.FullName}' return type, allowable types: '{typeof(void).FullName}'"))
				.AddErrors(connectMethodToPropertyAttributes
					.Where(x => x.Property == null)
					.Select(x => $"Not found property with name '{x.Attribute.ConnectingPropertyName}', " +
						$"specified in '{nameof(ConnectMethodToPropertyAttribute)}' at method '{x.Method.Name}'"))
				.AddErrors(connectMethodToPropertyAttributes
					.Where(x => x.Property != null && x.Property.IsNotInheritedFrom(commandType))
					.Select(x => $"Property '{x.Property.Name}' can not have '{x.Property.Type.FullName}' type, " +
						$"allowable types: all inherited from '{KnownTypeNames.ICommand}', " +
						$"connection in '{nameof(ConnectMethodToPropertyAttribute)}' at method '{x.Method.Name}'"));

			var connectPropertyToMethodAttributes = viewModelType.Properties
				.SelectMany(property => property.GetReflectionAttributes<ConnectPropertyToMethodAttribute>()
					.Select(attribute => new { Property = property, Attribute = attribute, NamesToMethods = attribute.ConnectingMethodNames.Select(name => new { Name = name, Methods = viewModelType.GetMethods(name) }).ToArray() }))
				.ToArray();

			errorsService
				.AddErrors(connectPropertyToMethodAttributes
					.Where(x => x.Property.IsNotInheritedFrom(commandType))
					.Select(x => $"Patching property '{x.Property.Name}' can not have '{x.Property.Type.FullName}' type, " +
						$"allowable types: all inherited from '{KnownTypeNames.ICommand}'"))
				.AddErrors(connectPropertyToMethodAttributes
					.SelectMany(x => x.NamesToMethods
						.Where(y => !y.Methods.Any())
						.Select(y => $"Not found method with name '{y.Name}', " +
							$"specified in '{nameof(ConnectPropertyToMethodAttribute)}' at property '{x.Property.Name}'")))
				.AddErrors(connectPropertyToMethodAttributes
					.SelectMany(x => x.NamesToMethods
						.Where(y => y.Methods.Length > 1)
						.Select(y => $"Found several methods with name '{y.Name}', " +
							$"specified in '{nameof(ConnectPropertyToMethodAttribute)}' at property '{x.Property.Name}'")))
				.AddErrors(connectPropertyToMethodAttributes
					.SelectMany(x => x.NamesToMethods
						.Where(y => y.Methods.Length == 1 && y.Methods.Single().ParameterTypes.Any())
						.Select(y => $"Patching method '{y.Methods.Single().Name}' can not have parameters, " +
							$"connection in '{nameof(ConnectPropertyToMethodAttribute)}' at property '{x.Property.Name}'")))
				.AddErrors(connectPropertyToMethodAttributes
					.SelectMany(x => x.NamesToMethods
						.Where(y => y.Methods.Length == 1 && y.Methods.Single().ContainsAttribute<NotPatchingCommandAttribute>())
						.Select(y => $"Patching method '{y.Methods.Single().Name}' can not have '{nameof(NotPatchingCommandAttribute)}', " +
							$"connection in '{nameof(ConnectPropertyToMethodAttribute)}' at property '{x.Property.Name}'")))
				.AddErrors(connectPropertyToMethodAttributes
					.SelectMany(x => x.NamesToMethods
						.Where(y => y.Methods.Length == 1 && y.Methods.Single().ReturnType != typeof(void) && y.Methods.Single().ReturnType != typeof(bool))
						.Select(y => $"Patching method '{y.Methods.Single().Name}' can not have " +
							$"'{y.Methods.Single().ReturnType.FullName}' return type, allowable types: '{typeof(void).FullName}', '{typeof(bool).FullName}', " +
							$"connection in '{nameof(ConnectPropertyToMethodAttribute)}' at property '{x.Property.Name}'")))
				.AddErrors(connectPropertyToMethodAttributes
					.Where(x => x.NamesToMethods.Length == 1 && x.NamesToMethods.Single().Methods.Length == 1)
					.Where(x => x.NamesToMethods.Single().Methods.Single().ReturnType == typeof(bool))
					.Select(x => $"Can not be connect to can execute method '{x.NamesToMethods.Single().Methods.Single().Name}', " +
						$"connection in '{nameof(ConnectPropertyToMethodAttribute)}' at property '{x.Property.Name}'"))
				.AddErrors(connectPropertyToMethodAttributes
					.Where(x => x.NamesToMethods.Length == 2 && x.NamesToMethods[0].Methods.Length == 1 && x.NamesToMethods[1].Methods.Length == 1)
					.Where(x => x.NamesToMethods[0].Methods.Single().ReturnType == typeof(void) && x.NamesToMethods[1].Methods.Single().ReturnType == typeof(void))
					.Select(x => $"Can not be connect to two execute methods: '{x.NamesToMethods[0].Methods.Single().Name}' and '{x.NamesToMethods[1].Methods.Single().Name}', " +
						$"connection in '{nameof(ConnectPropertyToMethodAttribute)}' at property '{x.Property.Name}'"))
				.AddErrors(connectPropertyToMethodAttributes
					.Where(x => x.NamesToMethods.Length == 2 && x.NamesToMethods[0].Methods.Length == 1 && x.NamesToMethods[1].Methods.Length == 1)
					.Where(x => x.NamesToMethods[0].Methods.Single().ReturnType == typeof(bool) && x.NamesToMethods[1].Methods.Single().ReturnType == typeof(bool))
					.Select(x => $"Can not be connect to two can execute methods: '{x.NamesToMethods[0].Methods.Single().Name}' and '{x.NamesToMethods[1].Methods.Single().Name}', " +
						$"connection in '{nameof(ConnectPropertyToMethodAttribute)}' at property '{x.Property.Name}'"));

			var connectPropertyToFieldAttributes = viewModelType.Properties
				.Where(property => property.IsInheritedFrom(commandType))
				.SelectMany(property => property.GetReflectionAttributes<ConnectPropertyToFieldAttribute>()
					.Select(attribute => new { Property = property, Attribute = attribute, Field = viewModelType.GetField(attribute.ConnectingFieldName) }))
				.ToArray();

			errorsService
				.AddErrors(connectPropertyToFieldAttributes
					.Where(x => x.Field == null)
					.Select(x => $"Not found field with name '{x.Attribute.ConnectingFieldName}', " +
						$"specified in '{nameof(ConnectPropertyToFieldAttribute)}' at property '{x.Property.Name}'"))
				.AddErrors(connectPropertyToFieldAttributes
					.Where(x => x.Field != null && x.Property.IsNot(x.Field))
					.Select(x => $"Types do not match between property '{x.Property.Name}' and field '{x.Field.Name}', " +
						$"connection in '{nameof(ConnectPropertyToFieldAttribute)}' at property '{x.Property.Name}'"));

			var connectFieldToPropertyAttributes = viewModelType.Fields
				.Where(field => field.IsInheritedFrom(commandType))
				.SelectMany(field => field.GetReflectionAttributes<ConnectFieldToPropertyAttribute>()
					.Select(attribute => new { Field = field, Attribute = attribute, Property = viewModelType.GetProperty(attribute.ConnectingPropertyName) }))
				.ToArray();

			errorsService
				.AddErrors(connectFieldToPropertyAttributes
					.Where(x => x.Property == null)
					.Select(x => $"Not found property with name '{x.Attribute.ConnectingPropertyName}', " +
						$"specified in '{nameof(ConnectFieldToPropertyAttribute)}' at field '{x.Field.Name}'"))
				.AddErrors(connectFieldToPropertyAttributes
					.Where(x => x.Property != null && x.Field.IsNot(x.Property))
					.Select(x => $"Types do not match between field '{x.Field.Name}' and property '{x.Property.Name}', " +
						$"connection in '{nameof(ConnectFieldToPropertyAttribute)}' at field '{x.Field.Name}'"));

			if (errorsService.HasErrors)
				throw new CommandPatchingException(errorsService);
		}

		private CommonMethod[] GetPatchingMethods(IHasMethods viewModelType, ViewModelPatchingType patchingType) {
			return viewModelType.Methods
				.Where(method => method.NotContainsAttribute<NotPatchingCommandAttribute>() && method.ReturnType == typeof(void) && !method.ParameterTypes.Any() &&
					(!applicationPatcherWpfConfiguration.SkipConnectingByNameIfNameIsInvalid || nameRulesService.IsNameValid(method.Name, UseNameRulesFor.CommandExecuteMethod)) && (
						patchingType == ViewModelPatchingType.All ||
						method.ContainsAttribute<PatchingCommandAttribute>() ||
						method.ContainsAttribute<ConnectMethodToMethodAttribute>() ||
						method.ContainsAttribute<ConnectMethodToPropertyAttribute>()))
				.ToArray();
		}

		private void CheckPatchingMethodNames(IEnumerable<CommonMethod> patchingMethods) {
			var methodsWithUseSearchByName = patchingMethods
				.Where(method => method.NotContainsAttribute<NotUseSearchByNameAttribute>() &&
					(applicationPatcherWpfConfiguration.ConnectByNameIfExsistConnectAttribute || method.NotContainsAttribute<ConnectMethodToMethodAttribute>() && method.NotContainsAttribute<ConnectMethodToPropertyAttribute>()))
				.ToArray();

			var errorsService = new ErrorsService()
				.AddErrors(methodsWithUseSearchByName
					.Where(method => !nameRulesService.IsNameValid(method.Name, UseNameRulesFor.CommandExecuteMethod))
					.Select(method => $"Not valid patching command method name '{method.Name}'"));

			if (errorsService.HasErrors)
				throw new CommandPatchingException(errorsService);
		}

		private List<(CommonMethod ExecuteMethod, List<CommonMethod> CanExecuteMethods, List<CommonProperty> Properties, List<CommonField> Fields)> GetPatchingCommandGroups(IEnumerable<CommonMethod> patchingMethods, CommonType viewModelType, CommonType commandType) {
			var patchingCommandGroups = patchingMethods.Select(CreateEmptyPatchingCommandGroup).ToList();

			AddGroupsByCommandName(patchingCommandGroups, viewModelType);
			AddGroupsFromConnectMethodToMethodAttribute(patchingCommandGroups, viewModelType);
			AddGroupsFromConnectMethodToPropertyAttribute(patchingCommandGroups, viewModelType);
			AddGroupsFromConnectPropertyToMethodAttribute(patchingCommandGroups, viewModelType);

			patchingCommandGroups = SplitGroups(patchingCommandGroups);
			CheckPatchingPropertyNames(patchingCommandGroups.SelectMany(group => group.Properties));

			FillGroupsByPropertyName(patchingCommandGroups, viewModelType, commandType);
			FillGroupsFromConnectPropertyToFieldAttribute(patchingCommandGroups, viewModelType, commandType);
			FillGroupsFromConnectFieldToPropertyAttribute(patchingCommandGroups, viewModelType, commandType);

			return patchingCommandGroups;
		}
		private void AddGroupsByCommandName(ICollection<(CommonMethod ExecuteMethod, List<CommonMethod> CanExecuteMethods, List<CommonProperty> Properties, List<CommonField> Fields)> patchingCommandGroups, CommonType viewModelType) {
			var methodsWithUseSearchByName = patchingCommandGroups.Select(group => group.ExecuteMethod)
				.Where(method => method.NotContainsAttribute<NotUseSearchByNameAttribute>() &&
					(applicationPatcherWpfConfiguration.ConnectByNameIfExsistConnectAttribute || method.NotContainsAttribute<ConnectMethodToMethodAttribute>() && method.NotContainsAttribute<ConnectMethodToPropertyAttribute>()))
				.ToArray();

			foreach (var method in methodsWithUseSearchByName) {
				var propertyName = nameRulesService.ConvertName(method.Name, UseNameRulesFor.CommandExecuteMethod, UseNameRulesFor.CommandProperty);
				var canExecuteMethodName = nameRulesService.ConvertName(method.Name, UseNameRulesFor.CommandExecuteMethod, UseNameRulesFor.CommandCanExecuteMethod);

				if (viewModelType.TryGetProperty(propertyName, out var property))
					patchingCommandGroups.GetOrAdd(group => group.ExecuteMethod == method, () => CreateEmptyPatchingCommandGroup(method)).Properties.AddIfNotContains(property);
				if (viewModelType.TryGetMethod(canExecuteMethodName, out var canExecuteMethod))
					patchingCommandGroups.GetOrAdd(group => group.ExecuteMethod == method, () => CreateEmptyPatchingCommandGroup(method)).CanExecuteMethods.AddIfNotContains(canExecuteMethod);
			}
		}
		private static void AddGroupsFromConnectMethodToMethodAttribute(ICollection<(CommonMethod ExecuteMethod, List<CommonMethod> CanExecuteMethods, List<CommonProperty> Properties, List<CommonField> Fields)> patchingCommandGroups, IHasMethods viewModelType) {
			foreach (var method in viewModelType.Methods) {
				foreach (var connectedMethod in method.GetReflectionAttributes<ConnectMethodToMethodAttribute>().Select(attribute => viewModelType.GetMethod(attribute.ConnectingMethodName, true))) {
					var executeMethod = method.ReturnType == typeof(void) ? method : connectedMethod.ReturnType == typeof(void) ? connectedMethod : throw new Exception();
					var canExecuteMethod = method.ReturnType == typeof(bool) ? method : connectedMethod.ReturnType == typeof(bool) ? connectedMethod : throw new Exception();
					patchingCommandGroups.GetOrAdd(group => group.ExecuteMethod == executeMethod, () => CreateEmptyPatchingCommandGroup(executeMethod)).CanExecuteMethods.AddIfNotContains(canExecuteMethod);
				}
			}
		}
		private static void AddGroupsFromConnectMethodToPropertyAttribute(ICollection<(CommonMethod ExecuteMethod, List<CommonMethod> CanExecuteMethods, List<CommonProperty> Properties, List<CommonField> Fields)> patchingCommandGroups, CommonType viewModelType) {
			foreach (var method in viewModelType.Methods) {
				foreach (var property in method.GetReflectionAttributes<ConnectMethodToPropertyAttribute>().Select(attribute => viewModelType.GetProperty(attribute.ConnectingPropertyName, true)))
					patchingCommandGroups.GetOrAdd(group => group.ExecuteMethod == method, () => CreateEmptyPatchingCommandGroup(method)).Properties.AddIfNotContains(property);
			}
		}
		private static void AddGroupsFromConnectPropertyToMethodAttribute(ICollection<(CommonMethod ExecuteMethod, List<CommonMethod> CanExecuteMethods, List<CommonProperty> Properties, List<CommonField> Fields)> patchingCommandGroups, CommonType viewModelType) {
			foreach (var property in viewModelType.Properties) {
				foreach (var methods in property.GetReflectionAttributes<ConnectPropertyToMethodAttribute>().Select(attribute => attribute.ConnectingMethodNames.Select(name => viewModelType.GetMethod(name, true)).ToArray())) {
					var executeMethod = methods.First(method => method.ReturnType == typeof(void));
					var canExecuteMethod = methods.FirstOrDefault(method => method.ReturnType == typeof(bool));

					var (_, canExecuteMethods, properties, _) = patchingCommandGroups.GetOrAdd(group => group.ExecuteMethod == executeMethod, () => CreateEmptyPatchingCommandGroup(executeMethod));
					properties.AddIfNotContains(property);

					if (canExecuteMethod != null)
						canExecuteMethods.AddIfNotContains(canExecuteMethod);
				}
			}
		}

		private static List<(CommonMethod ExecuteMethod, List<CommonMethod> CanExecuteMethods, List<CommonProperty> Properties, List<CommonField> Fields)> SplitGroups(IEnumerable<(CommonMethod ExecuteMethod, List<CommonMethod> CanExecuteMethods, List<CommonProperty> Properties, List<CommonField> Fields)> patchingCommandGroups) {
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
		private void CheckPatchingPropertyNames(IEnumerable<CommonProperty> patchingProperties) {
			if (applicationPatcherWpfConfiguration.SkipConnectingByNameIfNameIsInvalid)
				return;

			var propertiesWithUseSearchByName = patchingProperties
				.Where(property => property.NotContainsAttribute<NotUseSearchByNameAttribute>() &&
					(applicationPatcherWpfConfiguration.ConnectByNameIfExsistConnectAttribute || property.NotContainsAttribute<ConnectPropertyToFieldAttribute>()))
				.ToArray();

			var errorsService = new ErrorsService()
				.AddErrors(propertiesWithUseSearchByName
					.Where(method => !nameRulesService.IsNameValid(method.Name, UseNameRulesFor.CommandProperty))
					.Select(method => $"Not valid patching command property name '{method.Name}'"));

			if (errorsService.HasErrors)
				throw new CommandPatchingException(errorsService);
		}

		private void FillGroupsByPropertyName(ICollection<(CommonMethod ExecuteMethod, List<CommonMethod> CanExecuteMethods, List<CommonProperty> Properties, List<CommonField> Fields)> patchingCommandGroups, CommonType viewModelType, IHasType commandType) {
			var propertiesWithUseSearchByName = patchingCommandGroups.SelectMany(group => group.Properties)
				.Where(property => property.IsInheritedFrom(commandType) && property.NotContainsAttribute<NotUseSearchByNameAttribute>() &&
					(applicationPatcherWpfConfiguration.ConnectByNameIfExsistConnectAttribute || property.NotContainsAttribute<ConnectPropertyToFieldAttribute>()))
				.ToArray();

			if (applicationPatcherWpfConfiguration.SkipConnectingByNameIfNameIsInvalid)
				propertiesWithUseSearchByName = propertiesWithUseSearchByName.Where(property => nameRulesService.IsNameValid(property.Name, UseNameRulesFor.CommandProperty)).ToArray();

			foreach (var property in propertiesWithUseSearchByName) {
				if (!viewModelType.TryGetField(nameRulesService.ConvertName(property.Name, UseNameRulesFor.CommandProperty, UseNameRulesFor.CommandField), out var foundField))
					continue;

				foreach (var (_, _, _, fields) in patchingCommandGroups.Where(group => group.Properties.Contains(property)))
					fields.AddIfNotContains(foundField);
			}
		}
		private static void FillGroupsFromConnectPropertyToFieldAttribute(ICollection<(CommonMethod ExecuteMethod, List<CommonMethod> CanExecuteMethods, List<CommonProperty> Properties, List<CommonField> Fields)> patchingCommandGroups, CommonType viewModelType, IHasType commandType) {
			foreach (var property in patchingCommandGroups.SelectMany(group => group.Properties)) {
				var foundFields = property.GetReflectionAttributes<ConnectPropertyToFieldAttribute>().Select(attribute => viewModelType.GetField(attribute.ConnectingFieldName, true)).ToArray();
				if (!foundFields.Any())
					continue;

				foreach (var (_, _, _, fields) in patchingCommandGroups.Where(group => group.Properties.Contains(property)))
					foundFields.ForEach(foundField => fields.AddIfNotContains(foundField));
			}
		}
		private static void FillGroupsFromConnectFieldToPropertyAttribute(ICollection<(CommonMethod ExecuteMethod, List<CommonMethod> CanExecuteMethods, List<CommonProperty> Properties, List<CommonField> Fields)> patchingCommandGroups, CommonType viewModelType, IHasType commandType) {
			foreach (var field in viewModelType.Fields.Where(field => field.IsInheritedFrom(commandType))) {
				var properties = field.GetReflectionAttributes<ConnectFieldToPropertyAttribute>().Select(attribute => viewModelType.GetProperty(attribute.ConnectingPropertyName, true)).ToArray();
				if (!properties.Any())
					continue;

				foreach (var (_, _, _, fields) in patchingCommandGroups.Where(group => group.Properties.Intersect(properties).Any()))
					fields.AddIfNotContains(field);
			}
		}
		private static (CommonMethod ExecuteMethod, List<CommonMethod> CanExecuteMethods, List<CommonProperty> Properties, List<CommonField> Fields) CreateEmptyPatchingCommandGroup(CommonMethod executeMethod) {
			return (executeMethod, new List<CommonMethod>(), new List<CommonProperty>(), new List<CommonField>());
		}

		private void CheckPatchingCommandGroups(IReadOnlyCollection<(CommonMethod ExecuteMethod, List<CommonMethod> CanExecuteMethods, List<CommonProperty> Properties, List<CommonField> Fields)> patchingCommandGroups) {
			var errorsService = new ErrorsService()
				.AddErrors(patchingCommandGroups
					.Where(group => group.Properties.Count == 1 && group.Fields.Count > 1)
					.Select(group => "Multi-connect property to field found: " +
						$"property '{group.Properties.Single().Name}', fields: {group.Fields.Select(property => $"'{property.Name}'").JoinToString(", ")}"))
				.AddErrors(patchingCommandGroups
					.Where(group => group.Properties.Count > 1)
					.Select(group => "Multi-connect execute method to property found: " +
						$"execute method '{group.ExecuteMethod.Name}', properties: {group.Properties.Select(property => $"'{property.Name}'").JoinToString(", ")}"))
				.AddErrors(patchingCommandGroups
					.Where(group => group.CanExecuteMethods.Count > 1)
					.Select(group => "Multi-connect execute method to can execute method found: " +
						$"execute method '{group.ExecuteMethod.Name}', can execute methods: {group.CanExecuteMethods.Select(method => $"'{method.Name}'").JoinToString(", ")}"))
				.AddErrors(patchingCommandGroups
					.Where(group => group.Properties.Count == 1 && group.Fields.Count == 1 && group.Properties.Single().IsNot(group.Fields.Single()))
					.Select(group => $"Types do not match inside group: property '{group.Properties.Single().Name}', field '{group.Fields.Single().Name}'"))
				.AddErrors(patchingCommandGroups
					.Where(group => group.ExecuteMethod.ParameterTypes.Any())
					.Select(group => $"Execute method '{group.ExecuteMethod.Name}' can not have parameters"))
				.AddErrors(patchingCommandGroups
					.Where(group => group.ExecuteMethod.ReturnType != typeof(void))
					.Select(group => $"Execute method '{group.ExecuteMethod.Name}' can not have " +
						$"'{group.ExecuteMethod.ReturnType.FullName}' return type, allowable types: '{typeof(void).FullName}'"))
				.AddErrors(patchingCommandGroups
					.Where(group => group.CanExecuteMethods.Count == 1 && group.CanExecuteMethods.Single().ParameterTypes.Any())
					.Select(group => $"Can execute method '{group.CanExecuteMethods.Single().Name}' can not have parameters"))
				.AddErrors(patchingCommandGroups
					.Where(group => group.CanExecuteMethods.Count == 1 && group.CanExecuteMethods.Single().ReturnType != typeof(bool))
					.Select(group => $"Can execute method '{group.CanExecuteMethods.Single().Name}' can not have " +
						$"'{group.CanExecuteMethods.Single().ReturnType.FullName}' return type, allowable types: '{typeof(bool).FullName}'"))
				.AddErrors(patchingCommandGroups
					.Where(group => !group.Properties.Any() && group.ExecuteMethod.ContainsAttribute<NotUseSearchByNameAttribute>())
					.Select(group => $"Not found property for execute method '{group.ExecuteMethod.Name}' when using '{nameof(NotUseSearchByNameAttribute)}'"))
				.AddErrors(patchingCommandGroups
					.Where(group => !group.Fields.Any() && group.Properties.Count == 1 && group.Properties.Single().ContainsAttribute<NotUseSearchByNameAttribute>())
					.Select(group => $"Not found field for property '{group.Properties.Single().Name}' when using '{nameof(NotUseSearchByNameAttribute)}'"));

			if (errorsService.HasErrors)
				throw new CommandPatchingException(errorsService);
		}
	}
}