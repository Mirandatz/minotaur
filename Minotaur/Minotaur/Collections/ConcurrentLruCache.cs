namespace Minotaur.Collections {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Threading;

	/// <summary>
	/// This class is in-memory, thread-safe cache.
	/// The evicition policy is the Least Recently Used (LRU),
	/// see https://en.wikipedia.org/wiki/Cache_replacement_policies#Least_recently_used_(LRU).
	/// </summary>
	/// <remarks>
	/// This class is based on the following implementation https://stackoverflow.com/a/3719378/1642116
	/// 
	/// </remarks>
	public sealed class ConcurrentLruCache<TKey, TValue>: IConcurrentCache<TKey, TValue> {
		private readonly int _capacity;
		private readonly Dictionary<TKey, LinkedListNode<LruCacheEntry>> _cacheMap;
		private readonly LinkedList<LruCacheEntry> _lruList = new LinkedList<LruCacheEntry>();
		public object SyncRoot { get; } = new object();

		/// <summary>
		/// The constructor of the class.
		/// <paramref name="capacity"/> must be >= 0.
		/// If you want to disable the caching of the system,
		/// consindering using the NullCache class.
		/// </summary>
		public ConcurrentLruCache(int capacity) {
			if (capacity <= 0)
				throw new ArgumentOutOfRangeException(nameof(capacity) + " must be > 0");

			this._capacity = capacity;
			this._cacheMap = new Dictionary<TKey, LinkedListNode<LruCacheEntry>>(capacity: capacity);
		}

		/// <summary>
		/// Adds the pair (<paramref name="key"/>, <paramref name="value"/>)
		/// to the cache.
		/// If the key is already in the cache, an exception is thrown.
		/// <remarks>
		/// This operation is atomic.
		/// </remarks>
		public void Add(TKey key, TValue value) {
			if (key == null)
				throw new ArgumentNullException(nameof(key));

			// We are doing as much work as we can outside of the lock
			var cacheItem = new LruCacheEntry(key, value);
			var newLruNode = new LinkedListNode<LruCacheEntry>(cacheItem);

			lock (SyncRoot) {
				var isCached = _cacheMap.TryGetValue(
					key: key,
					value: out var oldLruNode);

				if (isCached) {
					// Update key-value of cache
					_cacheMap[key] = newLruNode;

					// Update usage; not only the position in the lruList 
					// but also the contents of the node, since the value
					// in the cacheItem may have changed					
					_lruList.Remove(oldLruNode);
					_lruList.AddLast(newLruNode);
				} else {
					_cacheMap.Add(key: key, value: newLruNode);
					RemoveLeastRecentlyUsedIfNecessary();
					_lruList.AddLast(newLruNode);
				}
			}
		}

		/// <summary>
		/// Tries to get the value associated with <paramref name="key"/>.
		/// 
		/// If the key is stored in the cache,
		/// <paramref name="value"/> is set to the value associated with it,
		/// the key's "least recently used" data is updated,
		/// and the method returns true.
		/// 
		/// If the key is not stored in the cache,
		/// <paramref name="value"/> is set to default
		/// and the method returns false.
		/// </summary>
		/// <remarks>
		/// This operation is atomic.
		/// </remarks>
		public bool TryGet(TKey key, out TValue value) {
			if (key == null)
				throw new ArgumentNullException(nameof(key));

			lock (SyncRoot) {
				var isCached = _cacheMap.TryGetValue(key, out var lruNode);

				if (isCached) {
					UpdateUsage(lruNode);
					value = lruNode.Value.Value;
					return true;
				} else {
					value = default;
					return false;
				}
			}
		}

		private void UpdateUsage(LinkedListNode<LruCacheEntry> node) {
			Debug.Assert(Monitor.IsEntered(SyncRoot));

			_lruList.Remove(node);
			_lruList.AddLast(node);
		}

		private void RemoveLeastRecentlyUsedIfNecessary() {
			Debug.Assert(Monitor.IsEntered(SyncRoot));

			if (_cacheMap.Count < _capacity)
				return;

			var node = _lruList.First;
			_lruList.RemoveFirst();
			_cacheMap.Remove(node.Value.Key);
		}

		private sealed class LruCacheEntry {
			public readonly TKey Key;
			public readonly TValue Value;

			public LruCacheEntry(TKey key, TValue valeu) {
				Key = key;
				Value = valeu;
			}
		}
	}
}
