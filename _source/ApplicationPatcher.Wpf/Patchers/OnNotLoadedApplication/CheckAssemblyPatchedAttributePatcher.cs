using ApplicationPatcher.Core;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Core.Logs;
using ApplicationPatcher.Core.Patchers;
using ApplicationPatcher.Core.Types.CommonMembers;
using ApplicationPatcher.Wpf.Types.Attributes;
using JetBrains.Annotations;

namespace ApplicationPatcher.Wpf.Patchers.OnNotLoadedApplication {
	[UsedImplicitly]
	public class CheckAssemblyPatchedAttributePatcher : PatcherOnNotLoadedApplication {
		private readonly ILog log;

		public CheckAssemblyPatchedAttributePatcher() {
			log = Log.For(this);
		}

		public override PatchResult Patch(CommonAssembly assembly) {
			log.Info("Check assembly patched attribute...");

			if (assembly.ContainsAttribute<AssemblyPatchedAttribute>()) {
				log.Info("Assembly patched attribute found");
				return PatchResult.Cancel;
			}

			log.Info("Assembly patched attribute not found");
			return PatchResult.Continue;
		}
	}
}