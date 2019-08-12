namespace Minotaur.Collections {
	using System;
	using System.Collections.Generic;

	/// <remarks>
	/// This class is based on the following inplementation https://stackoverflow.com/a/3719378/1642116
	/// </remarks>
	public sealed partial class LruCache<K, V> {

		private readonly int _capacity;
		private readonly Dictionary<K, LinkedListNode<LRUCacheEntry<K, V>>> _cacheMap = new Dictionary<K, LinkedListNode<LRUCacheEntry<K, V>>>();
		private readonly LinkedList<LRUCacheEntry<K, V>> _lruList = new LinkedList<LRUCacheEntry<K, V>>();

		public LruCache(int capacity) {
			if (capacity <= 0)
				throw new ArgumentOutOfRangeException(nameof(capacity) + " must be > 0");

			this._capacity = capacity;
		}

		public void Add(K key, V val) {
			if (key == null)
				throw new ArgumentNullException(nameof(key));

			if (_cacheMap.Count >= _capacity)
				RemoveLastRecentlyUsed();

			var cacheItem = new LRUCacheEntry<K, V>(key, val);
			var node = new LinkedListNode<LRUCacheEntry<K, V>>(cacheItem);
			_lruList.AddLast(node);
			_cacheMap.Add(key, node);
		}

		public bool TryGet(K key, out V value) {
			if (key == null)
				throw new ArgumentNullException(nameof(key));

			var exists = _cacheMap.TryGetValue(key, out var node);

			if (!exists) {
				value = default;
				return false;
			}

			// Updating the last recently used thingy
			_lruList.Remove(node);
			_lruList.AddLast(node);

			value = node.Value.Value;
			return true;
		}

		private void RemoveLastRecentlyUsed() {
			// Remove from LRUPriority
			var node = _lruList.First;
			_lruList.RemoveFirst();

			// Remove from cache
			_cacheMap.Remove(node.Value.Key);
		}

		private sealed class LRUCacheEntry<KeyType, ValueType> {
			public readonly KeyType Key;
			public readonly ValueType Value;

			public LRUCacheEntry(KeyType k, ValueType v) {
				Key = k;
				Value = v;
			}
		}
	}
}
