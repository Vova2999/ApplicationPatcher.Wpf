using FluentAssertions;
using GalaSoft.MvvmLight;
using JetBrains.Annotations;
using NUnit.Framework;

namespace ApplicationPatcher.Wpf.Tests.Integration.ViewModels.Commands {
	public class ViewModelWithSimpleCommands : ViewModelBase {
		public int Value;

		[UsedImplicitly]
		public void ExecuteAnyActionMethod() {
			Value = 5;
		}
	}

	[TestFixture]
	public class ViewModelCommandsTestsWithSimpleCommands : ViewModelCommandsTestsBase {
		[Test]
		public void ExecuteCommandTest() {
			var viewModel = new ViewModelWithSimpleCommands();

			ExecuteCommand(viewModel, "AnyActionCommand");
			viewModel.Value.Should().Be(5);
		}
	}
}