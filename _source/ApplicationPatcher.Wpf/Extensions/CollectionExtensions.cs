using System;
using System.Collections.Generic;
using System.Linq;

namespace ApplicationPatcher.Wpf.Extensions {
	public static class CollectionExtensions {
		public static void AddIfNotContains<TValue>(this ICollection<TValue> collection, TValue value) {
			if (!collection.Contains(value))
				collection.Add(value);
		}

		public static TValue GetOrAdd<TValue>(this ICollection<TValue> collection, Func<TValue, bool> selector, Func<TValue> createValue) {
			foreach (var collectionValue in collection.Where(selector))
				return collectionValue;

			var newValue = createValue();
			collection.Add(newValue);
			return newValue;
		}
	}
}