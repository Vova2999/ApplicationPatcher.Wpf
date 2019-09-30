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

		public override PatchResult Patch(ICommonAssembly assembly) {
			log.Info("Patching framework element types...");

			var selectingType = assembly.GetReflectionAttribute<SelectingFrameworkElementAttribute>()?.SelectingType
				?? applicationPatcherWpfConfiguration.DefaultFrameworkElementSelectingType;
			log.Info($"Framework element selecting type: '{selectingType}'");

			var frameworkElementBaseType = assembly.GetCommonType(KnownTypeNames.FrameworkElement, true).Load();
			CheckAssembly(assembly, frameworkElementBaseType);

			var frameworkElementTypes = assembly.GetInheritanceCommonTypesFromThisAssembly(frameworkElementBaseType).ToArray();

			if (!frameworkElementTypes.Any()) {
				log.Info("Not found framework element types");
				return PatchResult.Continue;
			}

			log.Debug("Framework element types found:", frameworkElementTypes.Select(frameworkElement => frameworkElement.FullName).OrderBy(fullName => fullName));

			var patchingFrameworkElementTypes = frameworkElementTypes
				.Where(frameworkElementType => frameworkElementType.NotContainsReflectionAttribute<NotPatchingFrameworkElementAttribute>() &&
					(selectingType == FrameworkElementSelectingType.All || frameworkElementType.ContainsReflectionAttribute<PatchingFrameworkElementAttribute>()))
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

		private static void CheckAssembly(ICommonAssembly assembly, ICommonType frameworkElementBaseType) {
			var typesWithPatchingFrameworkElementAttribute = assembly.TypesFromThisAssembly.Where(type => type.ContainsReflectionAttribute<PatchingFrameworkElementAttribute>()).ToArray();
			var typesWithNotPatchingFrameworkElementAttribute = assembly.TypesFromThisAssembly.Where(type => type.ContainsReflectionAttribute<NotPatchingFrameworkElementAttribute>()).ToArray();

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
		private PatchResult PatchFrameworkElement(ICommonAssembly assembly, ICommonType frameworkElementType) {
			log.Info($"Loading type '{frameworkElementType.FullName}'...");
			frameworkElementType.Load(1);
			log.Info($"Type '{frameworkElementType.FullName}' was loaded");

			var patchingType = frameworkElementType.GetReflectionAttribute<PatchingFrameworkElementAttribute>()?.PatchingType
				?? applicationPatcherWpfConfiguration.DefaultFrameworkElementPatchingType;
			log.Info($"Framework element patching type: '{patchingType}'");

			return PatchHelper.PatchApplication(frameworkElementPartPatchers, patcher => patcher.Patch(assembly, frameworkElementType, patchingType), log);
		}
	}
}