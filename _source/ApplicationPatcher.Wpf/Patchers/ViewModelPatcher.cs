using System.Linq;
using ApplicationPatcher.Core;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Core.Helpers;
using ApplicationPatcher.Core.Logs;
using ApplicationPatcher.Core.Patchers;
using ApplicationPatcher.Core.Types.Common;
using ApplicationPatcher.Wpf.Configurations;
using ApplicationPatcher.Wpf.Types.Attributes;
using JetBrains.Annotations;

namespace ApplicationPatcher.Wpf.Patchers {
	[UsedImplicitly]
	public class ViewModelPatcher : LoadedAssemblyPatcher {
		private readonly ApplicationPatcherWpfConfiguration applicationPatcherWpfConfiguration;
		private readonly ViewModelPartPatcher[] viewModelPartPatchers;
		private readonly ILog log;

		public ViewModelPatcher(ApplicationPatcherWpfConfiguration applicationPatcherWpfConfiguration, ViewModelPartPatcher[] viewModelPartPatchers) {
			this.applicationPatcherWpfConfiguration = applicationPatcherWpfConfiguration;
			this.viewModelPartPatchers = viewModelPartPatchers;
			log = Log.For(this);
		}

		public override PatchResult Patch(CommonAssembly commonAssembly) {
			log.Info("Patching view model types...");
			var viewModelBaseType = commonAssembly.GetCommonType(KnownTypeNames.ViewModelBase).Load();

			var viewModelTypes = commonAssembly.GetInheritanceCommonTypesFromThisAssembly(viewModelBaseType).ToArray();
			if (!viewModelTypes.Any()) {
				log.Info("Not found view model types");
				return PatchResult.Continue;
			}

			log.Debug("View model types found:", viewModelTypes.Select(viewModel => viewModel.FullName).OrderBy(fullName => fullName));

			foreach (var viewModel in viewModelTypes) {
				log.Info($"Patching type '{viewModel.FullName}'...");

				log.Info($"Loading type '{viewModel.FullName}'...");
				viewModel.Load();
				log.Info($"Type '{viewModel.FullName}' was loaded");

				var patchingType = viewModel.GetReflectionAttribute<PatchingViewModelAttribute>()?.ViewModelPatchingType ?? applicationPatcherWpfConfiguration.DefaultViewModelPatchingType;
				log.Info($"View model patching type: {patchingType}");

				var patchResult = PatchHelper.PatchApplication(viewModelPartPatchers, patcher => patcher.Patch(commonAssembly, viewModelBaseType, viewModel, patchingType), log);
				if (patchResult == PatchResult.Cancel)
					return PatchResult.Cancel;

				log.Info($"Type '{viewModel.FullName}' was patched");
			}

			log.Info("View model types was patched");
			return PatchResult.Continue;
		}
	}
}