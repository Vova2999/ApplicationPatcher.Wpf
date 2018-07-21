using FluentAssertions;
using GalaSoft.MvvmLight;
using JetBrains.Annotations;
using NUnit.Framework;

namespace ApplicationPatcher.Wpf.Tests.Integration.ViewModels.Commands {
	public class ViewModelWithCanExecuteMethod : ViewModelBase {
		public int Value;

		[UsedImplicitly]
		public bool CanExecuteAnyActionMethod() {
			return Value == 2;
		}

		[UsedImplicitly]
		public void ExecuteAnyActionMethod() {
			Value = 5;
		}
	}

	[TestFixture]
	public class ViewModelCommandsTestsWithCanExecuteMethod : ViewModelCommandsTestsBase {
		[Test]
		public void CanExecuteCommandTest() {
			var viewModel = new ViewModelWithCanExecuteMethod();

			CanExecuteCommand(viewModel, "AnyActionCommand").Should().BeFalse();

			viewModel.Value = 2;
			CanExecuteCommand(viewModel, "AnyActionCommand").Should().BeTrue();

			ExecuteCommand(viewModel, "AnyActionCommand");
			viewModel.Value.Should().Be(5);
		}
	}
}