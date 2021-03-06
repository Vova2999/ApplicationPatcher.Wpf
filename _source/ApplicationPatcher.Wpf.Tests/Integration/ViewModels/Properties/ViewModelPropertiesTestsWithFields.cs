﻿using System.Diagnostics.CodeAnalysis;
using ApplicationPatcher.Wpf.Types.Attributes.Connect;
using FluentAssertions;
using GalaSoft.MvvmLight;
using NUnit.Framework;

namespace ApplicationPatcher.Wpf.Tests.Integration.ViewModels.Properties {
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public class ViewModelWithFields : ViewModelBase {
		public int firstProperty;
		public int FirstProperty { get; set; }

		public string secondAnyProperty;

		[ConnectPropertyToField(nameof(secondAnyProperty))]
		public string SecondProperty { get; set; }
	}

	[TestFixture]
	public class ViewModelPropertiesTestsWithFields : ViewModelPropertiesTestsBase {
		[Test]
		public void ChangeAllPropertiesTest() {
			var viewModel = CreateViewModel<ViewModelWithFields>();

			viewModel.FirstProperty = 123;
			viewModel.SecondProperty = "123";

			CheckChangedProperties(nameof(viewModel.FirstProperty), nameof(viewModel.SecondProperty));
			viewModel.firstProperty.Should().Be(123);
			viewModel.secondAnyProperty.Should().Be("123");
			viewModel.FirstProperty.Should().Be(123);
			viewModel.SecondProperty.Should().Be("123");
		}

		[Test]
		public void ChangeAllPropertiesWithRepeatTest() {
			var viewModel = CreateViewModel<ViewModelWithFields>();

			viewModel.FirstProperty = 123;
			viewModel.FirstProperty = 123;
			viewModel.SecondProperty = "123";
			viewModel.SecondProperty = "123";

			CheckChangedProperties(nameof(viewModel.FirstProperty), nameof(viewModel.SecondProperty));
			viewModel.firstProperty.Should().Be(123);
			viewModel.secondAnyProperty.Should().Be("123");
			viewModel.FirstProperty.Should().Be(123);
			viewModel.SecondProperty.Should().Be("123");
		}

		[Test]
		public void ChangeFieldTest() {
			var viewModel = CreateViewModel<ViewModelWithFields>();

			viewModel.firstProperty = 123;

			CheckChangedProperties();
			viewModel.firstProperty.Should().Be(123);
			viewModel.FirstProperty.Should().Be(123);
		}
	}
}