using System;
using System.Collections.Generic;

namespace ApplicationPatcher.Wpf.Extensions {
	public static class DictionaryExtensions {
		public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> getDefaultValue) {
			return dictionary.TryGetValue(key, out var value) ? value : dictionary[key] = getDefaultValue();
		}
	}
}