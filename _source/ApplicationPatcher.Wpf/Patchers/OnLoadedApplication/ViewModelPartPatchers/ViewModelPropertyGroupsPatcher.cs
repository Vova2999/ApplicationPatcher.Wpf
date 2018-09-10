using System.Collections.Generic;
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

			GenerateGetMethodBody(viewModelType, group, field);
			GenerateSetMethodBody(assembly, viewModelBaseType, viewModelType, group, field);
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

		private void GenerateGetMethodBody(CommonType viewModelType, PropertyGroup group, FieldReference field) {
			log.Info("Generate get method body...");

			var property = group.Property;
			if (property.GetMethod == null) {
				log.Debug($"Create get accessor method for property '{property.Name}'");
				var getMethod = new MethodDefinition($"get_{property.Name}", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName, property.MonoCecil.PropertyType);

				property.MonoCecil.GetMethod = getMethod;
				viewModelType.MonoCecil.Methods.Add(getMethod);
			}

			property.MonoCecil.GetMethod.Body.Variables.Clear();
			property.MonoCecil.GetMethod.Body.Instructions.Clear();
			CreateCallMethodInstructions(group.CalledMethodBeforeGetProperty).ForEach(property.MonoCecil.GetMethod.Body.Instructions.Add);
			property.MonoCecil.GetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
			property.MonoCecil.GetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, field));
			property.MonoCecil.GetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

			property.MonoCecil.GetMethod.RemoveAttributes<CompilerGeneratedAttribute>();

			log.Info("Get method body was generated");
		}
		private void GenerateSetMethodBody(CommonAssembly assembly, IHasMethods viewModelBaseType, CommonType viewModelType, PropertyGroup group, FieldReference field) {
			log.Info("Generate set method body...");

			var property = group.Property;
			if (property.SetMethod == null) {
				log.Debug($"Create set accessor method for property '{property.Name}'");
				var setMethod = new MethodDefinition($"set_{property.Name}", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName, assembly.MonoCecil.MainModule.TypeSystem.Void);
				setMethod.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, property.MonoCecil.PropertyType));

				property.MonoCecil.SetMethod = setMethod;
				viewModelType.MonoCecil.Methods.Add(setMethod);
			}

			var setMethodFromViewModelBaseWithGeneric = new GenericInstanceMethod(viewModelBaseType.GetMethod("Set", new[] { typeof(string).FullName, "T&", "T", typeof(bool).FullName }, true).MonoCecil);
			setMethodFromViewModelBaseWithGeneric.GenericArguments.Add(property.MonoCecil.PropertyType);

			var importedSetMethodFromViewModelBaseWithGeneric = assembly.MonoCecil.MainModule.ImportReference(setMethodFromViewModelBaseWithGeneric);
			var callMethodAfterSetPropertyInstructions = CreateCallMethodInstructions(group.CalledMethodAfterSetProperty).ToArray();

			property.MonoCecil.SetMethod.Body.Variables.Clear();
			property.MonoCecil.SetMethod.Body.Instructions.Clear();

			CreateCallMethodInstructions(group.CalledMethodBeforeSetProperty).ForEach(property.MonoCecil.SetMethod.Body.Instructions.Add);
			property.MonoCecil.SetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
			property.MonoCecil.SetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldstr, property.Name));
			property.MonoCecil.SetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
			property.MonoCecil.SetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldflda, field));
			property.MonoCecil.SetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
			property.MonoCecil.SetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
			property.MonoCecil.SetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Call, importedSetMethodFromViewModelBaseWithGeneric));

			var returnInstruction = Instruction.Create(OpCodes.Ret);
			if (group.CalledMethodAfterSuccessSetProperty != null) {
				var firstInstructionAfterJump = callMethodAfterSetPropertyInstructions.FirstOrDefault() ?? returnInstruction;
				property.MonoCecil.SetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Brfalse_S, firstInstructionAfterJump));
				CreateCallMethodInstructions(group.CalledMethodAfterSuccessSetProperty).ForEach(property.MonoCecil.SetMethod.Body.Instructions.Add);
			}
			else {
				property.MonoCecil.SetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Pop));
			}

			callMethodAfterSetPropertyInstructions.ForEach(property.MonoCecil.SetMethod.Body.Instructions.Add);
			property.MonoCecil.SetMethod.Body.Instructions.Add(returnInstruction);

			property.MonoCecil.SetMethod.RemoveAttributes<CompilerGeneratedAttribute>();

			log.Info("Set method body was generated");
		}
		private static IEnumerable<Instruction> CreateCallMethodInstructions(CommonMethod calledMethod) {
			if (calledMethod == null)
				yield break;

			if (!calledMethod.MonoCecil.IsStatic)
				yield return Instruction.Create(OpCodes.Ldarg_0);

			yield return Instruction.Create(calledMethod.MonoCecil.IsAbstract || calledMethod.MonoCecil.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, calledMethod.MonoCecil);

			if (calledMethod.ReturnType != typeof(void))
				yield return Instruction.Create(OpCodes.Pop);
		}
	}
}