using ApplicationPatcher.Core;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Core.Logs;
using ApplicationPatcher.Core.Patchers;
using ApplicationPatcher.Core.Types.CommonInterfaces;
using ApplicationPatcher.Wpf.Types.Attributes;
using JetBrains.Annotations;

namespace ApplicationPatcher.Wpf.Patchers.OnNotLoadedApplication {
	[UsedImplicitly]
	public class CheckAssemblyPatchedAttributePatcher : PatcherOnNotLoadedApplication {
		private readonly ILog log;

		public CheckAssemblyPatchedAttributePatcher() {
			log = Log.For(this);
		}

		public override PatchResult Patch(ICommonAssembly assembly) {
			log.Info("Check assembly patched attribute...");

			if (assembly.ContainsReflectionAttribute<AssemblyPatchedAttribute>()) {
				log.Info("Assembly patched attribute found");
				return PatchResult.Cancel;
			}

			log.Info("Assembly patched attribute not found");
			return PatchResult.Continue;
		}
	}
}