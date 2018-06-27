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

namespace ApplicationPatcher.Wpf.Services.PropertyGrouper {
	public class PropertyGrouperService {
		private readonly ApplicationPatcherWpfConfiguration applicationPatcherWpfConfiguration;
		private readonly NameRulesService nameRulesService;

		public PropertyGrouperService(ApplicationPatcherWpfConfiguration applicationPatcherWpfConfiguration, NameRulesService nameRulesService) {
			this.applicationPatcherWpfConfiguration = applicationPatcherWpfConfiguration;
			this.nameRulesService = nameRulesService;
		}

		public PropertyGroup[] GetGroups(IHasTypes commonAssembly, CommonType viewModelType, ViewModelPatchingType patchingType) {
			var commandType = commonAssembly.GetCommonType(KnownTypeNames.ICommand, true);
			CheckViewModel(commandType, viewModelType);

			var patchingProperties = GetPatchingProperties(viewModelType, commandType, patchingType);
			CheckPatchingPropertyNames(patchingProperties);

			var propertyToFieldsGroups = GetPropertyToFieldsGroups(patchingProperties, viewModelType, commandType);
			CheckPropertyToFieldsGroups(propertyToFieldsGroups);

			return propertyToFieldsGroups.Select(group => new PropertyGroup(group.Value.SingleOrDefault(), group.Key)).ToArray();
		}

		private static void CheckViewModel(IHasType commandType, CommonType viewModelType) {
			var propertiesWithPatchingPropertyAttribute = viewModelType.Properties.Where(property => property.ContainsAttribute<PatchingPropertyAttribute>()).ToArray();

			var errorsService = new ErrorsService()
				.AddErrors(propertiesWithPatchingPropertyAttribute
					.Where(property => property.IsInheritedFrom(commandType))
					.Select(property => $"Patching property '{property.Name}' can not inherited from '{KnownTypeNames.ICommand}'"))
				.AddErrors(propertiesWithPatchingPropertyAttribute
					.Where(property => property.ContainsAttribute<NotPatchingPropertyAttribute>())
					.Select(property => $"Patching property '{property.Name}' can not have " +
						$"'{nameof(PatchingPropertyAttribute)}' and '{nameof(NotPatchingPropertyAttribute)}' at the same time"));

			var connectFieldToPropertyAttributes = viewModelType.Fields
				.Where(field => field.IsNotInheritedFrom(commandType))
				.SelectMany(field => field.GetReflectionAttributes<ConnectFieldToPropertyAttribute>()
					.Select(attribute => new { Field = field, Attribute = attribute, Property = viewModelType.GetProperty(attribute.ConnectingPropertyName) }))
				.ToArray();

			errorsService.AddErrors(connectFieldToPropertyAttributes
				.Where(x => x.Property == null)
				.Select(x => $"Not found property with name '{x.Attribute.ConnectingPropertyName}', " +
					$"specified in '{nameof(ConnectFieldToPropertyAttribute)}' at field '{x.Field.Name}'"));

			errorsService.AddErrors(connectFieldToPropertyAttributes
				.Where(x => x.Property != null && x.Field.IsNot(x.Property))
				.Select(x => $"Types do not match between field '{x.Field.Name}' and property '{x.Property.Name}', " +
					$"connection in '{nameof(ConnectFieldToPropertyAttribute)}' at field '{x.Field.Name}'"));

			errorsService.AddErrors(connectFieldToPropertyAttributes
				.Where(x => x.Property != null && x.Property.ContainsAttribute<NotPatchingPropertyAttribute>())
				.Select(x => $"Patching property '{x.Property.Name}' can not have '{nameof(NotPatchingPropertyAttribute)}', " +
					$"connection in '{nameof(ConnectFieldToPropertyAttribute)}' at field '{x.Field.Name}'"));

			var connectPropertyToFieldAttributes = viewModelType.Properties
				.Where(property => property.IsNotInheritedFrom(commandType))
				.SelectMany(property => property.GetReflectionAttributes<ConnectPropertyToFieldAttribute>()
					.Select(attribute => new { Property = property, Attribute = attribute, Field = viewModelType.GetField(attribute.ConnectingFieldName) }))
				.ToArray();

			errorsService.AddErrors(connectPropertyToFieldAttributes
				.Where(x => x.Property.ContainsAttribute<NotPatchingPropertyAttribute>())
				.Select(x => $"Patching property '{x.Property.Name}' can not have '{nameof(NotPatchingPropertyAttribute)}', " +
					$"connection in '{nameof(ConnectPropertyToFieldAttribute)}' at property '{x.Property.Name}'"));

			errorsService.AddErrors(connectPropertyToFieldAttributes
				.Where(x => x.Field == null)
				.Select(x => $"Not found field with name '{x.Attribute.ConnectingFieldName}', " +
					$"specified in '{nameof(ConnectPropertyToFieldAttribute)}' at property '{x.Property.Name}'"));

			errorsService.AddErrors(connectPropertyToFieldAttributes
				.Where(x => x.Field != null && x.Property.IsNot(x.Field))
				.Select(x => $"Types do not match between property '{x.Property.Name}' and field '{x.Field.Name}', " +
					$"connection in '{nameof(ConnectPropertyToFieldAttribute)}' at property '{x.Property.Name}'"));

			if (errorsService.HasErrors)
				throw new PropertyPatchingException(errorsService);
		}

		private static CommonProperty[] GetPatchingProperties(IHasProperties viewModelType, IHasType commandType, ViewModelPatchingType patchingType) {
			return viewModelType.Properties
				.Where(property => property.NotContainsAttribute<NotPatchingPropertyAttribute>() && (
					patchingType == ViewModelPatchingType.All && property.IsNotInheritedFrom(commandType) ||
					property.ContainsAttribute<PatchingPropertyAttribute>() ||
					property.ContainsAttribute<ConnectPropertyToFieldAttribute>()))
				.ToArray();
		}

		private void CheckPatchingPropertyNames(IEnumerable<CommonProperty> patchingProperties) {
			var errorsService = new ErrorsService();
			var propertiesWithUseSearchByName = patchingProperties
				.Where(property => property.NotContainsAttribute<NotUseSearchByNameAttribute>() &&
					(applicationPatcherWpfConfiguration.ConnectByNameIfExsistConnectAttribute || property.NotContainsAttribute<ConnectPropertyToFieldAttribute>()));

			errorsService.AddErrors(propertiesWithUseSearchByName
				.Where(property => !nameRulesService.IsNameValid(property.Name, UseNameRulesFor.Property))
				.Select(property => $"Not valid patching property name '{property.Name}'"));

			if (errorsService.HasErrors)
				throw new PropertyPatchingException(errorsService);
		}

		private IDictionary<CommonProperty, List<CommonField>> GetPropertyToFieldsGroups(IEnumerable<CommonProperty> patchingProperties, CommonType viewModelType, IHasType commandType) {
			var propertyToFieldsGroups = patchingProperties.ToDictionary(property => property, property => new List<CommonField>());

			AddFieldsByName(propertyToFieldsGroups, viewModelType);
			AddFieldsFromConnectPropertyToFieldAttribute(propertyToFieldsGroups, viewModelType);
			AddFieldsFromConnectFieldToPropertyAttribute(propertyToFieldsGroups, viewModelType, commandType);
			return propertyToFieldsGroups;
		}
		private void AddFieldsByName(IDictionary<CommonProperty, List<CommonField>> propertyToFieldsGroups, IHasFields viewModelType) {
			var propertiesWithUseSearchByName = propertyToFieldsGroups.Keys
				.Where(property => property.NotContainsAttribute<NotUseSearchByNameAttribute>() &&
					(applicationPatcherWpfConfiguration.ConnectByNameIfExsistConnectAttribute || property.NotContainsAttribute<ConnectPropertyToFieldAttribute>()));

			foreach (var property in propertiesWithUseSearchByName) {
				var fieldName = nameRulesService.ConvertName(property.Name, UseNameRulesFor.Property, UseNameRulesFor.Field);
				if (viewModelType.TryGetField(fieldName, out var field))
					propertyToFieldsGroups.GetOrCreate(property, () => new List<CommonField>()).AddIfNotContains(field);
			}
		}
		private static void AddFieldsFromConnectPropertyToFieldAttribute(IDictionary<CommonProperty, List<CommonField>> propertyToFieldsGroups, IHasFields viewModelType) {
			foreach (var property in propertyToFieldsGroups.Keys) {
				foreach (var attribute in property.GetReflectionAttributes<ConnectPropertyToFieldAttribute>())
					propertyToFieldsGroups.GetOrCreate(property, () => new List<CommonField>()).AddIfNotContains(viewModelType.GetField(attribute.ConnectingFieldName, true));
			}
		}
		private static void AddFieldsFromConnectFieldToPropertyAttribute(IDictionary<CommonProperty, List<CommonField>> propertyToFieldsGroups, CommonType viewModelType, IHasType commandType) {
			foreach (var field in viewModelType.Fields.Where(f => f.IsNotInheritedFrom(commandType))) {
				foreach (var attribute in field.GetReflectionAttributes<ConnectFieldToPropertyAttribute>())
					propertyToFieldsGroups.GetOrCreate(viewModelType.GetProperty(attribute.ConnectingPropertyName, true), () => new List<CommonField>()).AddIfNotContains(field);
			}
		}

		private static void CheckPropertyToFieldsGroups(IDictionary<CommonProperty, List<CommonField>> propertyToFieldsGroups) {
			var errorsService = new ErrorsService();

			errorsService.AddErrors(propertyToFieldsGroups
				.Where(group => !group.Value.Any() && group.Key.ContainsAttribute<NotUseSearchByNameAttribute>())
				.Select(group => $"Not found field for property '{group.Key.Name}' when using '{nameof(NotUseSearchByNameAttribute)}'"));

			errorsService.AddErrors(propertyToFieldsGroups
				.Where(group => group.Value.Count > 1)
				.Select(group => "Multi-connect property to field found: " +
					$"property '{group.Key.Name}', fields: {group.Value.Select(field => $"'{field.Name}'").JoinToString(", ")}"));

			errorsService.AddErrors(propertyToFieldsGroups
				.SelectMany(group => group.Value
					.Where(field => group.Key.IsNot(field))
					.Select(field => $"Types do not match inside group: property '{group.Key.Name}', field '{field.Name}'")));

			if (errorsService.HasErrors)
				throw new PropertyPatchingException(errorsService);
		}
	}
}