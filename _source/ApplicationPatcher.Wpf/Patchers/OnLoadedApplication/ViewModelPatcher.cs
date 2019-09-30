using System.Linq;
using ApplicationPatcher.Core;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Core.Helpers;
using ApplicationPatcher.Core.Logs;
using ApplicationPatcher.Core.Patchers;
using ApplicationPatcher.Core.Types.CommonInterfaces;
using ApplicationPatcher.Wpf.Configurations;
using ApplicationPatcher.Wpf.Exceptions;
using ApplicationPatcher.Wpf.Services;
using ApplicationPatcher.Wpf.Types.Attributes;
using ApplicationPatcher.Wpf.Types.Attributes.ViewModel;
using ApplicationPatcher.Wpf.Types.Enums;
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

		public override PatchResult Patch(ICommonAssembly assembly) {
			log.Info("Patching view model types...");

			var selectingType = assembly.GetReflectionAttribute<SelectingViewModelAttribute>()?.SelectingType
				?? applicationPatcherWpfConfiguration.DefaultViewModelSelectingType;
			log.Info($"View model selecting type: '{selectingType}'");

			var viewModelBaseType = assembly.GetCommonType(KnownTypeNames.ViewModelBase, true).Load();
			CheckAssembly(assembly, viewModelBaseType);

			var viewModelTypes = assembly.GetInheritanceCommonTypesFromThisAssembly(viewModelBaseType).ToArray();
			if (!viewModelTypes.Any()) {
				log.Info("Not found view model types");
				return PatchResult.Continue;
			}

			log.Debug("View model types found:", viewModelTypes.Select(viewModel => viewModel.FullName).OrderBy(fullName => fullName));

			var patchingViewModelTypes = viewModelTypes
				.Where(viewModelType => viewModelType.NotContainsReflectionAttribute<NotPatchingViewModelAttribute>() &&
					(selectingType == ViewModelSelectingType.All || viewModelType.ContainsReflectionAttribute<PatchingViewModelAttribute>()))
				.ToArray();

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

		private static void CheckAssembly(ICommonAssembly assembly, ICommonType viewModelBaseType) {
			var typesWithPatchingViewModelAttribute = assembly.TypesFromThisAssembly.Where(type => type.ContainsReflectionAttribute<PatchingViewModelAttribute>()).ToArray();
			var typesWithNotPatchingViewModelAttribute = assembly.TypesFromThisAssembly.Where(type => type.ContainsReflectionAttribute<NotPatchingViewModelAttribute>()).ToArray();

			var errorsService = new ErrorsService()
				.AddErrors(typesWithPatchingViewModelAttribute
					.Where(type => type.IsNotInheritedFrom(viewModelBaseType))
					.Select(type => $"Type '{type.FullName}' with attribute " +
						$"'{nameof(PatchingViewModelAttribute)}' must be inherited from '{viewModelBaseType.FullName}'"))
				.AddErrors(typesWithNotPatchingViewModelAttribute
					.Where(type => type.IsNotInheritedFrom(viewModelBaseType))
					.Select(type => $"Type '{type.FullName}' with attribute " +
						$"'{nameof(NotPatchingViewModelAttribute)}' must be inherited from '{viewModelBaseType.FullName}'"))
				.AddErrors(typesWithPatchingViewModelAttribute
					.Intersect(typesWithNotPatchingViewModelAttribute)
					.Select(type => $"Patching type '{type.FullName}' can not have " +
						$"'{nameof(PatchingViewModelAttribute)}' and '{nameof(NotPatchingViewModelAttribute)}' at the same time"));

			if (errorsService.HasErrors)
				throw new ViewModelPatchingException(errorsService);
		}

		[AddLogOffset]
		private PatchResult PatchViewModel(ICommonAssembly assembly, ICommonType viewModelBaseType, ICommonType viewModelType) {
			log.Info($"Loading type '{viewModelType.FullName}'...");
			viewModelType.Load(1);
			log.Info($"Type '{viewModelType.FullName}' was loaded");

			var patchingType = viewModelType.GetReflectionAttribute<PatchingViewModelAttribute>()?.PatchingType
				?? applicationPatcherWpfConfiguration.DefaultViewModelPatchingType;
			log.Info($"View model patching type: '{patchingType}'");

			return PatchHelper.PatchApplication(viewModelPartPatchers, patcher => patcher.Patch(assembly, viewModelBaseType, viewModelType, patchingType), log);
		}
	}
}