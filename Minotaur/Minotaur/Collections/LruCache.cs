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
	/// This class is based on the following inplementation https://stackoverflow.com/a/3719378/1642116
	/// </remarks>
	public sealed class LruCache<TKey, TValue> {
		private readonly int _capacity;
		private readonly Dictionary<TKey, LinkedListNode<LruCacheEntry>> _cacheMap;
		private readonly LinkedList<LruCacheEntry> _lruList = new LinkedList<LruCacheEntry>();
		public readonly object SyncRoot = new object();
				
		/// <summary>
		/// The constructor of the class.
		/// <paramref name="capacity"/> must be >= 0.
		/// If you want to disable the caching of the system,
		/// consindering using the NullCache class.
		/// </summary>
		public LruCache(int capacity) {
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
		/// This method is thread-safe.
		/// </remarks>
		public void Add(TKey key, TValue value) {
			if (key == null)
				throw new ArgumentNullException(nameof(key));

			// We are doing as much work as we can outside of the lock
			var cacheItem = new LruCacheEntry(key, value);
			var lruNode = new LinkedListNode<LruCacheEntry>(cacheItem);

			lock (SyncRoot) {
				if (!_cacheMap.ContainsKey(key)) {
					_cacheMap.Add(key: key, value: lruNode);
					RemoveLeastRecentlyUsedIfNecessary();
				} else {
					throw new InvalidOperationException("The value is already in the cache.");
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
		/// This method is thread-safe.
		/// </remarks>
		public bool TryGet(TKey key, out TValue value) {
			if (key == null)
				throw new ArgumentNullException(nameof(key));

			lock (SyncRoot) {
				var isCached = _cacheMap.TryGetValue(key, out var lruNode);

				if (!isCached) {
					value = default;
					return false;
				} else {
					UpdateUsage(lruNode);
					value = lruNode.Value.Value;
					return true;
				}
			}
		}

		/// <summary>
		/// Tries to get the value associated with <paramref name="key"/>.
		/// If the value is not stored in the cache,
		/// the function <paramref name="valueCreator"/> is called 
		/// and the result is stored in the cache.
		/// If a <paramref name="valueCreatorSyncRoot"/> is provided,
		/// the invocation of <typeparamref name="TValue"/> is wrapped
		/// in a lock statement.
		/// </summary>
		/// <remarks>
		/// This method is thread-safe.
		/// </remarks>
		public TValue GetOrCreate(
			TKey key,
			Func<TValue> valueCreator,
			object valueCreatorSyncRoot
			) {
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			if (valueCreator is null)
				throw new ArgumentNullException(nameof(valueCreator));

			// We take the lock for a long time,
			// but we must ensure that the entire operation is atomic.
			// Maybe using System.Threading.ReaderWriterLockSlim and some
			// black magic we could reduce contention, but aint that 
			// high level of a wizard

			lock (SyncRoot) {
				var isCached = _cacheMap.TryGetValue(key: key, value: out var lruNode);
				if (isCached) {
					UpdateUsage(lruNode);
					return lruNode.Value.Value;
				}

				var value = CreateValue(
					valueCreator: valueCreator,
					valueCreatorSyncRoot: valueCreatorSyncRoot);

				var cacheEntry = new LruCacheEntry(
					key: key,
					valeu: value);

				lruNode = new LinkedListNode<LruCacheEntry>(cacheEntry);

				_cacheMap.Add(key: key, value: lruNode);
				RemoveLeastRecentlyUsedIfNecessary();

				return value;
			}
		}

		private TValue CreateValue(Func<TValue> valueCreator, object valueCreatorSyncRoot) {
			if (valueCreatorSyncRoot is null) {
				return valueCreator();
			} else {
				lock (valueCreatorSyncRoot) {
					return valueCreator();
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
