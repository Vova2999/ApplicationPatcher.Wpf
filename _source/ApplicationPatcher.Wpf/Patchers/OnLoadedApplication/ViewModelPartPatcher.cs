using ApplicationPatcher.Core;
using ApplicationPatcher.Core.Logs;
using ApplicationPatcher.Core.Types.CommonInterfaces;
using ApplicationPatcher.Wpf.Types.Enums;

namespace ApplicationPatcher.Wpf.Patchers.OnLoadedApplication {
	public abstract class ViewModelPartPatcher {
		[AddLogOffset]
		public abstract PatchResult Patch(ICommonAssembly assembly, ICommonType viewModelBaseType, ICommonType viewModelType, ViewModelPatchingType patchingType);
	}
}