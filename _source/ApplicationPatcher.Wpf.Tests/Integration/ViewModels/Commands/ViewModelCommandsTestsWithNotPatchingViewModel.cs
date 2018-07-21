using ApplicationPatcher.Wpf.Types.Attributes;
using FluentAssertions;
using GalaSoft.MvvmLight;
using JetBrains.Annotations;
using NUnit.Framework;

namespace ApplicationPatcher.Wpf.Tests.Integration.ViewModels.Commands {
	[NotPatchingViewModel]
	public class ViewModelWithNotPatchingViewModel : ViewModelBase {
		[UsedImplicitly]
		public void ExecuteAnyActionMethod() {
		}
	}

	[TestFixture]
	public class ViewModelCommandsTestsWithNotPatchingViewModel : ViewModelCommandsTestsBase {
		[Test]
		public void CommandMissingTest() {
			typeof(ViewModelWithNotPatchingViewModel).GetProperty("AnyActionCommand").Should().BeNull();
		}
	}
}