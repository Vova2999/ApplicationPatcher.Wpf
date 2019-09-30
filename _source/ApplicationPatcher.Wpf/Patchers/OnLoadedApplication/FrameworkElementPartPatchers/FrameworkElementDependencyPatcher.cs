using System;
using System.Linq;
using System.Runtime.CompilerServices;
using ApplicationPatcher.Core;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Core.Logs;
using ApplicationPatcher.Core.Types.CommonInterfaces;
using ApplicationPatcher.Wpf.Extensions;
using ApplicationPatcher.Wpf.Helpers;
using ApplicationPatcher.Wpf.Services.Groupers.Dependency;
using ApplicationPatcher.Wpf.Services.NameRules;
using ApplicationPatcher.Wpf.Types.Enums;
using JetBrains.Annotations;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace ApplicationPatcher.Wpf.Patchers.OnLoadedApplication.FrameworkElementPartPatchers {
	[UsedImplicitly]
	public class FrameworkElementDependencyPatcher : FrameworkElementPartPatcher {
		private readonly DependencyGrouperService dependencyGrouperService;
		private readonly NameRulesService nameRulesService;
		private readonly ILog log;

		private readonly ExtendedLazy<ICommonAssembly, ICommonType> dependencyPropertyType =
			new ExtendedLazy<ICommonAssembly, ICommonType>(assembly => assembly.GetCommonType(KnownTypeNames.DependencyProperty, true));

		private readonly ExtendedLazy<ICommonAssembly, MethodReference> getTypeFromHandleMethod =
			new ExtendedLazy<ICommonAssembly, MethodReference>(assembly => assembly.MonoCecil.MainModule.ImportReference(assembly.GetCommonType(typeof(Type), true).Load().GetMethod("GetTypeFromHandle", new[] { typeof(RuntimeTypeHandle) }, true).MonoCecil));

		private readonly ExtendedLazy<ICommonAssembly, MethodReference> registerMethod =
			new ExtendedLazy<ICommonAssembly, MethodReference>(assembly => assembly.MonoCecil.MainModule.ImportReference(assembly.GetCommonType(KnownTypeNames.DependencyProperty, true).Load().GetMethod("Register", new[] { typeof(string), typeof(Type), typeof(Type) }, true).MonoCecil));

		private readonly ExtendedLazy<ICommonAssembly, MethodReference> getValueMethod =
			new ExtendedLazy<ICommonAssembly, MethodReference>(assembly => assembly.MonoCecil.MainModule.ImportReference(assembly.GetCommonType(KnownTypeNames.DependencyObject, true).Load().GetMethod("GetValue", new[] { assembly.GetCommonType(KnownTypeNames.DependencyProperty, true).Type }).MonoCecil));

		private readonly ExtendedLazy<ICommonAssembly, MethodReference> setValueMethod =
			new ExtendedLazy<ICommonAssembly, MethodReference>(assembly => assembly.MonoCecil.MainModule.ImportReference(assembly.GetCommonType(KnownTypeNames.DependencyObject, true).Load().GetMethod("SetValue", new[] { assembly.GetCommonType(KnownTypeNames.DependencyProperty, true).Type, typeof(object) }).MonoCecil));

		public FrameworkElementDependencyPatcher(DependencyGrouperService dependencyGrouperService, NameRulesService nameRulesService) {
			this.dependencyGrouperService = dependencyGrouperService;
			this.nameRulesService = nameRulesService;
			log = Log.For(this);
		}

		public override PatchResult Patch(ICommonAssembly assembly, ICommonType frameworkElementType, FrameworkElementPatchingType patchingType) {
			log.Info("Patching dependency groups...");

			var propertyGroups = dependencyGrouperService.GetGroups(assembly, frameworkElementType, patchingType);
			if (!propertyGroups.Any()) {
				log.Info("Not found dependency groups");
				return PatchResult.Continue;
			}

			log.Debug("Dependency groups found:", propertyGroups.Select(group => $"Property: '{group.Property.Name}', dependency field: '{group.Field?.Name}'"));

			foreach (var group in propertyGroups) {
				log.Info($"Patch group: property: '{group.Property.Name}', dependency field: '{group.Field?.Name}'...");
				PatchGroup(assembly, frameworkElementType, group);
				log.Info("Group was patched");
			}

			log.Info("Dependency groups was patched");
			return PatchResult.Continue;
		}

		[AddLogOffset]
		private void PatchGroup(ICommonAssembly assembly, ICommonType frameworkElementType, DependencyGroup group) {
			RemoveDefaultField(frameworkElementType, group.Property.Name);

			var field = group.Field?.MonoCecil ?? CreateField(assembly, frameworkElementType, group.Property);

			GenerateGetMethodBody(assembly, frameworkElementType, group.Property.MonoCecil, field);
			GenerateSetMethodBody(assembly, frameworkElementType, group.Property.MonoCecil, field);
		}

		private void RemoveDefaultField(ICommonType frameworkElementType, string propertyName) {
			var defaultFieldName = $"<{propertyName}>k__BackingField";
			if (!frameworkElementType.TryGetField(defaultFieldName, out var defaultField))
				return;

			log.Debug($"Remove field with name '{defaultFieldName}'");
			frameworkElementType.MonoCecil.Fields.Remove(defaultField.MonoCecil);
		}

		private FieldDefinition CreateField(ICommonAssembly assembly, ICommonType frameworkElementType, ICommonProperty property) {
			var fieldName = nameRulesService.ConvertName(property.Name, UseNameRulesFor.DependencyProperty, UseNameRulesFor.DependencyField);

			log.Debug($"Create field with name '{fieldName}'");
			var field = new FieldDefinition(fieldName, FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.InitOnly, assembly.MonoCecil.MainModule.ImportReference(dependencyPropertyType.GetValue(assembly).MonoCecil));
			frameworkElementType.MonoCecil.Fields.Add(field);

			InitializeInStaticConstructor(assembly, frameworkElementType, property, field);

			return field;
		}
		private void InitializeInStaticConstructor(ICommonAssembly assembly, ICommonType frameworkElementType, ICommonProperty property, FieldReference field) {
			var staticConstructor = frameworkElementType.MonoCecil.GetStaticConstructor();
			if (staticConstructor == null) {
				staticConstructor = new MethodDefinition(".cctor", MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.Static, assembly.MonoCecil.MainModule.TypeSystem.Void);
				staticConstructor.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
				frameworkElementType.MonoCecil.Methods.Add(staticConstructor);
			}

			staticConstructor.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Ldstr, property.Name));
			staticConstructor.Body.Instructions.Insert(1, Instruction.Create(OpCodes.Ldtoken, property.MonoCecil.PropertyType));
			staticConstructor.Body.Instructions.Insert(2, Instruction.Create(OpCodes.Call, getTypeFromHandleMethod.GetValue(assembly)));
			staticConstructor.Body.Instructions.Insert(3, Instruction.Create(OpCodes.Ldtoken, frameworkElementType.MonoCecil));
			staticConstructor.Body.Instructions.Insert(4, Instruction.Create(OpCodes.Call, getTypeFromHandleMethod.GetValue(assembly)));
			staticConstructor.Body.Instructions.Insert(5, Instruction.Create(OpCodes.Call, registerMethod.GetValue(assembly)));
			staticConstructor.Body.Instructions.Insert(6, Instruction.Create(OpCodes.Stsfld, field));
		}

		private void GenerateGetMethodBody(ICommonAssembly assembly, ICommonType frameworkElementType, PropertyDefinition property, FieldReference field) {
			log.Info("Generate get method body...");

			if (property.GetMethod == null) {
				log.Debug($"Create get accessor method for property '{property.Name}'");
				var getMethod = new MethodDefinition($"get_{property.Name}", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName, property.PropertyType);

				property.GetMethod = getMethod;
				frameworkElementType.MonoCecil.Methods.Add(getMethod);
			}

			property.GetMethod.Body.Variables.Clear();

			property.GetMethod.Body.Instructions.Clear();
			property.GetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
			property.GetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldsfld, field));
			property.GetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Call, getValueMethod.GetValue(assembly)));
			property.GetMethod.Body.Instructions.Add(Instruction.Create(property.PropertyType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, property.PropertyType));
			property.GetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

			property.GetMethod.RemoveAttributes<CompilerGeneratedAttribute>();

			log.Info("Get method body was generated");
		}
		private void GenerateSetMethodBody(ICommonAssembly assembly, ICommonType frameworkElementType, PropertyDefinition property, FieldReference field) {
			log.Info("Generate set method body...");

			if (property.SetMethod == null) {
				log.Debug($"Create set accessor method for property '{property.Name}'");
				var setMethod = new MethodDefinition($"set_{property.Name}", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName, assembly.MonoCecil.MainModule.TypeSystem.Void);
				setMethod.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, property.PropertyType));

				property.SetMethod = setMethod;
				frameworkElementType.MonoCecil.Methods.Add(setMethod);
			}

			property.GetMethod.Body.Variables.Clear();

			property.SetMethod.Body.Instructions.Clear();
			property.SetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
			property.SetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldsfld, field));
			property.SetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));

			if (property.PropertyType.IsValueType)
				property.SetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Box, property.PropertyType));

			property.SetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Call, setValueMethod.GetValue(assembly)));
			property.SetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

			property.SetMethod.RemoveAttributes<CompilerGeneratedAttribute>();

			log.Info("Set method body was generated");
		}
	}
}