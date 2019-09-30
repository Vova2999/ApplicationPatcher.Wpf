using ApplicationPatcher.Core;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Core.Logs;
using ApplicationPatcher.Core.Patchers;
using ApplicationPatcher.Core.Types.CommonInterfaces;
using ApplicationPatcher.Wpf.Types.Attributes;
using JetBrains.Annotations;
using Mono.Cecil;

namespace ApplicationPatcher.Wpf.Patchers.OnPatchedApplication {
	[UsedImplicitly]
	public class AddAssemblyPatchedAttributePatcher : PatcherOnPatchedApplication {
		private readonly ILog log;

		public AddAssemblyPatchedAttributePatcher() {
			log = Log.For(this);
		}

		public override PatchResult Patch(ICommonAssembly assembly) {
			log.Info("Add assembly patched attribute...");

			if (!assembly.NotContainsReflectionAttribute<AssemblyPatchedAttribute>()) {
				log.Info("Assembly patched attribute already exists");
				return PatchResult.Continue;
			}

			var assemblyPatchedAttributeConstructor = assembly.GetCommonType(typeof(AssemblyPatchedAttribute), true).Load().GetConstructor(true);
			assembly.MonoCecil.CustomAttributes.Add(new CustomAttribute(assembly.MonoCecil.MainModule.ImportReference(assemblyPatchedAttributeConstructor.MonoCecil)));

			log.Info("Assembly patched attribute was added");
			return PatchResult.Continue;
		}
	}
}