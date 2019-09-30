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

namespace ApplicationPatcher.Wpf.Services.Groupers.Property {
	public class PropertyGrouperService {
		private readonly ApplicationPatcherWpfConfiguration applicationPatcherWpfConfiguration;
		private readonly NameRulesService nameRulesService;

		public PropertyGrouperService(ApplicationPatcherWpfConfiguration applicationPatcherWpfConfiguration, NameRulesService nameRulesService) {
			this.applicationPatcherWpfConfiguration = applicationPatcherWpfConfiguration;
			this.nameRulesService = nameRulesService;
		}

		public PropertyGroup[] GetGroups(ICommonAssembly assembly, ICommonType viewModelType, ViewModelPatchingType patchingType) {
			var commandType = assembly.GetCommonType(KnownTypeNames.ICommand, true);
			CheckViewModel(commandType, viewModelType);

			var patchingProperties = GetPatchingProperties(viewModelType, commandType, patchingType);
			CheckPatchingPropertyNames(patchingProperties);

			var patchingPropertyGroups = GetPatchingPropertyGroups(patchingProperties, viewModelType, commandType);
			CheckPatchingPropertyGroups(patchingPropertyGroups);

			return patchingPropertyGroups
				.Select(group => {
					var patchingPropertyAttribute = group.Property.GetCastedAttribute<PatchingPropertyAttribute>();
					return new PropertyGroup(
						group.Fields.SingleOrDefault(),
						group.Property,
						GetCalledMethod(viewModelType, patchingPropertyAttribute?.CalledMethodNameBeforeGetProperty),
						GetCalledMethod(viewModelType, patchingPropertyAttribute?.CalledMethodNameBeforeSetProperty),
						GetCalledMethod(viewModelType, patchingPropertyAttribute?.CalledMethodNameAfterSuccessSetProperty),
						GetCalledMethod(viewModelType, patchingPropertyAttribute?.CalledMethodNameAfterSetProperty));
				})
				.ToArray();
		}

		private static void CheckViewModel(IHasType commandType, ICommonType viewModelType) {
			var errorsService = new ErrorsService();
			foreach (var property in viewModelType.Properties.Select(property => property)) {
				var patchingPropertyAttribute = property.GetCastedAttribute<PatchingPropertyAttribute>();
				if (patchingPropertyAttribute != null) {
					if (property.IsInheritedFrom(commandType))
						errorsService.AddError($"Patching property '{property.Name}' can not inherited from '{KnownTypeNames.ICommand}'");

					if (property.ContainsAttribute<NotPatchingPropertyAttribute>())
						errorsService.AddError($"Patching property '{property.Name}' can not have " +
							$"'{nameof(PatchingPropertyAttribute)}' and '{nameof(NotPatchingPropertyAttribute)}' at the same time");

					CheckCalledMethod(errorsService, viewModelType, patchingPropertyAttribute.CalledMethodNameBeforeGetProperty, property.Name);
					CheckCalledMethod(errorsService, viewModelType, patchingPropertyAttribute.CalledMethodNameBeforeSetProperty, property.Name);
					CheckCalledMethod(errorsService, viewModelType, patchingPropertyAttribute.CalledMethodNameAfterSuccessSetProperty, property.Name);
					CheckCalledMethod(errorsService, viewModelType, patchingPropertyAttribute.CalledMethodNameAfterSetProperty, property.Name);
				}

				if (property.IsInheritedFrom(commandType))
					continue;

				var connectPropertyToFieldAttributes = property.GetCastedAttributes<ConnectPropertyToFieldAttribute>().ToArray();
				if (!connectPropertyToFieldAttributes.Any())
					continue;

				if (property.ContainsAttribute<NotPatchingPropertyAttribute>())
					errorsService.AddError($"Patching property '{property.Name}' can not have '{nameof(NotPatchingPropertyAttribute)}', " +
						$"connection in '{nameof(ConnectPropertyToFieldAttribute)}' at property '{property.Name}'");

				foreach (var attribute in connectPropertyToFieldAttributes) {
					var field = viewModelType.GetField(attribute.ConnectingFieldName);

					if (field == null)
						errorsService.AddError($"Not found field with name '{attribute.ConnectingFieldName}', " +
							$"specified in '{nameof(ConnectPropertyToFieldAttribute)}' at property '{property.Name}'");
					else if (property.IsNot(field))
						errorsService.AddError($"Types do not match between property '{property.Name}' and field '{field.Name}', " +
							$"connection in '{nameof(ConnectPropertyToFieldAttribute)}' at property '{property.Name}'");
				}
			}

			foreach (var field in viewModelType.Fields.Select(field => field).Where(field => field.IsNotInheritedFrom(commandType))) {
				foreach (var attribute in field.GetCastedAttributes<ConnectFieldToPropertyAttribute>()) {
					var property = viewModelType.GetProperty(attribute.ConnectingPropertyName);

					if (property == null) {
						errorsService.AddError($"Not found property with name '{attribute.ConnectingPropertyName}', " +
							$"specified in '{nameof(ConnectFieldToPropertyAttribute)}' at field '{field.Name}'");
					}
					else {
						if (field.IsNot(property))
							errorsService.AddError($"Types do not match between field '{field.Name}' and property '{property.Name}', " +
								$"connection in '{nameof(ConnectFieldToPropertyAttribute)}' at field '{field.Name}'");

						if (property.ContainsAttribute<NotPatchingPropertyAttribute>())
							errorsService.AddError($"Patching property '{property.Name}' can not have '{nameof(NotPatchingPropertyAttribute)}', " +
								$"connection in '{nameof(ConnectFieldToPropertyAttribute)}' at field '{field.Name}'");
					}
				}
			}

			if (errorsService.HasErrors)
				throw new ViewModelPropertyPatchingException(errorsService);
		}
		private static void CheckCalledMethod(ErrorsService errorsService, IHasMethods viewModelType, string calledMethodName, string propertyName) {
			if (calledMethodName.IsNullOrEmpty())
				return;

			var calledMethodsBeforeGetProperty = viewModelType.GetMethods(calledMethodName).ToArray();
			switch (calledMethodsBeforeGetProperty.Length) {
				case 0:
					errorsService.AddError($"Not found called method with name '{calledMethodName}', " +
						$"specified in '{nameof(PatchingPropertyAttribute)}' at property '{propertyName}'");
					break;

				case 1:
					if (calledMethodsBeforeGetProperty.Single().ParameterTypes.Any())
						errorsService.AddError($"Called method '{calledMethodName}' can not have parameters, " +
							$"specified in '{nameof(PatchingPropertyAttribute)}' at property '{propertyName}'");
					break;

				default:
					errorsService.AddError($"Found several called methods with name '{calledMethodName}', " +
						$"specified in '{nameof(PatchingPropertyAttribute)}' at property '{propertyName}'");
					break;
			}
		}

		private ICommonProperty[] GetPatchingProperties(IHasProperties viewModelType, IHasType commandType, ViewModelPatchingType patchingType) {
			return viewModelType.Properties
				.Select(property => property)
				.Where(property => property.NotContainsAttribute<NotPatchingPropertyAttribute>() &&
					(!applicationPatcherWpfConfiguration.SkipConnectingByNameIfNameIsInvalid || nameRulesService.IsNameValid(property.Name, UseNameRulesFor.Property)) && (
						patchingType == ViewModelPatchingType.All && property.IsNotInheritedFrom(commandType) ||
						property.ContainsAttribute<PatchingPropertyAttribute>() ||
						property.ContainsAttribute<ConnectPropertyToFieldAttribute>()))
				.ToArray();
		}

		private void CheckPatchingPropertyNames(IEnumerable<ICommonProperty> patchingProperties) {
			var propertiesWithUseSearchByName = patchingProperties
				.Select(property => property)
				.Where(property => property.NotContainsAttribute<NotUseSearchByNameAttribute>() &&
					(applicationPatcherWpfConfiguration.ConnectByNameIfExistConnectAttribute || property.NotContainsAttribute<ConnectPropertyToFieldAttribute>()));

			var errorsService = new ErrorsService()
				.AddErrors(propertiesWithUseSearchByName
					.Where(property => !nameRulesService.IsNameValid(property.Name, UseNameRulesFor.Property))
					.Select(property => $"Not valid patching property name '{property.Name}'"));

			if (errorsService.HasErrors)
				throw new ViewModelPropertyPatchingException(errorsService);
		}

		private List<(ICommonProperty Property, List<ICommonField> Fields)> GetPatchingPropertyGroups(IEnumerable<ICommonProperty> patchingProperties, ICommonType viewModelType, IHasType commandType) {
			var patchingPropertyGroups = patchingProperties.Select(CreateEmptyPatchingPropertyGroup).ToList();

			AddGroupsByPropertyName(patchingPropertyGroups, viewModelType);
			AddGroupsFromConnectPropertyToFieldAttribute(patchingPropertyGroups, viewModelType, commandType);
			AddGroupsFromConnectFieldToPropertyAttribute(patchingPropertyGroups, viewModelType, commandType);

			return patchingPropertyGroups;
		}
		private static (ICommonProperty Property, List<ICommonField> Fields) CreateEmptyPatchingPropertyGroup(ICommonProperty property) {
			return (property, new List<ICommonField>());
		}
		private void AddGroupsByPropertyName(ICollection<(ICommonProperty Property, List<ICommonField> Fields)> patchingPropertyGroups, IHasFields viewModelType) {
			var propertiesWithUseSearchByName = patchingPropertyGroups.Select(group => group.Property)
				.Where(property => property.NotContainsAttribute<NotUseSearchByNameAttribute>() &&
					(applicationPatcherWpfConfiguration.ConnectByNameIfExistConnectAttribute || property.NotContainsAttribute<ConnectPropertyToFieldAttribute>()));

			foreach (var property in propertiesWithUseSearchByName) {
				if (viewModelType.TryGetField(nameRulesService.ConvertName(property.Name, UseNameRulesFor.Property, UseNameRulesFor.Field), out var field))
					patchingPropertyGroups.GetOrAdd(group => group.Property == property, () => CreateEmptyPatchingPropertyGroup(property)).Fields.AddIfNotContains(field);
			}
		}
		private static void AddGroupsFromConnectPropertyToFieldAttribute(ICollection<(ICommonProperty Property, List<ICommonField> Fields)> patchingPropertyGroups, ICommonType viewModelType, IHasType commandType) {
			foreach (var property in viewModelType.Properties.Where(p => p.IsNotInheritedFrom(commandType))) {
				foreach (var field in property.GetCastedAttributes<ConnectPropertyToFieldAttribute>().Select(attribute => viewModelType.GetField(attribute.ConnectingFieldName, true)))
					patchingPropertyGroups.GetOrAdd(group => group.Property == property, () => CreateEmptyPatchingPropertyGroup(property)).Fields.AddIfNotContains(field);
			}
		}
		private static void AddGroupsFromConnectFieldToPropertyAttribute(ICollection<(ICommonProperty Property, List<ICommonField> Fields)> patchingPropertyGroups, ICommonType viewModelType, IHasType commandType) {
			foreach (var field in viewModelType.Fields.Where(f => f.IsNotInheritedFrom(commandType))) {
				foreach (var property in field.GetCastedAttributes<ConnectFieldToPropertyAttribute>().Select(attribute => viewModelType.GetProperty(attribute.ConnectingPropertyName, true)))
					patchingPropertyGroups.GetOrAdd(group => group.Property == property, () => CreateEmptyPatchingPropertyGroup(property)).Fields.AddIfNotContains(field);
			}
		}

		private static void CheckPatchingPropertyGroups(IEnumerable<(ICommonProperty Property, List<ICommonField> Fields)> patchingPropertyGroups) {
			var errorsService = new ErrorsService();
			foreach (var group in patchingPropertyGroups) {
				switch (group.Fields.Count) {
					case 0:
						if (group.Property.ContainsAttribute<NotUseSearchByNameAttribute>())
							errorsService.AddError($"Not found field for property '{group.Property.Name}' when using '{nameof(NotUseSearchByNameAttribute)}'");
						break;

					case 1:
						var singleField = group.Fields.Single();

						if (group.Property.IsNot(singleField))
							errorsService.AddError($"Types do not match inside group: property '{group.Property.Name}', field '{singleField.Name}'");
						break;

					default:
						errorsService.AddError("Multi-connect property to field found: " +
							$"property '{group.Property.Name}', fields: {group.Fields.Select(field => $"'{field.Name}'").JoinToString(", ")}");
						break;
				}
			}

			if (errorsService.HasErrors)
				throw new ViewModelPropertyPatchingException(errorsService);
		}

		private static ICommonMethod GetCalledMethod(IHasMethods viewModelType, string calledMethodName) {
			return calledMethodName.IsNullOrEmpty() ? null : viewModelType.GetMethod(calledMethodName, true);
		}
	}
}