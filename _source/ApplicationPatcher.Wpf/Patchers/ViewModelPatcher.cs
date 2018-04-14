using System.Linq;
using ApplicationPatcher.Core;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Core.Helpers;
using ApplicationPatcher.Core.Types.Common;
using ApplicationPatcher.Wpf.Types.Attributes.ViewModels;
using ApplicationPatcher.Wpf.Types.Enums;
using JetBrains.Annotations;

namespace ApplicationPatcher.Wpf.Patchers {
	[UsedImplicitly]
	public class ViewModelPatcher : IPatcher {
		private readonly IViewModelPartPatcher[] viewModelPartPatchers;
		private readonly Log log;

		public ViewModelPatcher(IViewModelPartPatcher[] viewModelPartPatchers) {
			this.viewModelPartPatchers = viewModelPartPatchers;
			log = Log.For(this);
		}

		[DoNotAddLogOffset]
		public void Patch(CommonAssembly assembly) {
			log.Info("Patching view models...");
			var viewModelBase = assembly.GetCommonType("GalaSoft.MvvmLight.ViewModelBase").Load();

			var viewModels = assembly.GetInheritanceCommonTypes(viewModelBase).WhereFrom(assembly).ToArray();
			if (!viewModels.Any()) {
				log.Info("Not found view models");
				return;
			}

			log.Debug("View models found:", viewModels.Select(viewModel => viewModel.FullName));
			foreach (var viewModel in viewModels) {
				log.Info($"Patching {viewModel.FullName}...");
				viewModel.Load();

				var patchingViewModelAttribute = viewModel.GetReflectionAttribute<PatchingViewModelAttribute>();
				var viewModelPatchingType = patchingViewModelAttribute?.ViewModelPatchingType ?? ViewModelPatchingType.All;
				log.Info($"View model patching type: {viewModelPatchingType}");

				viewModelPartPatchers.ForEach(viewModelPartPatcher => viewModelPartPatcher.Patch(assembly, viewModelBase, viewModel, viewModelPatchingType));
				log.Info($"{viewModel.FullName} was patched");
			}

			log.Info("View models was patched");
		}
	}
}