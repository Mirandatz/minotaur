namespace Minotaur.Collections {
	using System;
	using System.Diagnostics.CodeAnalysis;

	/// <summary>
	/// This class is used to effectively disable caching
	/// in place where a ICache can be used.
	/// </summary>
	public sealed class NullCache<TKey, TValue>:
		IConcurrentCache<TKey, TValue>
		where TKey : notnull
		where TValue : class {

		public object SyncRoot { get; } = new object();

		/// <summary>
		/// Does nothing.
		/// </summary>
		/// <remarks>
		/// <paramref name="key"/> still can't be null.
		/// </remarks>
		public void Add(TKey key, TValue value) {
			if (key == null)
				throw new ArgumentNullException(nameof(key));
		}

		/// <summary>
		/// Always assigns default do <paramref name="value"/> and returns false.
		/// </summary>
		/// <remarks>
		/// <paramref name="key"/> still can't be null.
		/// </remarks>
		public bool TryGet(TKey key, [MaybeNullWhen(false)] out TValue value) {
			if (key == null)
				throw new ArgumentNullException(nameof(key));

			value = null!;
			return false;
		}
	}
}
