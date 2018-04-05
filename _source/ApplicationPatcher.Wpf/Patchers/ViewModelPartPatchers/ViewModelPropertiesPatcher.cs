﻿using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ApplicationPatcher.Core.Extensions;
using ApplicationPatcher.Core.Helpers;
using ApplicationPatcher.Core.Types.Common;
using ApplicationPatcher.Wpf.Exceptions;
using ApplicationPatcher.Wpf.Types.Attributes.Properties;
using ApplicationPatcher.Wpf.Types.Enums;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ApplicationPatcher.Wpf.Patchers.ViewModelPartPatchers {
	public class ViewModelPropertiesPatcher : IViewModelPartPatcher {
		private readonly Log log;

		public ViewModelPropertiesPatcher() {
			log = Log.For(this);
		}

		[DoNotAddLogOffset]
		public void Patch(CommonAssembly assembly, CommonType viewModelBase, CommonType viewModel, ViewModelPatchingType viewModelPatchingType) {
			log.Info($"Patching {viewModel.FullName} properties...");

			var properties = GetCommonProperties(viewModel, viewModelPatchingType);
			if (!properties.Any()) {
				log.Info("Not found properties");
				return;
			}

			log.Debug("Properties found:", properties.Select(property => property.FullName));

			foreach (var property in properties) {
				log.Info($"Patching {property.FullName}...");
				PatchProprty(assembly, viewModelBase, viewModel, property);
				log.Info($"{property.FullName} was patched");
			}

			log.Info($"Properties {viewModel.FullName} was patched");
		}

		[DoNotAddLogOffset]
		private CommonProperty[] GetCommonProperties(CommonType viewModel, ViewModelPatchingType viewModelPatchingType) {
			switch (viewModelPatchingType) {
				case ViewModelPatchingType.All:
					return viewModel.Properties
						.Where(property =>
							property.NotContainsAttribute(typeof(NotPatchingPropertyAttribute)) &&
							(property.ContainsAttribute(typeof(PatchingPropertyAttribute)) || property.IsNotInheritedFrom(typeof(ICommand))))
						.ToArray();
				case ViewModelPatchingType.Selectively:
					return viewModel.Properties
						.Where(property => property.ContainsAttribute(typeof(PatchingPropertyAttribute)))
						.ToArray();
				default:
					log.Error($"Not implement patching for properties with {nameof(ViewModelPatchingType)} = {viewModelPatchingType}");
					throw new ArgumentOutOfRangeException(nameof(viewModelPatchingType), viewModelPatchingType, null);
			}
		}

		private void PatchProprty(CommonAssembly assembly, CommonType viewModelBase, CommonType viewModel, CommonProperty property) {
			CheckProperty(property);

			var propertyName = property.Name;
			log.Debug($"Property name: {propertyName}");

			var backgroundFieldName = $"{char.ToLower(propertyName.First())}{propertyName.Substring(1)}";
			log.Debug($"Background field name: {backgroundFieldName}");

			var backgroundField = viewModel.Fields.FirstOrDefault(field => field.Name == backgroundFieldName)?.MonoCecilField;

			if (backgroundField == null) {
				backgroundField = new FieldDefinition(backgroundFieldName, FieldAttributes.Private, property.MonoCecilProperty.PropertyType);
				viewModel.MonoCecilType.Fields.Add(backgroundField);
				log.Debug("Background field was created");
			}
			else
				log.Debug("Background field was connected");

			GenerateGetMethodBody(property.MonoCecilProperty, backgroundField);
			GenerateSetMethodBody(assembly, viewModelBase, property.MonoCecilProperty, propertyName, backgroundField);
		}

		[DoNotAddLogOffset]
		private void CheckProperty(CommonProperty property) {
			if (property.IsInheritedFrom(typeof(ICommand))) {
				const string errorMessage = "Patching property type cannot be inherited from ICommand";

				log.Error(errorMessage);
				throw new PropertyPatchingException(errorMessage, property.FullName);
			}

			// ReSharper disable once InvertIf
			if (char.IsLower(property.Name.First())) {
				const string errorMessage = "First character of property name must be to upper case";

				log.Error(errorMessage);
				throw new PropertyPatchingException(errorMessage, property.FullName);
			}
		}

		[DoNotAddLogOffset]
		private void GenerateGetMethodBody(PropertyDefinition property, FieldDefinition backgroundField) {
			log.Info("Generate get method body...");
			var propertyGetMethod = property.GetMethod;
			if (propertyGetMethod == null) {
				const string errorMessage = "Patching property must have get method accessor";

				log.Error(errorMessage);
				throw new PropertyPatchingException(errorMessage, property.FullName);
			}

			var getMethodBodyInstructions = propertyGetMethod.Body.Instructions;
			getMethodBodyInstructions.Clear();
			getMethodBodyInstructions.Add(Instruction.Create(OpCodes.Ldarg_0));
			getMethodBodyInstructions.Add(Instruction.Create(OpCodes.Ldfld, backgroundField));
			getMethodBodyInstructions.Add(Instruction.Create(OpCodes.Ret));

			RemoveCompilerGeneratedAttribute(propertyGetMethod);
			log.Info("Get method body was generated");
		}

		[DoNotAddLogOffset]
		private void GenerateSetMethodBody(CommonAssembly assembly, CommonType viewModelBase, PropertyDefinition property, string propertyName, FieldDefinition backgroundField) {
			log.Info("Generate method reference on Set method in ViewModelBase...");
			var propertySetMethod = property.SetMethod;
			if (propertySetMethod == null) {
				const string errorMessage = "Patching property must have set method accessor";

				log.Error(errorMessage);
				throw new PropertyPatchingException(errorMessage, property.FullName);
			}

			var setMethodFromViewModelBase = new GenericInstanceMethod(GetSetMethodFromViewModelBase(viewModelBase.MonoCecilType));
			setMethodFromViewModelBase.GenericArguments.Add(property.PropertyType);
			var setMethodInViewModelBaseWithGenericParameter = assembly.MonoCecilAssembly.MainModule.ImportReference(setMethodFromViewModelBase);

			log.Info("Generate set method body...");
			var setMethodBodyInstructions = propertySetMethod.Body.Instructions;
			setMethodBodyInstructions.Clear();
			setMethodBodyInstructions.Add(Instruction.Create(OpCodes.Ldarg_0));
			setMethodBodyInstructions.Add(Instruction.Create(OpCodes.Ldstr, propertyName));
			setMethodBodyInstructions.Add(Instruction.Create(OpCodes.Ldarg_0));
			setMethodBodyInstructions.Add(Instruction.Create(OpCodes.Ldflda, backgroundField));
			setMethodBodyInstructions.Add(Instruction.Create(OpCodes.Ldarg_1));
			setMethodBodyInstructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
			setMethodBodyInstructions.Add(Instruction.Create(OpCodes.Call, setMethodInViewModelBaseWithGenericParameter));
			setMethodBodyInstructions.Add(Instruction.Create(OpCodes.Pop));
			setMethodBodyInstructions.Add(Instruction.Create(OpCodes.Ret));

			RemoveCompilerGeneratedAttribute(propertySetMethod);
			log.Info("Set method body was generated");
		}

		[DoNotAddLogOffset]
		private static void RemoveCompilerGeneratedAttribute(MethodDefinition method) {
			var compilerGeneratedAttribute = method.CustomAttributes
				.FirstOrDefault(attribute => attribute.AttributeType.FullName == typeof(CompilerGeneratedAttribute).FullName);
			if (compilerGeneratedAttribute != null)
				method.CustomAttributes.Remove(compilerGeneratedAttribute);
		}

		private static MethodDefinition setMethodInViewModelBase;
		private static MethodDefinition GetSetMethodFromViewModelBase(TypeDefinition viewModelBaseType) {
			return setMethodInViewModelBase ?? (setMethodInViewModelBase =
				viewModelBaseType.Methods.Single(method =>
					method.Name == "Set" &&
					method.Parameters.Count == 4 &&
					method.Parameters[0].ParameterType.FullName == typeof(string).FullName &&
					method.Parameters[1].ParameterType.FullName == "T&" &&
					method.Parameters[2].ParameterType.FullName == "T" &&
					method.Parameters[3].ParameterType.FullName == typeof(bool).FullName));
		}
	}
}