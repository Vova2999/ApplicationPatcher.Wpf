using FluentAssertions;
using GalaSoft.MvvmLight;
using NUnit.Framework;

namespace ApplicationPatcher.Wpf.Tests.Integration.ViewModels.Commands {
	public class ViewModelWithParameterCommands : ViewModelBase {
		public int Value;

		public void ExecuteAnyActionMethod(int value) {
			Value = value;
		}
	}

	[TestFixture]
	public class ViewModelCommandsTestsWithParameterCommands : ViewModelCommandsTestsBase {
		[Test]
		public void ExecuteCommandTest() {
			var viewModel = new ViewModelWithParameterCommands();

			ExecuteCommand(viewModel, "AnyActionCommand", 10);
			viewModel.Value.Should().Be(10);
		}
	}
}