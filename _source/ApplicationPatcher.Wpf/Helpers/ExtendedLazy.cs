using System;

namespace ApplicationPatcher.Wpf.Helpers {
	public abstract class ExtendedLazy<TValue> {
		private TValue value;
		private bool valueReaded;

		protected TValue GetValueInternal(Func<TValue> getValue) {
			if (valueReaded)
				return value;

			valueReaded = true;
			return value = getValue();
		}
	}

	public class ExtendedLazy<TParam1, TValue> : ExtendedLazy<TValue> {
		private readonly Func<TParam1, TValue> getValue;

		public ExtendedLazy(Func<TParam1, TValue> getValue) {
			this.getValue = getValue;
		}

		public TValue GetValue(TParam1 param1) {
			return GetValueInternal(() => getValue(param1));
		}
	}
}