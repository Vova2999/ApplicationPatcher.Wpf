using System.Linq;
using ApplicationPatcher.Core;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Core.Logs;
using ApplicationPatcher.Core.Patchers;
using ApplicationPatcher.Core.Types.CommonMembers;
using ApplicationPatcher.Wpf.Extensions;
using ApplicationPatcher.Wpf.Types.Attributes;
using ApplicationPatcher.Wpf.Types.Attributes.ViewModel;
using JetBrains.Annotations;

namespace ApplicationPatcher.Wpf.Patchers.OnPatchedApplication {
	[UsedImplicitly]
	public class RemoveApplicationPatcherWpfAttributesPatcher : PatcherOnPatchedApplication {
		private readonly ILog log;

		public RemoveApplicationPatcherWpfAttributesPatcher() {
			log = Log.For(this);
		}

		public override PatchResult Patch(CommonAssembly assembly) {
			log.Info("Remove application patcher wpf attributes...");

			RemoveAttributesFromViewModels(assembly);

			log.Info("Application patcher wpf attributes was removed");
			return PatchResult.Continue;
		}

		private static void RemoveAttributesFromViewModels(CommonAssembly assembly) {
			var viewModelBaseType = assembly.GetCommonType(KnownTypeNames.ViewModelBase);
			var viewModelTypes = assembly.GetInheritanceCommonTypesFromThisAssembly(viewModelBaseType).ToArray();

			foreach (var viewModelType in viewModelTypes) {
				viewModelType.MonoCecil.RemoveAttributes<PatchingViewModelAttribute>();
				viewModelType.MonoCecil.RemoveAttributes<NotPatchingViewModelAttribute>();

				foreach (var property in viewModelType.Fields)
					property.MonoCecil.RemoveAttributes<ConnectFieldToPropertyAttribute>();

				foreach (var property in viewModelType.Properties) {
					property.MonoCecil.RemoveAttributes<NotUseSearchByNameAttribute>();
					property.MonoCecil.RemoveAttributes<PatchingPropertyAttribute>();
					property.MonoCecil.RemoveAttributes<NotPatchingPropertyAttribute>();
					property.MonoCecil.RemoveAttributes<ConnectPropertyToFieldAttribute>();
					property.MonoCecil.RemoveAttributes<ConnectPropertyToMethodAttribute>();
				}

				foreach (var property in viewModelType.Methods) {
					property.MonoCecil.RemoveAttributes<NotUseSearchByNameAttribute>();
					property.MonoCecil.RemoveAttributes<PatchingCommandAttribute>();
					property.MonoCecil.RemoveAttributes<NotPatchingCommandAttribute>();
					property.MonoCecil.RemoveAttributes<ConnectMethodToMethodAttribute>();
					property.MonoCecil.RemoveAttributes<ConnectMethodToPropertyAttribute>();
				}
			}
		}
	}
}