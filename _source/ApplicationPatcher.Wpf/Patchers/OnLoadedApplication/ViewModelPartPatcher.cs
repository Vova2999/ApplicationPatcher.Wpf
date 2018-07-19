using ApplicationPatcher.Core;
using ApplicationPatcher.Core.Logs;
using ApplicationPatcher.Core.Types.CommonMembers;
using ApplicationPatcher.Wpf.Types.Enums;

namespace ApplicationPatcher.Wpf.Patchers.OnLoadedApplication {
	public abstract class ViewModelPartPatcher {
		[AddLogOffset]
		public abstract PatchResult Patch(CommonAssembly assembly, CommonType viewModelBaseType, CommonType viewModelType, ViewModelPatchingType patchingType);
	}
}