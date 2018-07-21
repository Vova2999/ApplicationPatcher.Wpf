using ApplicationPatcher.Core;
using ApplicationPatcher.Core.Logs;
using ApplicationPatcher.Core.Patchers;
using ApplicationPatcher.Core.Types.CommonMembers;
using ApplicationPatcher.Wpf.Extensions;
using ApplicationPatcher.Wpf.Types.Attributes;
using JetBrains.Annotations;

namespace ApplicationPatcher.Wpf.Patchers.OnPatchedApplication {
	[UsedImplicitly]
	public class RemoveAssemblyAttributesPatcher : PatcherOnPatchedApplication {
		private readonly ILog log;

		public RemoveAssemblyAttributesPatcher() {
			log = Log.For(this);
		}

		public override PatchResult Patch(CommonAssembly assembly) {
			log.Info("Remove assembly attributes...");

			RemoveAttributesFromAssembly(assembly);

			log.Info("Assembly attributes was removed");
			return PatchResult.Continue;
		}

		private static void RemoveAttributesFromAssembly(CommonAssembly assembly) {
			assembly.MonoCecil.RemoveAttributes<SelectingViewModelAttribute>();
			assembly.MonoCecil.RemoveAttributes<SelectingFrameworkElementAttribute>();
		}
	}
}