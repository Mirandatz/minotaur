namespace Minotaur.Collections {
	using System;
	using System.Collections;
	using System.Collections.Generic;

	public sealed class Set<T>: IReadOnlyCollection<T> {

		private readonly HashSet<T> _items;
		public int Count => _items.Count;

		private Set(HashSet<T> items) {
			_items = items;
		}

		public static Set<T> Wrap(HashSet<T> set) => new Set<T>(set);

		public bool Contains(T item) => _items.Contains(item);

		public bool SetEquals(Set<T> rulesSet) => _items.SetEquals(rulesSet._items);

		public override string ToString() => throw new NotImplementedException();

		public override bool Equals(object? obj) => throw new NotImplementedException();

		public override int GetHashCode() => throw new NotImplementedException();

		// Implementation of IEnumerable<T>
		public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
	}
}
