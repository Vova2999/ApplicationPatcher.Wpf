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

namespace ApplicationPatcher.Wpf.Services.Groupers.Dependency {
	public class DependencyGrouperService {
		private readonly ApplicationPatcherWpfConfiguration applicationPatcherWpfConfiguration;
		private readonly NameRulesService nameRulesService;

		public DependencyGrouperService(ApplicationPatcherWpfConfiguration applicationPatcherWpfConfiguration, NameRulesService nameRulesService) {
			this.applicationPatcherWpfConfiguration = applicationPatcherWpfConfiguration;
			this.nameRulesService = nameRulesService;
		}

		public DependencyGroup[] GetGroups(ICommonAssembly assembly, ICommonType frameworkElementType, FrameworkElementPatchingType patchingType) {
			var dependencyPropertyType = assembly.GetCommonType(KnownTypeNames.DependencyProperty, true);
			CheckFrameworkElement(frameworkElementType, dependencyPropertyType);

			var patchingProperties = GetPatchingProperties(frameworkElementType, patchingType);
			CheckPatchingPropertyNames(patchingProperties);

			var patchingDependencyGroups = GetPatchingDependencyGroups(patchingProperties, frameworkElementType);
			CheckPatchingPropertyGroups(patchingDependencyGroups, dependencyPropertyType);

			return patchingDependencyGroups.Select(group => new DependencyGroup(group.Fields.SingleOrDefault(), group.Property)).ToArray();
		}

		private static void CheckFrameworkElement(ICommonType frameworkElementType, ICommonType dependencyPropertyType) {
			var errorsService = new ErrorsService();
			foreach (var property in frameworkElementType.Properties) {
				if (property.ContainsAttribute<PatchingPropertyAttribute>() && property.ContainsAttribute<NotPatchingPropertyAttribute>())
					errorsService.AddError($"Patching property '{property.Name}' can not have " +
						$"'{nameof(PatchingPropertyAttribute)}' and '{nameof(NotPatchingPropertyAttribute)}' at the same time");

				var connectPropertyToDependencyAttributes = property.GetCastedAttributes<ConnectPropertyToFieldAttribute>().ToArray();
				if (!connectPropertyToDependencyAttributes.Any())
					continue;

				if (property.ContainsAttribute<NotPatchingPropertyAttribute>())
					errorsService.AddError($"Patching property '{property.Name}' can not have '{nameof(NotPatchingPropertyAttribute)}', " +
						$"connection in '{nameof(ConnectPropertyToFieldAttribute)}' at property '{property.Name}'");

				foreach (var attribute in connectPropertyToDependencyAttributes) {
					var field = frameworkElementType.GetField(attribute.ConnectingFieldName);

					if (field == null) {
						errorsService.AddError($"Not found field with name '{attribute.ConnectingFieldName}', " +
							$"specified in '{nameof(ConnectPropertyToFieldAttribute)}' at property '{property.Name}'");
					}
					else {
						if (!field.MonoCecil.IsStatic)
							errorsService.AddError($"Patching field '{field.Name}' can not be non static, " +
								$"connection in '{nameof(ConnectPropertyToFieldAttribute)}' at property '{property.Name}'");

						if (field.IsNot(dependencyPropertyType))
							errorsService.AddError($"Patching field '{field.Name}' can not have " +
								$"'{field.Type.FullName}' type, allowable types: '{dependencyPropertyType.FullName}', " +
								$"connection in '{nameof(ConnectPropertyToFieldAttribute)}' at property '{property.Name}'");
					}
				}
			}

			foreach (var field in frameworkElementType.Fields) {
				var connectDependencyToPropertyAttributes = field.GetCastedAttributes<ConnectFieldToPropertyAttribute>().ToArray();
				if (!connectDependencyToPropertyAttributes.Any())
					continue;

				if (field.IsNot(dependencyPropertyType))
					errorsService.AddError($"Patching field '{field.Name}' can not have " +
						$"'{field.Type.FullName}' type, allowable types: '{dependencyPropertyType.FullName}', " +
						$"connection in '{nameof(ConnectFieldToPropertyAttribute)}' at field '{field.Name}'");

				foreach (var attribute in connectDependencyToPropertyAttributes) {
					var property = frameworkElementType.GetProperty(attribute.ConnectingPropertyName);

					if (property == null)
						errorsService.AddError($"Not found property with name '{attribute.ConnectingPropertyName}', " +
							$"specified in '{nameof(ConnectFieldToPropertyAttribute)}' at field '{field.Name}'");
					else if (property.ContainsAttribute<NotPatchingPropertyAttribute>())
						errorsService.AddError($"Patching property '{property.Name}' can not have '{nameof(NotPatchingPropertyAttribute)}', " +
							$"connection in '{nameof(ConnectFieldToPropertyAttribute)}' at field '{field.Name}'");
				}
			}

			if (errorsService.HasErrors)
				throw new FrameworkElementDependencyPatchingException(errorsService);
		}

		private ICommonProperty[] GetPatchingProperties(IHasProperties frameworkElementType, FrameworkElementPatchingType patchingType) {
			return frameworkElementType.Properties
				.Where(property => property.NotContainsAttribute<NotPatchingPropertyAttribute>() &&
					(!applicationPatcherWpfConfiguration.SkipConnectingByNameIfNameIsInvalid || nameRulesService.IsNameValid(property.Name, UseNameRulesFor.DependencyProperty)) && (
						patchingType == FrameworkElementPatchingType.All ||
						property.ContainsAttribute<PatchingPropertyAttribute>() ||
						property.ContainsAttribute<ConnectPropertyToFieldAttribute>()))
				.ToArray();
		}

		private void CheckPatchingPropertyNames(IEnumerable<ICommonProperty> patchingProperties) {
			var propertiesWithUseSearchByName = patchingProperties
				.Where(property => property.NotContainsAttribute<NotUseSearchByNameAttribute>() &&
					(applicationPatcherWpfConfiguration.ConnectByNameIfExistConnectAttribute || property.NotContainsAttribute<ConnectPropertyToFieldAttribute>()));

			var errorsService = new ErrorsService()
				.AddErrors(propertiesWithUseSearchByName
					.Where(property => !nameRulesService.IsNameValid(property.Name, UseNameRulesFor.DependencyProperty))
					.Select(property => $"Not valid patching property name '{property.Name}'"));

			if (errorsService.HasErrors)
				throw new FrameworkElementDependencyPatchingException(errorsService);
		}

		private List<(ICommonProperty Property, List<ICommonField> Fields)> GetPatchingDependencyGroups(IEnumerable<ICommonProperty> patchingProperties, ICommonType frameworkElementType) {
			var patchingDependencyGroups = patchingProperties.Select(CreateEmptyPatchingDependencyGroup).ToList();

			AddGroupsByPropertyName(patchingDependencyGroups, frameworkElementType);
			AddGroupsFromConnectPropertyToDependencyAttribute(patchingDependencyGroups, frameworkElementType);
			AddGroupsFromConnectDependencyToPropertyAttribute(patchingDependencyGroups, frameworkElementType);

			return patchingDependencyGroups;
		}
		private static (ICommonProperty Property, List<ICommonField> Fields) CreateEmptyPatchingDependencyGroup(ICommonProperty property) {
			return (property, new List<ICommonField>());
		}
		private void AddGroupsByPropertyName(ICollection<(ICommonProperty Property, List<ICommonField> Fields)> patchingDependencyGroups, IHasFields frameworkElementType) {
			var propertiesWithUseSearchByName = patchingDependencyGroups.Select(group => group.Property)
				.Where(property => property.NotContainsAttribute<NotUseSearchByNameAttribute>() &&
					(applicationPatcherWpfConfiguration.ConnectByNameIfExistConnectAttribute || property.NotContainsAttribute<ConnectPropertyToFieldAttribute>()));

			foreach (var property in propertiesWithUseSearchByName) {
				if (frameworkElementType.TryGetField(nameRulesService.ConvertName(property.Name, UseNameRulesFor.DependencyProperty, UseNameRulesFor.DependencyField), out var field))
					patchingDependencyGroups.GetOrAdd(group => group.Property == property, () => CreateEmptyPatchingDependencyGroup(property)).Fields.AddIfNotContains(field);
			}
		}
		private static void AddGroupsFromConnectPropertyToDependencyAttribute(ICollection<(ICommonProperty Property, List<ICommonField> Fields)> patchingDependencyGroups, ICommonType frameworkElementType) {
			foreach (var property in frameworkElementType.Properties) {
				foreach (var field in property.GetCastedAttributes<ConnectPropertyToFieldAttribute>().Select(attribute => frameworkElementType.GetField(attribute.ConnectingFieldName, true)))
					patchingDependencyGroups.GetOrAdd(group => group.Property == property, () => CreateEmptyPatchingDependencyGroup(property)).Fields.AddIfNotContains(field);
			}
		}
		private static void AddGroupsFromConnectDependencyToPropertyAttribute(ICollection<(ICommonProperty Property, List<ICommonField> Fields)> patchingDependencyGroups, ICommonType frameworkElementType) {
			foreach (var field in frameworkElementType.Fields) {
				foreach (var property in field.GetCastedAttributes<ConnectFieldToPropertyAttribute>().Select(attribute => frameworkElementType.GetProperty(attribute.ConnectingPropertyName, true)))
					patchingDependencyGroups.GetOrAdd(group => group.Property == property, () => CreateEmptyPatchingDependencyGroup(property)).Fields.AddIfNotContains(field);
			}
		}

		private static void CheckPatchingPropertyGroups(IEnumerable<(ICommonProperty Property, List<ICommonField> Fields)> patchingDependencyGroups, ICommonType dependencyPropertyType) {
			var errorsService = new ErrorsService();
			foreach (var group in patchingDependencyGroups) {
				switch (group.Fields.Count) {
					case 0:
						if (group.Property.ContainsAttribute<NotUseSearchByNameAttribute>())
							errorsService.AddError($"Not found field for property '{group.Property.Name}' when using '{nameof(NotUseSearchByNameAttribute)}'");
						break;

					case 1:
						var singleField = group.Fields.Single();

						if (!singleField.MonoCecil.IsStatic)
							errorsService.AddError($"Patching field '{singleField.Name}' can not be non static");

						if (singleField.IsNot(dependencyPropertyType))
							errorsService.AddError($"Patching field '{singleField.Name}' can not have " +
								$"'{singleField.Type.FullName}' type, allowable types: '{dependencyPropertyType.FullName}'");
						break;

					default:
						errorsService.AddError("Multi-connect property to field found: " +
							$"property '{group.Property.Name}', fields: {group.Fields.Select(field => $"'{field.Name}'").JoinToString(", ")}");
						break;
				}
			}

			if (errorsService.HasErrors)
				throw new FrameworkElementDependencyPatchingException(errorsService);
		}
	}
}