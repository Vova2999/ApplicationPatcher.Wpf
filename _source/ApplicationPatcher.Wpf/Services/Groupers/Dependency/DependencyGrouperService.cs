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
using ApplicationPatcher.Wpf.Types.Attributes.FrameworkElement;
using ApplicationPatcher.Wpf.Types.Enums;

namespace ApplicationPatcher.Wpf.Services.Groupers.Dependency {
	public class DependencyGrouperService {
		private readonly ApplicationPatcherWpfConfiguration applicationPatcherWpfConfiguration;
		private readonly NameRulesService nameRulesService;

		public DependencyGrouperService(ApplicationPatcherWpfConfiguration applicationPatcherWpfConfiguration, NameRulesService nameRulesService) {
			this.applicationPatcherWpfConfiguration = applicationPatcherWpfConfiguration;
			this.nameRulesService = nameRulesService;
		}

		public DependencyGroup[] GetGroups(CommonAssembly assembly, CommonType frameworkElementType, FrameworkElementPatchingType patchingType) {
			var dependencyPropertyType = assembly.GetCommonType(KnownTypeNames.DependencyProperty, true);
			CheckFrameworkElement(frameworkElementType, dependencyPropertyType);

			var patchingProperties = GetPatchingProperties(frameworkElementType, patchingType);
			CheckPatchingPropertyNames(patchingProperties);

			var patchingDependencyGroups = GetPatchingDependencyGroups(patchingProperties, frameworkElementType);
			CheckPatchingPropertyGroups(patchingDependencyGroups, dependencyPropertyType);

			return patchingDependencyGroups.Select(group => new DependencyGroup(group.Fields.SingleOrDefault(), group.Property)).ToArray();
		}

		private static void CheckFrameworkElement(CommonType frameworkElementType, CommonType dependencyPropertyType) {
			var propertiesWithPatchingDependencyPropertyAttribute = frameworkElementType.Properties.Where(property => property.ContainsAttribute<PatchingDependencyPropertyAttribute>()).ToArray();

			var errorsService = new ErrorsService()
				.AddErrors(propertiesWithPatchingDependencyPropertyAttribute
					.Where(property => property.ContainsAttribute<NotPatchingDependencyPropertyAttribute>())
					.Select(property => $"Patching property '{property.Name}' can not have " +
						$"'{nameof(PatchingDependencyPropertyAttribute)}' and '{nameof(NotPatchingDependencyPropertyAttribute)}' at the same time"));

			var connectPropertyToDependencyAttributes = frameworkElementType.Properties
				.SelectMany(property => property.GetReflectionAttributes<ConnectPropertyToDependencyAttribute>()
					.Select(attribute => new { Property = property, Attribute = attribute, Field = frameworkElementType.GetField(attribute.ConnectingDependencyName) }))
				.ToArray();

			errorsService
				.AddErrors(connectPropertyToDependencyAttributes
					.Where(x => x.Property.ContainsAttribute<NotPatchingDependencyPropertyAttribute>())
					.Select(x => $"Patching property '{x.Property.Name}' can not have '{nameof(NotPatchingDependencyPropertyAttribute)}', " +
						$"connection in '{nameof(ConnectPropertyToDependencyAttribute)}' at property '{x.Property.Name}'"))
				.AddErrors(connectPropertyToDependencyAttributes
					.Where(x => x.Field == null)
					.Select(x => $"Not found dependency field with name '{x.Attribute.ConnectingDependencyName}', " +
						$"specified in '{nameof(ConnectPropertyToDependencyAttribute)}' at property '{x.Property.Name}'"))
				.AddErrors(connectPropertyToDependencyAttributes
					.Where(x => x.Field != null && !x.Field.MonoCecil.IsStatic)
					.Select(x => $"Patching dependency field '{x.Field.Name}' can not be non static, " +
						$"connection in '{nameof(ConnectPropertyToDependencyAttribute)}' at property '{x.Property.Name}'"))
				.AddErrors(connectPropertyToDependencyAttributes
					.Where(x => x.Field != null && x.Field.IsNot(dependencyPropertyType))
					.Select(x => $"Patching dependency field '{x.Field.Name}' can not have " +
						$"'{x.Field.Type.FullName}' type, allowable types: '{dependencyPropertyType.FullName}', " +
						$"connection in '{nameof(ConnectPropertyToDependencyAttribute)}' at property '{x.Property.Name}'"));

			var connectDependencyToPropertyAttributes = frameworkElementType.Fields
				.SelectMany(field => field.GetReflectionAttributes<ConnectDependencyToPropertyAttribute>()
					.Select(attribute => new { Field = field, Attribute = attribute, Property = frameworkElementType.GetProperty(attribute.ConnectingPropertyName) }))
				.ToArray();

			errorsService
				.AddErrors(connectDependencyToPropertyAttributes
					.Where(x => x.Property == null)
					.Select(x => $"Not found property with name '{x.Attribute.ConnectingPropertyName}', " +
						$"specified in '{nameof(ConnectDependencyToPropertyAttribute)}' at dependency field '{x.Field.Name}'"))
				.AddErrors(connectDependencyToPropertyAttributes
					.Where(x => x.Property != null && x.Property.ContainsAttribute<NotPatchingDependencyPropertyAttribute>())
					.Select(x => $"Patching property '{x.Property.Name}' can not have '{nameof(NotPatchingDependencyPropertyAttribute)}', " +
						$"connection in '{nameof(ConnectDependencyToPropertyAttribute)}' at dependency field '{x.Field.Name}'"))
				.AddErrors(connectDependencyToPropertyAttributes
					.Where(x => x.Field != null && x.Field.IsNot(dependencyPropertyType))
					.Select(x => $"Patching dependency field '{x.Field.Name}' can not have " +
						$"'{x.Field.Type.FullName}' type, allowable types: '{dependencyPropertyType.FullName}', " +
						$"connection in '{nameof(ConnectDependencyToPropertyAttribute)}' at dependency field '{x.Field.Name}'"));

			if (errorsService.HasErrors)
				throw new FrameworkElementDependencyPatchingException(errorsService);
		}

		private CommonProperty[] GetPatchingProperties(IHasProperties frameworkElementType, FrameworkElementPatchingType patchingType) {
			return frameworkElementType.Properties
				.Where(property => property.NotContainsAttribute<NotPatchingDependencyPropertyAttribute>() &&
					(!applicationPatcherWpfConfiguration.SkipConnectingByNameIfNameIsInvalid || nameRulesService.IsNameValid(property.Name, UseNameRulesFor.DependencyProperty)) && (
						patchingType == FrameworkElementPatchingType.All ||
						property.ContainsAttribute<PatchingDependencyPropertyAttribute>() ||
						property.ContainsAttribute<ConnectPropertyToDependencyAttribute>()))
				.ToArray();
		}

		private void CheckPatchingPropertyNames(IEnumerable<CommonProperty> patchingProperties) {
			var propertiesWithUseSearchByName = patchingProperties
				.Where(property => property.NotContainsAttribute<NotUseSearchByNameAttribute>() &&
					(applicationPatcherWpfConfiguration.ConnectByNameIfExsistConnectAttribute || property.NotContainsAttribute<ConnectPropertyToDependencyAttribute>()))
				.ToArray();

			var errorsService = new ErrorsService()
				.AddErrors(propertiesWithUseSearchByName
					.Where(property => !nameRulesService.IsNameValid(property.Name, UseNameRulesFor.DependencyProperty))
					.Select(property => $"Not valid patching property name '{property.Name}'"));

			if (errorsService.HasErrors)
				throw new FrameworkElementDependencyPatchingException(errorsService);
		}

		private List<(CommonProperty Property, List<CommonField> Fields)> GetPatchingDependencyGroups(IEnumerable<CommonProperty> patchingProperties, CommonType frameworkElementType) {
			var patchingDependencyGroups = patchingProperties.Select(CreateEmptyPatchingDependencyGroup).ToList();

			AddGroupsByPropertyName(patchingDependencyGroups, frameworkElementType);
			AddGroupsFromConnectPropertyToDependencyAttribute(patchingDependencyGroups, frameworkElementType);
			AddGroupsFromConnectDependencyToPropertyAttribute(patchingDependencyGroups, frameworkElementType);

			return patchingDependencyGroups;
		}
		private static (CommonProperty Property, List<CommonField> Fields) CreateEmptyPatchingDependencyGroup(CommonProperty property) {
			return (property, new List<CommonField>());
		}
		private void AddGroupsByPropertyName(ICollection<(CommonProperty Property, List<CommonField> Fields)> patchingDependencyGroups, IHasFields frameworkElementType) {
			var propertiesWithUseSearchByName = patchingDependencyGroups.Select(group => group.Property)
				.Where(property => property.NotContainsAttribute<NotUseSearchByNameAttribute>() &&
					(applicationPatcherWpfConfiguration.ConnectByNameIfExsistConnectAttribute || property.NotContainsAttribute<ConnectPropertyToDependencyAttribute>()))
				.ToArray();

			foreach (var property in propertiesWithUseSearchByName) {
				if (frameworkElementType.TryGetField(nameRulesService.ConvertName(property.Name, UseNameRulesFor.DependencyProperty, UseNameRulesFor.DependencyField), out var field))
					patchingDependencyGroups.GetOrAdd(group => group.Property == property, () => CreateEmptyPatchingDependencyGroup(property)).Fields.AddIfNotContains(field);
			}
		}
		private static void AddGroupsFromConnectPropertyToDependencyAttribute(ICollection<(CommonProperty Property, List<CommonField> Fields)> patchingDependencyGroups, CommonType frameworkElementType) {
			foreach (var property in frameworkElementType.Properties) {
				foreach (var field in property.GetReflectionAttributes<ConnectPropertyToDependencyAttribute>().Select(attribute => frameworkElementType.GetField(attribute.ConnectingDependencyName, true)))
					patchingDependencyGroups.GetOrAdd(group => group.Property == property, () => CreateEmptyPatchingDependencyGroup(property)).Fields.AddIfNotContains(field);
			}
		}
		private static void AddGroupsFromConnectDependencyToPropertyAttribute(ICollection<(CommonProperty Property, List<CommonField> Fields)> patchingDependencyGroups, CommonType frameworkElementType) {
			foreach (var field in frameworkElementType.Fields) {
				foreach (var property in field.GetReflectionAttributes<ConnectDependencyToPropertyAttribute>().Select(attribute => frameworkElementType.GetProperty(attribute.ConnectingPropertyName, true)))
					patchingDependencyGroups.GetOrAdd(group => group.Property == property, () => CreateEmptyPatchingDependencyGroup(property)).Fields.AddIfNotContains(field);
			}
		}

		private static void CheckPatchingPropertyGroups(IReadOnlyCollection<(CommonProperty Property, List<CommonField> Fields)> patchingDependencyGroups, CommonType dependencyPropertyType) {
			var errorsService = new ErrorsService()
				.AddErrors(patchingDependencyGroups
					.Where(group => group.Fields.Count > 1)
					.Select(group => "Multi-connect property to dependency field found: " +
						$"property '{group.Property.Name}', dependency fields: {group.Fields.Select(field => $"'{field.Name}'").JoinToString(", ")}"))
				.AddErrors(patchingDependencyGroups
					.Where(group => group.Fields.Count == 1 && !group.Fields.Single().MonoCecil.IsStatic)
					.Select(group => $"Patching dependency field '{group.Fields.Single().Name}' can not be non static"))
				.AddErrors(patchingDependencyGroups
					.Where(group => group.Fields.Count == 1 && group.Fields.Single().IsNot(dependencyPropertyType))
					.Select(group => $"Patching dependency field '{group.Fields.Single().Name}' can not have " +
						$"'{group.Fields.Single().Type.FullName}' type, allowable types: '{dependencyPropertyType.FullName}'"))
				.AddErrors(patchingDependencyGroups
					.Where(group => !group.Fields.Any() && group.Property.ContainsAttribute<NotUseSearchByNameAttribute>())
					.Select(group => $"Not found dependency field for property '{group.Property.Name}' when using '{nameof(NotUseSearchByNameAttribute)}'"));

			if (errorsService.HasErrors)
				throw new FrameworkElementDependencyPatchingException(errorsService);
		}
	}
}