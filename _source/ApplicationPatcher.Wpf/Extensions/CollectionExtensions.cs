using System.Collections.Generic;

namespace ApplicationPatcher.Wpf.Extensions {
	public static class CollectionExtensions {
		public static void AddIfNotContains<TValue>(this ICollection<TValue> collection, TValue value) {
			if (!collection.Contains(value))
				collection.Add(value);
		}
	}
}