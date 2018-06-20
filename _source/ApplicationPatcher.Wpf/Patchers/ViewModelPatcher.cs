using System.Linq;
using ApplicationPatcher.Core;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Core.Helpers;
using ApplicationPatcher.Core.Logs;
using ApplicationPatcher.Core.Patchers;
using ApplicationPatcher.Core.Types.Common;
using ApplicationPatcher.Wpf.Types.Attributes;
using ApplicationPatcher.Wpf.Types.Enums;
using JetBrains.Annotations;

namespace ApplicationPatcher.Wpf.Patchers {
	[UsedImplicitly]
	public class ViewModelPatcher : LoadedAssemblyPatcher {
		private readonly ViewModelPartPatcher[] viewModelPartPatchers;
		private readonly ILog log;

		public ViewModelPatcher(ViewModelPartPatcher[] viewModelPartPatchers) {
			this.viewModelPartPatchers = viewModelPartPatchers;
			log = Log.For(this);
		}

		public override PatchResult Patch(CommonAssembly assembly) {
			log.Info("Patching view models...");
			var viewModelBase = assembly.GetCommonType(KnownTypeNames.ViewModelBase).Load();

			var viewModels = assembly.GetInheritanceCommonTypes(viewModelBase).WhereFrom(assembly).ToArray();
			if (!viewModels.Any()) {
				log.Info("Not found view models");
				return PatchResult.Continue;
			}

			log.Debug("View models found:", viewModels.Select(viewModel => viewModel.FullName).OrderBy(fullName => fullName));

			foreach (var viewModel in viewModels) {
				log.Info($"Patching type '{viewModel.FullName}'...");

				log.Info($"Loading type '{viewModel.FullName}'...");
				viewModel.Load();
				log.Info($"Type '{viewModel.FullName}' was loaded");

				var patchingViewModelAttribute = viewModel.GetReflectionAttribute<PatchingViewModelAttribute>();
				var viewModelPatchingType = patchingViewModelAttribute?.ViewModelPatchingType ?? ViewModelPatchingType.All; // todo: Shift default ViewModelPatchingType to configuration
				log.Info($"View model patching type: {viewModelPatchingType}");

				var patchResult = PatchHelper.PatchApplication(viewModelPartPatchers, patcher => patcher.Patch(assembly, viewModelBase, viewModel, viewModelPatchingType), log);
				if (patchResult == PatchResult.Cancel)
					return PatchResult.Cancel;

				log.Info($"Type '{viewModel.FullName}' was patched");
			}

			log.Info("View models was patched");
			return PatchResult.Continue;
		}
	}
}