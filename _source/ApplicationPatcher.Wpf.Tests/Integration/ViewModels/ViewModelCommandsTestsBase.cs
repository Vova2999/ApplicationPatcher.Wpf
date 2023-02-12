using System.Windows.Input;
using FluentAssertions;
using GalaSoft.MvvmLight;

namespace ApplicationPatcher.Wpf.Tests.Integration.ViewModels {
	public abstract class ViewModelCommandsTestsBase {
		protected static bool CanExecuteCommand<TViewModel>(TViewModel viewModel, string propertyName, object parameter = null) where TViewModel : ViewModelBase {
			var propertyInfo = typeof(TViewModel).GetProperty(propertyName);
			propertyInfo.Should().NotBeNull();

			var command = propertyInfo?.GetValue(viewModel) as ICommand;
			command.Should().NotBeNull();

			return command?.CanExecute(parameter) == true;
		}

		protected static void ExecuteCommand<TViewModel>(TViewModel viewModel, string propertyName, object parameter = null) where TViewModel : ViewModelBase {
			var propertyInfo = typeof(TViewModel).GetProperty(propertyName);
			propertyInfo.Should().NotBeNull();

			var command = propertyInfo?.GetValue(viewModel) as ICommand;
			command.Should().NotBeNull();

			command?.Execute(parameter);
		}
	}
}