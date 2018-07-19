using System;

// ReSharper disable UnusedMember.Global

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

	public class ExtendedLazy<TParam1, TParam2, TValue> : ExtendedLazy<TValue> {
		private readonly Func<TParam1, TParam2, TValue> getValue;

		public ExtendedLazy(Func<TParam1, TParam2, TValue> getValue) {
			this.getValue = getValue;
		}

		public TValue GetValue(TParam1 param1, TParam2 param2) {
			return GetValueInternal(() => getValue(param1, param2));
		}
	}

	public class ExtendedLazy<TParam1, TParam2, TParam3, TValue> : ExtendedLazy<TValue> {
		private readonly Func<TParam1, TParam2, TParam3, TValue> getValue;

		public ExtendedLazy(Func<TParam1, TParam2, TParam3, TValue> getValue) {
			this.getValue = getValue;
		}

		public TValue GetValue(TParam1 param1, TParam2 param2, TParam3 param3) {
			return GetValueInternal(() => getValue(param1, param2, param3));
		}
	}
}