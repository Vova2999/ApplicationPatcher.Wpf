using System.Linq;
using ApplicationPatcher.Core;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Core.Helpers;
using ApplicationPatcher.Core.Logs;
using ApplicationPatcher.Core.Patchers;
using ApplicationPatcher.Core.Types.CommonMembers;
using ApplicationPatcher.Wpf.Configurations;
using ApplicationPatcher.Wpf.Exceptions;
using ApplicationPatcher.Wpf.Services;
using ApplicationPatcher.Wpf.Types.Attributes;
using ApplicationPatcher.Wpf.Types.Attributes.FrameworkElement;
using ApplicationPatcher.Wpf.Types.Enums;
using JetBrains.Annotations;

namespace ApplicationPatcher.Wpf.Patchers.OnLoadedApplication {
	[UsedImplicitly]
	public class FrameworkElementPatcher : PatcherOnLoadedApplication {
		private readonly ApplicationPatcherWpfConfiguration applicationPatcherWpfConfiguration;
		private readonly FrameworkElementPartPatcher[] frameworkElementPartPatchers;
		private readonly ILog log;

		public FrameworkElementPatcher(ApplicationPatcherWpfConfiguration applicationPatcherWpfConfiguration, FrameworkElementPartPatcher[] frameworkElementPartPatchers) {
			this.applicationPatcherWpfConfiguration = applicationPatcherWpfConfiguration;
			this.frameworkElementPartPatchers = frameworkElementPartPatchers;
			log = Log.For(this);
		}

		public override PatchResult Patch(CommonAssembly assembly) {
			log.Info("Patching framework element types...");

			var selectingType = assembly.GetReflectionAttribute<SelectingFrameworkElementAttribute>()?.SelectingType
				?? applicationPatcherWpfConfiguration.DefaultFrameworkElementSelectingType;
			log.Info($"Framework element selecting type: '{selectingType}'");

			var windowBaseType = assembly.GetCommonType(KnownTypeNames.Window, true);
			var frameworkElementBaseType = assembly.GetCommonType(KnownTypeNames.FrameworkElement, true);
			CheckAssembly(assembly, frameworkElementBaseType);

			var frameworkElementTypes = assembly.GetInheritanceCommonTypesFromThisAssembly(frameworkElementBaseType)
				.Where(frameworkElementType => frameworkElementType.IsNotInheritedFrom(windowBaseType))
				.ToArray();

			if (!frameworkElementTypes.Any()) {
				log.Info("Not found framework element types");
				return PatchResult.Continue;
			}

			log.Debug("Framework element types found:", frameworkElementTypes.Select(frameworkElement => frameworkElement.FullName).OrderBy(fullName => fullName));

			var patchingFrameworkElementTypes = frameworkElementTypes
				.Where(frameworkElementType => frameworkElementType.NotContainsAttribute<NotPatchingFrameworkElementAttribute>() &&
					(selectingType == FrameworkElementSelectingType.All || frameworkElementType.ContainsAttribute<PatchingFrameworkElementAttribute>()))
				.ToArray();

			if (!patchingFrameworkElementTypes.Any()) {
				log.Info("Not found patching framework element types");
				return PatchResult.Continue;
			}

			log.Debug("Patching framework element types:", patchingFrameworkElementTypes.Select(frameworkElement => frameworkElement.FullName).OrderBy(fullName => fullName));

			foreach (var frameworkElementType in patchingFrameworkElementTypes) {
				log.Info($"Patching type '{frameworkElementType.FullName}'...");

				if (PatchFrameworkElement(assembly, frameworkElementType) == PatchResult.Cancel)
					return PatchResult.Cancel;

				log.Info($"Type '{frameworkElementType.FullName}' was patched");
			}

			log.Info("Framework element types was patched");
			return PatchResult.Continue;
		}

		private static void CheckAssembly(CommonAssembly assembly, CommonType frameworkElementBaseType) {
			var typesWithPatchingFrameworkElementAttribute = assembly.TypesFromThisAssembly.Where(type => type.ContainsAttribute<PatchingFrameworkElementAttribute>()).ToArray();
			var typesWithNotPatchingFrameworkElementAttribute = assembly.TypesFromThisAssembly.Where(type => type.ContainsAttribute<NotPatchingFrameworkElementAttribute>()).ToArray();

			var errorsService = new ErrorsService()
				.AddErrors(typesWithPatchingFrameworkElementAttribute
					.Where(type => type.IsNotInheritedFrom(frameworkElementBaseType))
					.Select(type => $"Type '{type.FullName}' with attribute " +
						$"'{nameof(PatchingFrameworkElementAttribute)}' must be inherited from '{frameworkElementBaseType.FullName}'"))
				.AddErrors(typesWithNotPatchingFrameworkElementAttribute
					.Where(type => type.IsNotInheritedFrom(frameworkElementBaseType))
					.Select(type => $"Type '{type.FullName}' with attribute " +
						$"'{nameof(NotPatchingFrameworkElementAttribute)}' must be inherited from '{frameworkElementBaseType.FullName}'"))
				.AddErrors(typesWithPatchingFrameworkElementAttribute
					.Intersect(typesWithNotPatchingFrameworkElementAttribute)
					.Select(type => $"Patching type '{type.FullName}' can not have " +
						$"'{nameof(PatchingFrameworkElementAttribute)}' and '{nameof(NotPatchingFrameworkElementAttribute)}' at the same time"));

			if (errorsService.HasErrors)
				throw new FrameworkElementPatchingException(errorsService);
		}

		[AddLogOffset]
		private PatchResult PatchFrameworkElement(CommonAssembly assembly, CommonType frameworkElementType) {
			log.Info($"Loading type '{frameworkElementType.FullName}'...");
			frameworkElementType.Load();
			log.Info($"Type '{frameworkElementType.FullName}' was loaded");

			var patchingType = frameworkElementType.GetReflectionAttribute<PatchingFrameworkElementAttribute>()?.PatchingType
				?? applicationPatcherWpfConfiguration.DefaultFrameworkElementPatchingType;
			log.Info($"Framework element patching type: '{patchingType}'");

			return PatchHelper.PatchApplication(frameworkElementPartPatchers, patcher => patcher.Patch(assembly, frameworkElementType, patchingType), log);
		}
	}
}