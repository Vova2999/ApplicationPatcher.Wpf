using System.Linq;
using ApplicationPatcher.Core;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Core.Logs;
using ApplicationPatcher.Core.Patchers;
using ApplicationPatcher.Core.Types.CommonMembers;
using ApplicationPatcher.Wpf.Extensions;
using ApplicationPatcher.Wpf.Types.Attributes;
using ApplicationPatcher.Wpf.Types.Attributes.FrameworkElement;
using JetBrains.Annotations;

namespace ApplicationPatcher.Wpf.Patchers.OnPatchedApplication {
	[UsedImplicitly]
	public class RemoveFrameworkElementAttributesPatcher : PatcherOnPatchedApplication {
		private readonly ILog log;

		public RemoveFrameworkElementAttributesPatcher() {
			log = Log.For(this);
		}

		public override PatchResult Patch(CommonAssembly assembly) {
			log.Info("Remove framework element attributes...");

			RemoveAttributesFromFrameworkElements(assembly);

			log.Info("Framework element attributes was removed");
			return PatchResult.Continue;
		}

		private static void RemoveAttributesFromFrameworkElements(CommonAssembly assembly) {
			var frameworkElementBaseType = assembly.GetCommonType(KnownTypeNames.FrameworkElement, true);
			var frameworkElementTypes = assembly.GetInheritanceCommonTypesFromThisAssembly(frameworkElementBaseType).ToArray();

			foreach (var frameworkElementType in frameworkElementTypes) {
				frameworkElementType.MonoCecil.RemoveAttributes<PatchingFrameworkElementAttribute>();
				frameworkElementType.MonoCecil.RemoveAttributes<NotPatchingFrameworkElementAttribute>();

				foreach (var property in frameworkElementType.Fields)
					property.MonoCecil.RemoveAttributes<ConnectDependencyToPropertyAttribute>();

				foreach (var property in frameworkElementType.Properties) {
					property.MonoCecil.RemoveAttributes<NotUseSearchByNameAttribute>();
					property.MonoCecil.RemoveAttributes<PatchingDependencyPropertyAttribute>();
					property.MonoCecil.RemoveAttributes<NotPatchingDependencyPropertyAttribute>();
					property.MonoCecil.RemoveAttributes<ConnectPropertyToDependencyAttribute>();
				}
			}
		}
	}
}