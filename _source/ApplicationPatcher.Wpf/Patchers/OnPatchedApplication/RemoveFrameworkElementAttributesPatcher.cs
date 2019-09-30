using System.Linq;
using ApplicationPatcher.Core;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Core.Logs;
using ApplicationPatcher.Core.Patchers;
using ApplicationPatcher.Core.Types.CommonInterfaces;
using ApplicationPatcher.Wpf.Extensions;
using ApplicationPatcher.Wpf.Types.Attributes.Connect;
using ApplicationPatcher.Wpf.Types.Attributes.FrameworkElement;
using ApplicationPatcher.Wpf.Types.Attributes.SelectPatching;
using JetBrains.Annotations;

namespace ApplicationPatcher.Wpf.Patchers.OnPatchedApplication {
	[UsedImplicitly]
	public class RemoveFrameworkElementAttributesPatcher : PatcherOnPatchedApplication {
		private readonly ILog log;

		public RemoveFrameworkElementAttributesPatcher() {
			log = Log.For(this);
		}

		public override PatchResult Patch(ICommonAssembly assembly) {
			log.Info("Remove framework element attributes...");

			RemoveAttributesFromFrameworkElements(assembly);

			log.Info("Framework element attributes was removed");
			return PatchResult.Continue;
		}

		private static void RemoveAttributesFromFrameworkElements(ICommonAssembly assembly) {
			var frameworkElementBaseType = assembly.GetCommonType(KnownTypeNames.FrameworkElement, true);
			var frameworkElementTypes = assembly.GetInheritanceCommonTypesFromThisAssembly(frameworkElementBaseType).ToArray();

			foreach (var frameworkElementType in frameworkElementTypes.Select(type => type.Load())) {
				frameworkElementType.MonoCecil.RemoveAttributes<PatchingFrameworkElementAttribute>();
				frameworkElementType.MonoCecil.RemoveAttributes<NotPatchingFrameworkElementAttribute>();

				foreach (var property in frameworkElementType.Fields)
					property.MonoCecil.RemoveAttributes<ConnectFieldToPropertyAttribute>();

				foreach (var property in frameworkElementType.Properties) {
					property.MonoCecil.RemoveAttributes<NotUseSearchByNameAttribute>();
					property.MonoCecil.RemoveAttributes<PatchingPropertyAttribute>();
					property.MonoCecil.RemoveAttributes<NotPatchingPropertyAttribute>();
					property.MonoCecil.RemoveAttributes<ConnectPropertyToFieldAttribute>();
				}
			}
		}
	}
}