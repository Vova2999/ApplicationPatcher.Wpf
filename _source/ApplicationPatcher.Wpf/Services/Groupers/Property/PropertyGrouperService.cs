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
using ApplicationPatcher.Wpf.Types.Attributes.ViewModel;
using ApplicationPatcher.Wpf.Types.Enums;

namespace ApplicationPatcher.Wpf.Services.Groupers.Property {
	public class PropertyGrouperService {
		private readonly ApplicationPatcherWpfConfiguration applicationPatcherWpfConfiguration;
		private readonly NameRulesService nameRulesService;

		public PropertyGrouperService(ApplicationPatcherWpfConfiguration applicationPatcherWpfConfiguration, NameRulesService nameRulesService) {
			this.applicationPatcherWpfConfiguration = applicationPatcherWpfConfiguration;
			this.nameRulesService = nameRulesService;
		}

		public PropertyGroup[] GetGroups(CommonAssembly assembly, CommonType viewModelType, ViewModelPatchingType patchingType) {
			var commandType = assembly.GetCommonType(KnownTypeNames.ICommand, true);
			CheckViewModel(commandType, viewModelType);

			var patchingProperties = GetPatchingProperties(viewModelType, commandType, patchingType);
			CheckPatchingPropertyNames(patchingProperties);

			var patchingPropertyGroups = GetPatchingPropertyGroups(patchingProperties, viewModelType, commandType);
			CheckPatchingPropertyGroups(patchingPropertyGroups);

			return patchingPropertyGroups.Select(group => new PropertyGroup(group.Fields.SingleOrDefault(), group.Property)).ToArray();
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

			var connectPropertyToFieldAttributes = viewModelType.Properties
				.Where(property => property.IsNotInheritedFrom(commandType))
				.SelectMany(property => property.GetReflectionAttributes<ConnectPropertyToFieldAttribute>()
					.Select(attribute => new { Property = property, Attribute = attribute, Field = viewModelType.GetField(attribute.ConnectingFieldName) }))
				.ToArray();

			errorsService
				.AddErrors(connectPropertyToFieldAttributes
					.Where(x => x.Property.ContainsAttribute<NotPatchingPropertyAttribute>())
					.Select(x => $"Patching property '{x.Property.Name}' can not have '{nameof(NotPatchingPropertyAttribute)}', " +
						$"connection in '{nameof(ConnectPropertyToFieldAttribute)}' at property '{x.Property.Name}'"))
				.AddErrors(connectPropertyToFieldAttributes
					.Where(x => x.Field == null)
					.Select(x => $"Not found field with name '{x.Attribute.ConnectingFieldName}', " +
						$"specified in '{nameof(ConnectPropertyToFieldAttribute)}' at property '{x.Property.Name}'"))
				.AddErrors(connectPropertyToFieldAttributes
					.Where(x => x.Field != null && x.Property.IsNot(x.Field))
					.Select(x => $"Types do not match between property '{x.Property.Name}' and field '{x.Field.Name}', " +
						$"connection in '{nameof(ConnectPropertyToFieldAttribute)}' at property '{x.Property.Name}'"));

			var connectFieldToPropertyAttributes = viewModelType.Fields
				.Where(field => field.IsNotInheritedFrom(commandType))
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
						$"connection in '{nameof(ConnectFieldToPropertyAttribute)}' at field '{x.Field.Name}'"))
				.AddErrors(connectFieldToPropertyAttributes
					.Where(x => x.Property != null && x.Property.ContainsAttribute<NotPatchingPropertyAttribute>())
					.Select(x => $"Patching property '{x.Property.Name}' can not have '{nameof(NotPatchingPropertyAttribute)}', " +
						$"connection in '{nameof(ConnectFieldToPropertyAttribute)}' at field '{x.Field.Name}'"));

			if (errorsService.HasErrors)
				throw new ViewModelPropertyPatchingException(errorsService);
		}

		private CommonProperty[] GetPatchingProperties(IHasProperties viewModelType, IHasType commandType, ViewModelPatchingType patchingType) {
			return viewModelType.Properties
				.Where(property => property.NotContainsAttribute<NotPatchingPropertyAttribute>() &&
					(!applicationPatcherWpfConfiguration.SkipConnectingByNameIfNameIsInvalid || nameRulesService.IsNameValid(property.Name, UseNameRulesFor.Property)) && (
						patchingType == ViewModelPatchingType.All && property.IsNotInheritedFrom(commandType) ||
						property.ContainsAttribute<PatchingPropertyAttribute>() ||
						property.ContainsAttribute<ConnectPropertyToFieldAttribute>()))
				.ToArray();
		}

		private void CheckPatchingPropertyNames(IEnumerable<CommonProperty> patchingProperties) {
			var propertiesWithUseSearchByName = patchingProperties
				.Where(property => property.NotContainsAttribute<NotUseSearchByNameAttribute>() &&
					(applicationPatcherWpfConfiguration.ConnectByNameIfExsistConnectAttribute || property.NotContainsAttribute<ConnectPropertyToFieldAttribute>()))
				.ToArray();

			var errorsService = new ErrorsService()
				.AddErrors(propertiesWithUseSearchByName
					.Where(property => !nameRulesService.IsNameValid(property.Name, UseNameRulesFor.Property))
					.Select(property => $"Not valid patching property name '{property.Name}'"));

			if (errorsService.HasErrors)
				throw new ViewModelPropertyPatchingException(errorsService);
		}

		private List<(CommonProperty Property, List<CommonField> Fields)> GetPatchingPropertyGroups(IEnumerable<CommonProperty> patchingProperties, CommonType viewModelType, IHasType commandType) {
			var patchingPropertyGroups = patchingProperties.Select(CreateEmptyPatchingPropertyGroup).ToList();

			AddGroupsByPropertyName(patchingPropertyGroups, viewModelType);
			AddGroupsFromConnectPropertyToFieldAttribute(patchingPropertyGroups, viewModelType, commandType);
			AddGroupsFromConnectFieldToPropertyAttribute(patchingPropertyGroups, viewModelType, commandType);

			return patchingPropertyGroups;
		}
		private static (CommonProperty Property, List<CommonField> Fields) CreateEmptyPatchingPropertyGroup(CommonProperty property) {
			return (property, new List<CommonField>());
		}
		private void AddGroupsByPropertyName(ICollection<(CommonProperty Property, List<CommonField> Fields)> patchingPropertyGroups, IHasFields viewModelType) {
			var propertiesWithUseSearchByName = patchingPropertyGroups.Select(group => group.Property)
				.Where(property => property.NotContainsAttribute<NotUseSearchByNameAttribute>() &&
					(applicationPatcherWpfConfiguration.ConnectByNameIfExsistConnectAttribute || property.NotContainsAttribute<ConnectPropertyToFieldAttribute>()))
				.ToArray();

			foreach (var property in propertiesWithUseSearchByName) {
				if (viewModelType.TryGetField(nameRulesService.ConvertName(property.Name, UseNameRulesFor.Property, UseNameRulesFor.Field), out var field))
					patchingPropertyGroups.GetOrAdd(group => group.Property == property, () => CreateEmptyPatchingPropertyGroup(property)).Fields.AddIfNotContains(field);
			}
		}
		private static void AddGroupsFromConnectPropertyToFieldAttribute(ICollection<(CommonProperty Property, List<CommonField> Fields)> patchingPropertyGroups, CommonType viewModelType, IHasType commandType) {
			foreach (var property in viewModelType.Properties.Where(p => p.IsNotInheritedFrom(commandType))) {
				foreach (var field in property.GetReflectionAttributes<ConnectPropertyToFieldAttribute>().Select(attribute => viewModelType.GetField(attribute.ConnectingFieldName, true)))
					patchingPropertyGroups.GetOrAdd(group => group.Property == property, () => CreateEmptyPatchingPropertyGroup(property)).Fields.AddIfNotContains(field);
			}
		}
		private static void AddGroupsFromConnectFieldToPropertyAttribute(ICollection<(CommonProperty Property, List<CommonField> Fields)> patchingPropertyGroups, CommonType viewModelType, IHasType commandType) {
			foreach (var field in viewModelType.Fields.Where(f => f.IsNotInheritedFrom(commandType))) {
				foreach (var property in field.GetReflectionAttributes<ConnectFieldToPropertyAttribute>().Select(attribute => viewModelType.GetProperty(attribute.ConnectingPropertyName, true)))
					patchingPropertyGroups.GetOrAdd(group => group.Property == property, () => CreateEmptyPatchingPropertyGroup(property)).Fields.AddIfNotContains(field);
			}
		}

		private static void CheckPatchingPropertyGroups(IReadOnlyCollection<(CommonProperty Property, List<CommonField> Fields)> patchingPropertyGroups) {
			var errorsService = new ErrorsService()
				.AddErrors(patchingPropertyGroups
					.Where(group => group.Fields.Count > 1)
					.Select(group => "Multi-connect property to field found: " +
						$"property '{group.Property.Name}', fields: {group.Fields.Select(field => $"'{field.Name}'").JoinToString(", ")}"))
				.AddErrors(patchingPropertyGroups
					.Where(group => group.Fields.Count == 1 && group.Property.IsNot(group.Fields.Single()))
					.Select(group => $"Types do not match inside group: property '{group.Property.Name}', field '{group.Fields.Single().Name}'"))
				.AddErrors(patchingPropertyGroups
					.Where(group => !group.Fields.Any() && group.Property.ContainsAttribute<NotUseSearchByNameAttribute>())
					.Select(group => $"Not found field for property '{group.Property.Name}' when using '{nameof(NotUseSearchByNameAttribute)}'"));

			if (errorsService.HasErrors)
				throw new ViewModelPropertyPatchingException(errorsService);
		}
	}
}