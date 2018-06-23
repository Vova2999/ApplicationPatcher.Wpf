using System;
using System.Linq;
using ApplicationPatcher.Core;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Core.Logs;
using ApplicationPatcher.Core.Types.Common;
using ApplicationPatcher.Wpf.Exceptions;
using ApplicationPatcher.Wpf.Services;
using ApplicationPatcher.Wpf.Services.PropertyGrouper;
using ApplicationPatcher.Wpf.Types.Attributes;
using ApplicationPatcher.Wpf.Types.Enums;
using JetBrains.Annotations;

namespace ApplicationPatcher.Wpf.Patchers.ViewModelPartPatchers {
	[UsedImplicitly]
	public class ViewModelPropertiesPatcher : ViewModelPartPatcher {
		private readonly PropertyGrouperService propertyGrouperService;
		private readonly ILog log;

		public ViewModelPropertiesPatcher(PropertyGrouperService propertyGrouperService) {
			this.propertyGrouperService = propertyGrouperService;
			log = Log.For(this);
		}

		public override PatchResult Patch(CommonAssembly commonAssembly, CommonType viewModelBaseType, CommonType viewModelType, ViewModelPatchingType patchingType) {
			log.Info("Patching properties...");
			
			var propertyGroups = propertyGrouperService.GetGroups(commonAssembly, viewModelType, patchingType);

			log.Info("Properties was patched");
			return PatchResult.Continue;
		}
	}
}