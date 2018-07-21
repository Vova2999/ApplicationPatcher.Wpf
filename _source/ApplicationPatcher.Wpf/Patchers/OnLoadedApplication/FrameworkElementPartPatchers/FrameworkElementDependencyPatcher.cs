using ApplicationPatcher.Core;
using ApplicationPatcher.Core.Types.CommonMembers;
using ApplicationPatcher.Wpf.Types.Enums;
using JetBrains.Annotations;

namespace ApplicationPatcher.Wpf.Patchers.OnLoadedApplication.FrameworkElementPartPatchers {
	[UsedImplicitly]
	public class FrameworkElementDependencyPatcher : FrameworkElementPartPatcher {
		public override PatchResult Patch(CommonAssembly assembly, CommonType frameworkElementType, FrameworkElementPatchingType patchingType) {
			return PatchResult.Continue;
		}
	}
}