using ApplicationPatcher.Core.Types.CommonInterfaces;

namespace ApplicationPatcher.Wpf.Services.Groupers.Command {
	public class CommandGroup {
		public readonly ICommonField CommandField;
		public readonly ICommonProperty CommandProperty;
		public readonly ICommonMethod CommandExecuteMethod;
		public readonly ICommonMethod CommandCanExecuteMethod;

		public CommandGroup(ICommonField commandField, ICommonProperty commandProperty, ICommonMethod commandExecuteMethod, ICommonMethod commandCanExecuteMethod) {
			CommandField = commandField;
			CommandProperty = commandProperty;
			CommandExecuteMethod = commandExecuteMethod;
			CommandCanExecuteMethod = commandCanExecuteMethod;
		}
	}
}