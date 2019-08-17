namespace Minotaur.Collections {
	using System;

	/// <summary>
	/// This class is used to effectively disable caching
	/// in place where a ICache can be used.
	/// </summary>
	public sealed class NullCache<TKey, TValue>: ICache<TKey, TValue> {

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
		public bool TryGet(TKey key, out TValue value) {
			if (key == null)
				throw new ArgumentNullException(nameof(key));

			value = default;
			return false;
		}

		/// <summary>
		/// Invokes <paramref name="valueCreator"/> and returns its output.
		/// </summary>
		/// <remarks>
		/// Neither <paramref name="key"/> nor <paramref name="valueCreator"/> can be
		/// null.
		/// </remarks>
		public TValue GetOrCreate(TKey key, Func<TValue> valueCreator) {
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			if (valueCreator is null)
				throw new ArgumentNullException(nameof(valueCreator));

			return valueCreator();
		}
	}
}
