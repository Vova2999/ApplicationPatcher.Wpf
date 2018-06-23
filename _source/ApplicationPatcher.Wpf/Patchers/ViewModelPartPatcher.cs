using ApplicationPatcher.Core;
using ApplicationPatcher.Core.Logs;
using ApplicationPatcher.Core.Types.Common;
using ApplicationPatcher.Wpf.Types.Enums;

namespace ApplicationPatcher.Wpf.Patchers {
	public abstract class ViewModelPartPatcher {
		[AddLogOffset]
		public abstract PatchResult Patch(CommonAssembly commonAssembly, CommonType viewModelBaseType, CommonType viewModelType, ViewModelPatchingType patchingType);
	}
}