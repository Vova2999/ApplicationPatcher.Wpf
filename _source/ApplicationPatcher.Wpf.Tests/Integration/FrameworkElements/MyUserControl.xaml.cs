using JetBrains.Annotations;

namespace ApplicationPatcher.Wpf.Tests.Integration.FrameworkElements {
	public partial class MyUserControl {
		[UsedImplicitly]
		public int MyValue { get; set; }

		public MyUserControl() {
			InitializeComponent();
		}
	}
}