using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using FluentAssertions;
using NUnit.Framework;

namespace ApplicationPatcher.Wpf.Tests.Integration.FrameworkElements {
	[TestFixture]
	public class MyUserControlTests {
		private const BindingFlags bindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

		[Test]
		public void CheckDependencyField() {
			var fieldInfo = typeof(MyUserControl).GetField($"{nameof(MyUserControl.MyValue)}Property", bindingFlags);
			fieldInfo.Should().NotBeNull();
		}

		[Test, Apartment(ApartmentState.STA)]
		public void CheckSetAndGetValue() {
			var myUserControl = new MyUserControl();

			var fieldInfo = typeof(MyUserControl).GetField($"{nameof(MyUserControl.MyValue)}Property", bindingFlags);
			var setMethodInfo = typeof(DependencyObject).GetMethods(bindingFlags)
				.Single(info => info.Name == "SetValue" && info.GetParameters().Select(parameterInfo => parameterInfo.ParameterType).SequenceEqual(new[] { typeof(DependencyProperty), typeof(object) }));

			setMethodInfo.Invoke(myUserControl, new[] { fieldInfo?.GetValue(myUserControl), 123 });
			myUserControl.MyValue.Should().Be(123);
		}
	}
}