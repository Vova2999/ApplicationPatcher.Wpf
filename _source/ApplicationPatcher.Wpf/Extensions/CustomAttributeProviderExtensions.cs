using System.Linq;
using ApplicationPatcher.Core.Extensions;
using Mono.Cecil;

namespace ApplicationPatcher.Wpf.Extensions {
	public static class CustomAttributeProviderExtensions {
		public static void RemoveAttributes<TAttribute>(this ICustomAttributeProvider customAttributeProvider) {
			customAttributeProvider.CustomAttributes
				.Where(attribute => attribute.AttributeType.FullName == typeof(TAttribute).FullName)
				.ToArray()
				.ForEach(customAttributeProvider.CustomAttributes.Remove);
		}
	}
}