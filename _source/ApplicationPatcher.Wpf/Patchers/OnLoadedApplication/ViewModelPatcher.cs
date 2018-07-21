using System.Linq;
using ApplicationPatcher.Core;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Core.Helpers;
using ApplicationPatcher.Core.Logs;
using ApplicationPatcher.Core.Patchers;
using ApplicationPatcher.Core.Types.CommonMembers;
using ApplicationPatcher.Wpf.Configurations;
using ApplicationPatcher.Wpf.Types.Attributes.ViewModel;
using JetBrains.Annotations;

namespace ApplicationPatcher.Wpf.Patchers.OnLoadedApplication {
	[UsedImplicitly]
	public class ViewModelPatcher : PatcherOnLoadedApplication {
		private readonly ApplicationPatcherWpfConfiguration applicationPatcherWpfConfiguration;
		private readonly ViewModelPartPatcher[] viewModelPartPatchers;
		private readonly ILog log;

		public ViewModelPatcher(ApplicationPatcherWpfConfiguration applicationPatcherWpfConfiguration, ViewModelPartPatcher[] viewModelPartPatchers) {
			this.applicationPatcherWpfConfiguration = applicationPatcherWpfConfiguration;
			this.viewModelPartPatchers = viewModelPartPatchers;
			log = Log.For(this);
		}

		public override PatchResult Patch(CommonAssembly assembly) {
			log.Info("Patching view model types...");
			var viewModelBaseType = assembly.GetCommonType(KnownTypeNames.ViewModelBase).Load();

			var viewModelTypes = assembly.GetInheritanceCommonTypesFromThisAssembly(viewModelBaseType).ToArray();
			if (!viewModelTypes.Any()) {
				log.Info("Not found view model types");
				return PatchResult.Continue;
			}

			log.Debug("View model types found:", viewModelTypes.Select(viewModel => viewModel.FullName).OrderBy(fullName => fullName));

			var patchingViewModelTypes = viewModelTypes.Where(viewModelType => viewModelType.NotContainsAttribute<NotPatchingViewModelAttribute>()).ToArray();
			if (!patchingViewModelTypes.Any()) {
				log.Info("Not found patching view model types");
				return PatchResult.Continue;
			}

			log.Debug("Patching view model types:", patchingViewModelTypes.Select(viewModel => viewModel.FullName).OrderBy(fullName => fullName));

			foreach (var viewModelType in patchingViewModelTypes) {
				log.Info($"Patching type '{viewModelType.FullName}'...");

				if (PatchViewModel(assembly, viewModelBaseType, viewModelType) == PatchResult.Cancel)
					return PatchResult.Cancel;

				log.Info($"Type '{viewModelType.FullName}' was patched");
			}

			log.Info("View model types was patched");
			return PatchResult.Continue;
		}

		[AddLogOffset]
		private PatchResult PatchViewModel(CommonAssembly assembly, CommonType viewModelBaseType, CommonType viewModelType) {
			log.Info($"Loading type '{viewModelType.FullName}'...");
			viewModelType.Load();
			log.Info($"Type '{viewModelType.FullName}' was loaded");

			var patchingType = viewModelType.GetReflectionAttribute<PatchingViewModelAttribute>()?.ViewModelPatchingType ?? applicationPatcherWpfConfiguration.DefaultViewModelPatchingType;
			log.Info($"View model patching type: '{patchingType}'");

			return PatchHelper.PatchApplication(viewModelPartPatchers, patcher => patcher.Patch(assembly, viewModelBaseType, viewModelType, patchingType), log);
		}
	}
}