namespace Minotaur.Collections {
	using System.Diagnostics.CodeAnalysis;

	public interface IConcurrentCache<TKey, TValue>
		where TKey : notnull
		where TValue : notnull {

		object SyncRoot { get; }
		void Add(TKey key, TValue value);
		bool TryGet(TKey key, [MaybeNullWhen(false)] out TValue value);
	}
}
