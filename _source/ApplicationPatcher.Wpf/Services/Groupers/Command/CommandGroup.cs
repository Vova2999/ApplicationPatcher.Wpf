using ApplicationPatcher.Core.Types.CommonMembers;

namespace ApplicationPatcher.Wpf.Services.Groupers.Command {
	public class CommandGroup {
		public readonly CommonField CommandField;
		public readonly CommonProperty CommandProperty;
		public readonly CommonMethod CommandExecuteMethod;
		public readonly CommonMethod CommandCanExecuteMethod;

		public CommandGroup(CommonField commandField, CommonProperty commandProperty, CommonMethod commandExecuteMethod, CommonMethod commandCanExecuteMethod) {
			CommandField = commandField;
			CommandProperty = commandProperty;
			CommandExecuteMethod = commandExecuteMethod;
			CommandCanExecuteMethod = commandCanExecuteMethod;
		}
	}
}