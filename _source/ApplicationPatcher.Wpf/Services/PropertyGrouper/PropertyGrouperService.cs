using System;
using System.Linq;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Core.Types.Base;
using ApplicationPatcher.Core.Types.Common;
using ApplicationPatcher.Wpf.Exceptions;
using ApplicationPatcher.Wpf.Types.Attributes;
using ApplicationPatcher.Wpf.Types.Enums;

namespace ApplicationPatcher.Wpf.Services.PropertyGrouper {
	public class PropertyGrouperService {
		public PropertyGroup[] GetGroups(CommonAssembly commonAssembly, CommonType viewModelType, ViewModelPatchingType patchingType) {
			CheckViewModel(commonAssembly, viewModelType);
			var patchingProperties = GetPatchingProperties(commonAssembly, viewModelType, patchingType);
			CheckPatchingPropertyNames(patchingProperties);

			throw new NotImplementedException();
		}

		private void CheckViewModel(IHasTypes commonAssembly, CommonType viewModelType) {
			var commandType = commonAssembly.GetCommonType(KnownTypeNames.ICommand, true);
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
					$"connection in {nameof(ConnectFieldToPropertyAttribute)} at field '{x.Field.Name}'"));

			errorsService.AddErrors(connectFieldToPropertyAttributes
				.Where(x => x.Property.ContainsAttribute<NotPatchingPropertyAttribute>())
				.Select(x => $"Patching property '{x.Property.Name}' can not have '{nameof(NotPatchingPropertyAttribute)}', " +
					$"connection in {nameof(ConnectFieldToPropertyAttribute)} at field '{x.Field.Name}'"));

			var connectPropertyToFieldAttributes = viewModelType.Properties
				.Where(property => property.IsNotInheritedFrom(commandType))
				.SelectMany(property => property.GetReflectionAttributes<ConnectPropertyToFieldAttribute>()
					.Select(attribute => new { Property = property, Attribute = attribute, Field = viewModelType.GetField(attribute.ConnectingFieldName) }))
				.ToArray();

			errorsService.AddErrors(connectPropertyToFieldAttributes
				.Where(x => x.Field == null)
				.Select(x => $"Not found field with name '{x.Attribute.ConnectingFieldName}', " +
					$"specified in '{nameof(ConnectPropertyToFieldAttribute)}' at property '{x.Property.Name}'"));

			errorsService.AddErrors(connectPropertyToFieldAttributes
				.Where(x => x.Field != null && x.Property.IsNot(x.Field))
				.Select(x => $"Types do not match between property '{x.Property.Name}' and field '{x.Field.Name}', " +
					$"connection in {nameof(ConnectPropertyToFieldAttribute)} at property '{x.Property.Name}'"));

			if (errorsService.HasErrors)
				throw new PropertyPatchingException(errorsService);
		}

		private CommonProperty[] GetPatchingProperties(IHasTypes commonAssembly, CommonType viewModelType, ViewModelPatchingType patchingType) {
			var commandType = commonAssembly.GetCommonType(KnownTypeNames.ICommand, true);
			
			switch (patchingType) {
				case ViewModelPatchingType.All:
					return viewModelType.Properties
						.Where(property => property.NotContainsAttribute<NotPatchingPropertyAttribute>() && (
							property.IsNotInheritedFrom(commandType) ||
							property.ContainsAttribute<PatchingPropertyAttribute>() ||
							property.ContainsAttribute<ConnectPropertyToFieldAttribute>()))
						.Union(viewModelType.Fields
							.Where(field => field.IsNotInheritedFrom(commandType))
							.SelectMany(field => field.GetReflectionAttributes<ConnectFieldToPropertyAttribute>())
							.Select(attribute => viewModelType.GetProperty(attribute.ConnectingPropertyName, true)))
						.ToArray();

				case ViewModelPatchingType.Selectively:
					return viewModelType.Properties
						.Where(property => property.NotContainsAttribute<NotPatchingPropertyAttribute>() && (
							property.ContainsAttribute<PatchingPropertyAttribute>() ||
							property.ContainsAttribute<ConnectPropertyToFieldAttribute>()))
						.Union(viewModelType.Fields
							.Where(field => field.IsNotInheritedFrom(commandType))
							.SelectMany(field => field.GetReflectionAttributes<ConnectFieldToPropertyAttribute>())
							.Select(attribute => viewModelType.GetProperty(attribute.ConnectingPropertyName, true)))
						.ToArray();

				default:
					throw new ArgumentOutOfRangeException(nameof(patchingType), patchingType, null);
			}
		}

		private void CheckPatchingPropertyNames(CommonProperty[] patchingProperties) {

		}
	}
}