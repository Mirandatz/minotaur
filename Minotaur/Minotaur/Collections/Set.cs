namespace Minotaur.Collections {
	using System;
	using System.Collections;
	using System.Collections.Generic;

	public sealed class Set<T>: IReadOnlyCollection<T> {

		private readonly HashSet<T> _items;

		public Set(T[] items) {

			_items = new HashSet<T>(capacity: items.Length);
			for (int i = 0; i < items.Length; i++) {
				var added = _items.Add(items[i]);
				if (!added)
					throw new ArgumentException(nameof(_items) + " can't contain duplicates.");
			}
		}

		public int Count => _items.Count;

		public bool Contains(T item) => _items.Contains(item);

		// Silly Object methods...
		public override string ToString() => throw new NotImplementedException();

		public override bool Equals(object? obj) => throw new NotImplementedException();

		public override int GetHashCode() => throw new NotImplementedException();

		// Implementation of IEnumerable<T>
		public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
	}
}
