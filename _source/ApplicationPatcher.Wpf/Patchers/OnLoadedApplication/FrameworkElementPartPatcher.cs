using ApplicationPatcher.Core;
using ApplicationPatcher.Core.Logs;
using ApplicationPatcher.Core.Types.CommonMembers;
using ApplicationPatcher.Wpf.Types.Enums;

namespace ApplicationPatcher.Wpf.Patchers.OnLoadedApplication {
	public abstract class FrameworkElementPartPatcher {
		[AddLogOffset]
		public abstract PatchResult Patch(CommonAssembly assembly, CommonType frameworkElementType, FrameworkElementPatchingType patchingType);
	}
}