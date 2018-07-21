using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using GalaSoft.MvvmLight;
using NUnit.Framework;

namespace ApplicationPatcher.Wpf.Tests.Integration.ViewModels {
	public abstract class ViewModelPropertiesTestsBase {
		private List<string> savedChangedPropertyNames;

		[SetUp]
		public void SetUp() {
			savedChangedPropertyNames = new List<string>();
		}

		protected TViewModel CreateViewModel<TViewModel>() where TViewModel : ViewModelBase, new() {
			var viewModel = new TViewModel();
			viewModel.PropertyChanged += (sender, args) => savedChangedPropertyNames.Add(args.PropertyName);

			return viewModel;
		}

		protected void CheckChangedProperties(params string[] changedPropertyNames) {
			savedChangedPropertyNames.SequenceEqual(changedPropertyNames).Should().BeTrue();
		}
	}
}