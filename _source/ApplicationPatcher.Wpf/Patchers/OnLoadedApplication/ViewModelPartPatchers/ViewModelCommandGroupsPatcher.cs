using System;
using System.Linq;
using System.Runtime.CompilerServices;
using ApplicationPatcher.Core;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Core.Logs;
using ApplicationPatcher.Core.Types.CommonInterfaces;
using ApplicationPatcher.Wpf.Extensions;
using ApplicationPatcher.Wpf.Helpers;
using ApplicationPatcher.Wpf.Services.Groupers.Command;
using ApplicationPatcher.Wpf.Services.NameRules;
using ApplicationPatcher.Wpf.Types.Enums;
using JetBrains.Annotations;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace ApplicationPatcher.Wpf.Patchers.OnLoadedApplication.ViewModelPartPatchers {
	[UsedImplicitly]
	public class ViewModelCommandGroupsPatcher : ViewModelPartPatcher {
		private readonly CommandGrouperService commandGrouperService;
		private readonly NameRulesService nameRulesService;
		private readonly ILog log;

		private readonly ExtendedLazy<ICommonAssembly, ICommonType> commandType =
			new ExtendedLazy<ICommonAssembly, ICommonType>(assembly => assembly.GetCommonType(KnownTypeNames.ICommand, true));

		private readonly ExtendedLazy<ICommonAssembly, MethodReference> actionConstructor =
			new ExtendedLazy<ICommonAssembly, MethodReference>(GetActionConstructor);

		private readonly ExtendedLazy<ICommonAssembly, MethodReference> funcBoolConstructor =
			new ExtendedLazy<ICommonAssembly, MethodReference>(assembly => GetFuncConstructor(assembly, assembly.MonoCecil.MainModule.TypeSystem.Boolean));

		private readonly ExtendedLazy<ICommonAssembly, MethodReference> relayCommandConstructor =
			new ExtendedLazy<ICommonAssembly, MethodReference>(GetRelayCommandConstructor);

		private readonly ExtendedLazy<ICommonAssembly, MethodReference> relayCommandConstructorWithCanExecuteMethod =
			new ExtendedLazy<ICommonAssembly, MethodReference>(GetRelayCommandConstructorWithCanExecuteMethod);

		public ViewModelCommandGroupsPatcher(CommandGrouperService commandGrouperService, NameRulesService nameRulesService) {
			this.commandGrouperService = commandGrouperService;
			this.nameRulesService = nameRulesService;
			log = Log.For(this);
		}

		public override PatchResult Patch(ICommonAssembly assembly, ICommonType viewModelBaseType, ICommonType viewModelType, ViewModelPatchingType patchingType) {
			log.Info("Patching command groups...");

			var commandGroups = commandGrouperService.GetGroups(assembly, viewModelType, patchingType);
			if (!commandGroups.Any()) {
				log.Info("Not found command groups");
				return PatchResult.Continue;
			}

			log.Debug("Command groups found:",
				commandGroups.Select(group =>
					$"Execute method: '{group.CommandExecuteMethod.Name}', " +
					$"can execute method: '{group.CommandCanExecuteMethod?.Name}', " +
					$"property: '{group.CommandProperty?.Name}', " +
					$"field: '{group.CommandField?.Name}'"));

			foreach (var group in commandGroups) {
				log.Info("Patch group: " +
					$"Execute method: '{group.CommandExecuteMethod.Name}', " +
					$"can execute method: '{group.CommandCanExecuteMethod?.Name}', " +
					$"property: '{group.CommandProperty?.Name}', " +
					$"field: '{group.CommandField?.Name}'...");

				PatchGroup(assembly, viewModelType, group);
				log.Info("Group was patched");
			}

			log.Info("Command groups was patched");
			return PatchResult.Continue;
		}

		[AddLogOffset]
		private void PatchGroup(ICommonAssembly assembly, ICommonType viewModelType, CommandGroup group) {
			var property = group.CommandProperty?.MonoCecil ?? CreateProperty(assembly, viewModelType, group.CommandExecuteMethod.Name);
			RemoveDefaultField(viewModelType, property.Name);

			var field = group.CommandField?.MonoCecil ?? CreateField(viewModelType, property);

			RemoveSetMethod(viewModelType, property);
			GenerateGetMethodBody(assembly, viewModelType, group.CommandExecuteMethod, group.CommandCanExecuteMethod, property, field);
		}

		private PropertyDefinition CreateProperty(ICommonAssembly assembly, ICommonType viewModelType, string executeMethodName) {
			var propertyName = nameRulesService.ConvertName(executeMethodName, UseNameRulesFor.CommandExecuteMethod, UseNameRulesFor.CommandProperty);

			log.Debug($"Create property with name '{propertyName}'");
			var property = new PropertyDefinition(propertyName, PropertyAttributes.None, assembly.MonoCecil.MainModule.ImportReference(commandType.GetValue(assembly).MonoCecil));
			viewModelType.MonoCecil.Properties.Add(property);

			return property;
		}
		private void RemoveDefaultField(ICommonType viewModelType, string propertyName) {
			var defaultFieldName = $"<{propertyName}>k__BackingField";
			if (!viewModelType.TryGetField(defaultFieldName, out var defaultField))
				return;

			log.Debug($"Remove field with name '{defaultFieldName}'");
			viewModelType.MonoCecil.Fields.Remove(defaultField.MonoCecil);
		}

		private FieldDefinition CreateField(ICommonType viewModelType, PropertyReference property) {
			var fieldName = nameRulesService.ConvertName(property.Name, UseNameRulesFor.CommandProperty, UseNameRulesFor.CommandField);

			log.Debug($"Create field with name '{fieldName}'");
			var field = new FieldDefinition(fieldName, FieldAttributes.Private, property.PropertyType);
			viewModelType.MonoCecil.Fields.Add(field);

			return field;
		}

		private void RemoveSetMethod(ICommonType viewModelType, PropertyDefinition property) {
			if (property.SetMethod == null)
				return;

			log.Debug($"Remove set accessor method for property '{property.Name}'");
			viewModelType.MonoCecil.Methods.Remove(property.SetMethod);
			property.SetMethod = null;
		}
		private void GenerateGetMethodBody(ICommonAssembly assembly, ICommonType viewModelType, ICommonMethod executeMethod, ICommonMethod canExecuteMethod, PropertyDefinition property, FieldReference field) {
			log.Info("Generate get method body...");

			if (property.GetMethod == null) {
				log.Debug($"Create get accessor method for property '{property.Name}'");
				var getMethod = new MethodDefinition($"get_{property.Name}", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName, property.PropertyType);

				property.GetMethod = getMethod;
				viewModelType.MonoCecil.Methods.Add(getMethod);
			}

			var returnInstruction = Instruction.Create(OpCodes.Ret);

			property.GetMethod.Body.Variables.Clear();
			property.GetMethod.Body.InitLocals = true;
			property.GetMethod.Body.Variables.Add(new VariableDefinition(assembly.MonoCecil.MainModule.ImportReference(commandType.GetValue(assembly).MonoCecil)));

			property.GetMethod.Body.Instructions.Clear();
			property.GetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
			property.GetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, field));
			property.GetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Dup));
			property.GetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Brtrue_S, returnInstruction));
			property.GetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Pop));
			property.GetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
			property.GetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
			property.GetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldftn, executeMethod.MonoCecil));

			property.GetMethod.Body.Instructions.Add(executeMethod.ParameterTypes.Any()
				? Instruction.Create(OpCodes.Newobj, GetActionConstructor(assembly, executeMethod.MonoCecil.Parameters.Single().ParameterType))
				: Instruction.Create(OpCodes.Newobj, actionConstructor.GetValue(assembly)));

			if (canExecuteMethod == null) {
				property.GetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
				property.GetMethod.Body.Instructions.Add(executeMethod.ParameterTypes.Any()
					? Instruction.Create(OpCodes.Newobj, GetRelayCommandConstructor(assembly, executeMethod.MonoCecil.Parameters.Single().ParameterType))
					: Instruction.Create(OpCodes.Newobj, relayCommandConstructor.GetValue(assembly)));
			}
			else {
				property.GetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
				property.GetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldftn, canExecuteMethod.MonoCecil));

				property.GetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Newobj,
					executeMethod.ParameterTypes.Any()
						? GetFuncConstructor(assembly, executeMethod.MonoCecil.Parameters.Single().ParameterType, assembly.MonoCecil.MainModule.TypeSystem.Boolean)
						: funcBoolConstructor.GetValue(assembly)));

				property.GetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));

				property.GetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Newobj,
					executeMethod.ParameterTypes.Any()
						? GetRelayCommandConstructorWithCanExecuteMethod(assembly, executeMethod.MonoCecil.Parameters.Single().ParameterType)
						: relayCommandConstructorWithCanExecuteMethod.GetValue(assembly)));
			}

			property.GetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Dup));
			property.GetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Stloc_0));
			property.GetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Stfld, field));
			property.GetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldloc_0));
			property.GetMethod.Body.Instructions.Add(returnInstruction);

			property.GetMethod.RemoveAttributes<CompilerGeneratedAttribute>();

			log.Info("Get method body was generated");
		}

		private static MethodReference GetActionConstructor(ICommonAssembly assembly) {
			return assembly.MonoCecil.MainModule.ImportReference(assembly.GetCommonType(typeof(Action), true).Load().GetConstructor(new[] { typeof(object), typeof(IntPtr) }, true).MonoCecil);
		}

		private static MethodReference GetActionConstructor(ICommonAssembly assembly, TypeReference returnType) {
			var actionConstructor = assembly.GetCommonType(typeof(Action<>), true).Load().GetConstructor(new[] { typeof(object), typeof(IntPtr) }, true).MonoCecil;
			return assembly.MonoCecil.MainModule.ImportReference(actionConstructor.MakeHostInstanceGeneric(returnType));
		}

		private static MethodReference GetFuncConstructor(ICommonAssembly assembly, TypeReference returnType) {
			var funcConstructor = assembly.GetCommonType(typeof(Func<>), true).Load().GetConstructor(new[] { typeof(object), typeof(IntPtr) }, true).MonoCecil;
			return assembly.MonoCecil.MainModule.ImportReference(funcConstructor.MakeHostInstanceGeneric(returnType));
		}

		private static MethodReference GetFuncConstructor(ICommonAssembly assembly, TypeReference parameterType, TypeReference returnType) {
			var funcConstructor = assembly.GetCommonType(typeof(Func<,>), true).Load().GetConstructor(new[] { typeof(object), typeof(IntPtr) }, true).MonoCecil;
			return assembly.MonoCecil.MainModule.ImportReference(funcConstructor.MakeHostInstanceGeneric(parameterType, returnType));
		}

		private static MethodReference GetRelayCommandConstructor(ICommonAssembly assembly) {
			return assembly.MonoCecil.MainModule.ImportReference(assembly.GetCommonType(KnownTypeNames.RelayCommand, true).Load().GetConstructor(new[] { typeof(Action), typeof(bool) }, true).MonoCecil);
		}

		private static MethodReference GetRelayCommandConstructor(ICommonAssembly assembly, TypeReference parameterType) {
			var relayCommandConstructor = assembly.GetCommonType(KnownTypeNames.RelayCommandT, true).Load().GetConstructor(new[] { typeof(Action<>).Name, typeof(bool).FullName }, true).MonoCecil;
			return assembly.MonoCecil.MainModule.ImportReference(relayCommandConstructor.MakeHostInstanceGeneric(parameterType));
		}

		private static MethodReference GetRelayCommandConstructorWithCanExecuteMethod(ICommonAssembly assembly) {
			return assembly.MonoCecil.MainModule.ImportReference(assembly.GetCommonType(KnownTypeNames.RelayCommand, true).Load().GetConstructor(new[] { typeof(Action), typeof(Func<bool>), typeof(bool) }, true).MonoCecil);
		}

		private static MethodReference GetRelayCommandConstructorWithCanExecuteMethod(ICommonAssembly assembly, TypeReference parameterType) {
			var funcConstructor = assembly.GetCommonType(KnownTypeNames.RelayCommandT, true).Load().GetConstructor(new[] { typeof(Action<>).Name, typeof(Func<,>).Name, typeof(bool).FullName }, true).MonoCecil;
			return assembly.MonoCecil.MainModule.ImportReference(funcConstructor.MakeHostInstanceGeneric(parameterType));
		}
	}
}