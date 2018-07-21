using JetBrains.Annotations;

namespace ApplicationPatcher.Wpf.Extensions {
	public static class CharExtensions {
		[UsedImplicitly]
		public static char ToLower(this char symbol) {
			return char.ToLower(symbol);
		}

		[UsedImplicitly]
		public static char ToUpper(this char symbol) {
			return char.ToUpper(symbol);
		}
	}
}