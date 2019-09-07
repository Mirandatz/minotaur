namespace Minotaur.Collections {
	using System;

	public static class IConcurrentCacheCreator {

		public static IConcurrentCache<TKey, TValue> Create<TKey, TValue>(int capacity) {
			if (capacity < 0)
				throw new ArgumentOutOfRangeException(nameof(capacity) + " must be >= 0.");

			if (capacity == 0)
				return new NullCache<TKey, TValue>();

			return new ConcurrentLruCache<TKey, TValue>(capacity: capacity);
		}
	}
}
