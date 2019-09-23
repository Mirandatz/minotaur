namespace Minotaur.Collections {
	using System.Diagnostics.CodeAnalysis;

	public interface IConcurrentCache<TKey, TValue>
		where TValue : class {
		object SyncRoot { get; }
		void Add(TKey key, TValue value);
		bool TryGet(TKey key, [MaybeNullWhen(false)] out TValue? value);
	}
}
