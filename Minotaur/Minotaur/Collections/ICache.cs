namespace Minotaur.Collections {
	using System;

	public interface ICache<TKey, TValue> {
		void Add(TKey key, TValue value);
		bool TryGet(TKey key, out TValue value);
		TValue GetOrCreate(TKey key, Func<TValue> valueCreator);
	}
}
