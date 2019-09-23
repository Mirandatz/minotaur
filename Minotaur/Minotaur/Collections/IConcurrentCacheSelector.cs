namespace Minotaur.Collections {
	using System;

	public static class IConcurrentCacheSelector {

		public static IConcurrentCache<TKey, TValue> Create<TKey, TValue>(int capacity)
			where TKey : notnull
			where TValue : class {
			if (capacity < 0)
				throw new ArgumentOutOfRangeException(nameof(capacity) + " must be >= 0.");

			if (capacity == 0)
				return new NullCache<TKey, TValue>();
			else
				return new ConcurrentLruCache<TKey, TValue>(capacity: capacity);
		}
	}
}
