namespace Minotaur.Collections {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Threading;

	/// <remarks>
	/// This class is based on the following inplementation https://stackoverflow.com/a/3719378/1642116
	/// </remarks>
	public sealed class LruCache<TKey, TValue> {
		private readonly int _capacity;
		private readonly Dictionary<TKey, LinkedListNode<LruCacheEntry>> _cacheMap;

		private readonly LinkedList<LruCacheEntry> _lruList = new LinkedList<LruCacheEntry>();
		public readonly object SyncRoot = new object();

		/// <remarks>
		/// Providing the value 0 to <paramref name="capacity"/>
		/// effectively disables the cache.
		/// </remarks>
		public LruCache(int capacity) {
			if (capacity < 0)
				throw new ArgumentOutOfRangeException(nameof(capacity) + " must be >= 0");

			this._capacity = capacity;
			this._cacheMap = new Dictionary<TKey, LinkedListNode<LruCacheEntry>>(capacity: capacity);
		}

		/// <remarks>
		/// This method is thread-safe.
		/// Null keys are not accepted.
		/// </remarks>
		public void Add(TKey key, TValue value) {
			if (key == null)
				throw new ArgumentNullException(nameof(key));

			if (_capacity == 0)
				return;

			// We are doing as much work as we can outside of the lock
			var cacheItem = new LruCacheEntry(key, value);
			var lruNode = new LinkedListNode<LruCacheEntry>(cacheItem);

			lock (SyncRoot) {
				if (_cacheMap.ContainsKey(key)) {
					UpdateUsage(lruNode);
				} else {
					_cacheMap.Add(key: key, value: lruNode);
					RemoveLeastRecentlyUsedIfNecessary();
				}
			}
		}

		/// <remarks>
		/// This method is thread-safe.
		/// </remarks>
		public bool TryGet(TKey key, out TValue value) {
			if (key == null)
				throw new ArgumentNullException(nameof(key));

			lock (SyncRoot) {
				var isCached = _cacheMap.TryGetValue(key, out var node);

				if (isCached) {
					UpdateUsage(node);
					// Maybe I should move the two lines below to outside the lock?
					value = node.Value.Value;
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

			public LruCacheEntry(TKey k, TValue v) {
				Key = k;
				Value = v;
			}
		}
	}
}
