namespace Minotaur.Collections {

	public interface IConcurrentCache<TKey, TValue> {
		object SyncRoot { get; }
		void Add(TKey key, TValue value);
		bool TryGet(TKey key, out TValue value);
	}
}
