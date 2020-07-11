namespace Minotaur.Collections {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Text;
	using Minotaur.ExtensionMethods.SystemArray;

	public sealed class Array<T>: IReadOnlyList<T> {

		private readonly T[] _items;
		public int Length => _items.Length;

		// Constructors and alike
		private Array(T[] items) {
			_items = items;
		}

		public static Array<T> Wrap(T[] array) => new Array<T>(array);

		// Views
		public ReadOnlySpan<T> AsSpan() => new ReadOnlySpan<T>(_items);

		// IReadOnlyList
		public bool IsEmpty => Length == 0;

		public int Count => Length;

		public T this[int index] => _items[index];

		public T this[Index index] => _items[index];

		public IEnumerator<T> GetEnumerator() => _items.GetGenericEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

		// Silly overrides
		public override string ToString() => throw new NotImplementedException();

		public override int GetHashCode() => throw new NotImplementedException();

		public override bool Equals(object? obj) => throw new NotImplementedException();

		// Conversion from System.Array to Array, just for convenience
		public static implicit operator Array<T>(T[] mutableArray) => Wrap(mutableArray);

		// Actual methods
		public T[] ToArray() {
			var copy = new T[Length];
			Array.Copy(
				sourceArray: _items,
				destinationArray: copy,
				length: copy.Length);

			return copy;
		}
	}

	public static class ReferenceTypesArrayExtensions {

		public static bool ContainsNulls<T>(this Array<T> self) where T : class {
			for (int i = 0; i < self.Length; i++)
				if (self[i] is null)
					return true;

			return false;
		}
	}
}