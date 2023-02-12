using FluentAssertions;
using GalaSoft.MvvmLight;
using NUnit.Framework;

namespace ApplicationPatcher.Wpf.Tests.Integration.ViewModels.Commands {
	public class ViewModelWithCanExecuteAndParameterCommands : ViewModelBase {
		public int Value;

		public bool CanExecuteAnyActionMethod(int value) {
			return value == 10;
		}
		public void ExecuteAnyActionMethod(int value) {
			Value = value;
		}
	}

	[TestFixture]
	public class ViewModelCommandsTestsWithCanExecuteAndParameterCommands : ViewModelCommandsTestsBase {
		[Test]
		public void CanExecuteCommandTest() {
			var viewModel = new ViewModelWithCanExecuteAndParameterCommands();

			CanExecuteCommand(viewModel, "AnyActionCommand", 5).Should().BeFalse();
			CanExecuteCommand(viewModel, "AnyActionCommand", 10).Should().BeTrue();

			ExecuteCommand(viewModel, "AnyActionCommand", 10);
			viewModel.Value.Should().Be(10);
		}
	}
}