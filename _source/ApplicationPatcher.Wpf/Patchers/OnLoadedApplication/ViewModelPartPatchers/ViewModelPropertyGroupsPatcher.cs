using System.Linq;
using System.Runtime.CompilerServices;
using ApplicationPatcher.Core;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Core.Logs;
using ApplicationPatcher.Core.Types.CommonMembers;
using ApplicationPatcher.Core.Types.Interfaces;
using ApplicationPatcher.Wpf.Extensions;
using ApplicationPatcher.Wpf.Services.Groupers.Property;
using ApplicationPatcher.Wpf.Services.NameRules;
using ApplicationPatcher.Wpf.Types.Enums;
using JetBrains.Annotations;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ApplicationPatcher.Wpf.Patchers.OnLoadedApplication.ViewModelPartPatchers {
	[UsedImplicitly]
	public class ViewModelPropertyGroupsPatcher : ViewModelPartPatcher {
		private readonly PropertyGrouperService propertyGrouperService;
		private readonly NameRulesService nameRulesService;
		private readonly ILog log;

		public ViewModelPropertyGroupsPatcher(PropertyGrouperService propertyGrouperService, NameRulesService nameRulesService) {
			this.propertyGrouperService = propertyGrouperService;
			this.nameRulesService = nameRulesService;
			log = Log.For(this);
		}

		public override PatchResult Patch(CommonAssembly assembly, CommonType viewModelBaseType, CommonType viewModelType, ViewModelPatchingType patchingType) {
			log.Info("Patching property groups...");

			var propertyGroups = propertyGrouperService.GetGroups(assembly, viewModelType, patchingType);
			if (!propertyGroups.Any()) {
				log.Info("Not found property groups");
				return PatchResult.Continue;
			}

			log.Debug("Property groups found:", propertyGroups.Select(group => $"Property: '{group.Property.Name}', field: '{group.Field?.Name}'"));

			foreach (var group in propertyGroups) {
				log.Info($"Patch group: property: '{group.Property.Name}', field: '{group.Field?.Name}'...");
				PatchGroup(assembly, viewModelBaseType, viewModelType, group);
				log.Info("Group was patched");
			}

			log.Info("Property groups was patched");
			return PatchResult.Continue;
		}

		[AddLogOffset]
		private void PatchGroup(CommonAssembly assembly, IHasMethods viewModelBaseType, CommonType viewModelType, PropertyGroup group) {
			RemoveDefaultField(viewModelType, group.Property.Name);

			var field = group.Field?.MonoCecil ?? CreateField(viewModelType, group.Property);

			GenerateGetMethodBody(viewModelType, group.Property.MonoCecil, field);
			GenerateSetMethodBody(assembly, viewModelBaseType, viewModelType, group.Property.MonoCecil, field);
		}

		private void RemoveDefaultField(CommonType viewModelType, string propertyName) {
			var defaultFieldName = $"<{propertyName}>k__BackingField";
			if (!viewModelType.TryGetField(defaultFieldName, out var defaultField))
				return;

			log.Debug($"Remove field with name '{defaultFieldName}'");
			viewModelType.MonoCecil.Fields.Remove(defaultField.MonoCecil);
		}

		private FieldDefinition CreateField(CommonType viewModelType, CommonProperty property) {
			var fieldName = nameRulesService.ConvertName(property.Name, UseNameRulesFor.Property, UseNameRulesFor.Field);

			log.Debug($"Create field with name '{fieldName}'");
			var field = new FieldDefinition(fieldName, FieldAttributes.Private, property.MonoCecil.PropertyType);
			viewModelType.MonoCecil.Fields.Add(field);

			return field;
		}

		private void GenerateGetMethodBody(CommonType viewModelType, PropertyDefinition property, FieldReference field) {
			log.Info("Generate get method body...");

			if (property.GetMethod == null) {
				log.Debug($"Create get accessor method for property '{property.Name}'");
				var getMethod = new MethodDefinition($"get_{property.Name}", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName, property.PropertyType);

				property.GetMethod = getMethod;
				viewModelType.MonoCecil.Methods.Add(getMethod);
			}

			property.GetMethod.Body.Variables.Clear();

			property.GetMethod.Body.Instructions.Clear();
			property.GetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
			property.GetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, field));
			property.GetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

			property.GetMethod.RemoveAttributes<CompilerGeneratedAttribute>();

			log.Info("Get method body was generated");
		}
		private void GenerateSetMethodBody(CommonAssembly assembly, IHasMethods viewModelBaseType, CommonType viewModelType, PropertyDefinition property, FieldReference field) {
			log.Info("Generate set method body...");

			if (property.SetMethod == null) {
				log.Debug($"Create set accessor method for property '{property.Name}'");
				var setMethod = new MethodDefinition($"set_{property.Name}", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName, assembly.MonoCecil.MainModule.TypeSystem.Void);
				setMethod.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, property.PropertyType));

				property.SetMethod = setMethod;
				viewModelType.MonoCecil.Methods.Add(setMethod);
			}

			var setMethodFromViewModelBaseWithGeneric = new GenericInstanceMethod(viewModelBaseType.GetMethod("Set", new[] { typeof(string).FullName, "T&", "T", typeof(bool).FullName }, true).MonoCecil);
			setMethodFromViewModelBaseWithGeneric.GenericArguments.Add(property.PropertyType);

			var importedSetMethodFromViewModelBaseWithGeneric = assembly.MonoCecil.MainModule.ImportReference(setMethodFromViewModelBaseWithGeneric);

			property.GetMethod.Body.Variables.Clear();

			property.SetMethod.Body.Instructions.Clear();
			property.SetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
			property.SetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldstr, property.Name));
			property.SetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
			property.SetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldflda, field));
			property.SetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
			property.SetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
			property.SetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Call, importedSetMethodFromViewModelBaseWithGeneric));
			property.SetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Pop));
			property.SetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

			property.SetMethod.RemoveAttributes<CompilerGeneratedAttribute>();

			log.Info("Set method body was generated");
		}
	}
}