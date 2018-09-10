using System;

namespace ApplicationPatcher.Wpf.Types.Attributes.SelectPatching {
	[AttributeUsage(AttributeTargets.Property)]
	public class PatchingPropertyAttribute : Attribute {
		public string CalledMethodNameBeforeGetProperty { get; set; }
		public string CalledMethodNameBeforeSetProperty { get; set; }
		public string CalledMethodNameAfterSuccessSetProperty { get; set; }
		public string CalledMethodNameAfterSetProperty { get; set; }
	}
}