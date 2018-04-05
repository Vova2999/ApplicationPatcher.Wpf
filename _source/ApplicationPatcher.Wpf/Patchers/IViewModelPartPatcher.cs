using ApplicationPatcher.Core.Types.Common;
using ApplicationPatcher.Wpf.Types.Enums;

namespace ApplicationPatcher.Wpf.Patchers {
	public interface IViewModelPartPatcher {
		void Patch(CommonAssembly assembly, CommonType viewModelBase, CommonType viewModel, ViewModelPatchingType viewModelPatchingType);
	}
}