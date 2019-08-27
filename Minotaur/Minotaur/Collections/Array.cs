namespace Minotaur.Collections {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using Newtonsoft.Json;
	using Minotaur.ExtensionMethods.SystemArray;
	using System.Text;
	using Minotaur.Math.Dimensions;

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class Array<T>: IEnumerable<T> {

		[JsonProperty] public readonly int Length;
		[JsonProperty] private readonly T[] _items;

		private Array(T[] items) {
			_items = items;
			Length = _items.Length;
		}

		[JsonConstructor]
		private Array(int length, T[] items) {
			Length = length;
			_items = items;
		}

		public static Array<T> Wrap(T[] array) {
			if (array == null)
				throw new ArgumentNullException(nameof(array));

			return new Array<T>(array);
		}

		public bool IsEmpty => Length == 0;

		public T this[int index] => _items[index];

		public ReadOnlySpan<T> Span => new ReadOnlySpan<T>(_items);

		public ReadOnlyMemory<T> Memory => new ReadOnlyMemory<T>(_items);

		public int BinarySearch(T value) {
			return System.Array.BinarySearch(_items, value);
		}

		public Array<T> Clone() {

			var itemsClone = new T[this.Length];
			Array.Copy(
				sourceArray: this._items,
				destinationArray: itemsClone,
				length: Length);

			return new Array<T>(
				items: itemsClone,
				length: Length);
		}

		public T[] ToArray() {
			var copy = new T[Length];

			Array.Copy(
				sourceArray: _items,
				destinationArray: copy,
				length: copy.Length);

			return copy;
		}

		public IEnumerator<T> GetEnumerator() => _items.GetGenericEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

		public static implicit operator Array<T>(T[] mutableArray) => Wrap(mutableArray);
	}

	public static class ArrayExtensions {

		public static bool ContainsNulls<T>(this Array<T> readOnlyArray) where T : class {
			for (int i = 0; i < readOnlyArray.Length; i++)
				if (readOnlyArray[i] == null)
					return true;

			return false;
		}

		public static bool ContainsNaNs(this Array<float> readOnlyArray) {
			for (int i = 0; i < readOnlyArray.Length; i++)
				if (float.IsNaN(readOnlyArray[i]))
					return true;

			return false;
		}

		public static bool SequenceEquals<T>(this Array<T> self, Array<T> other) where T : IEquatable<T> {
			// We check ReferenceEquals before checking for nulls because we don't expect to
			// compare to nulls (ever)
			if (ReferenceEquals(self, other))
				return true;
			if (other == null)
				return false;

			if (self.Length != other.Length)
				return false;

			for (int i = 0; i < self.Length; i++) {
				var lhs = self[i];
				var rhs = other[i];

				if (lhs == null) {
					if (rhs != null)
						return false;
				} else {
					if (!lhs.Equals(rhs))
						return false;
				}
			}

			return true;
		}

		public static Array<T> Append<T>(this Array<T> self, T item) {
			var newArray = new T[self.Length + 1];

			for (int i = 0; i < self.Length; i++)
				newArray[i] = self[i];

			newArray[newArray.Length - 1] = item;

			return newArray;
		}

		public static Array<T> Swap<T>(this Array<T> self, T item, int index) {
			if (index < 0 || index >= self.Length)
				throw new ArgumentOutOfRangeException(nameof(index) + $" must be in range [0, {self.Length}[");

			// Copy everyone and overwrite should be faster 
			// than conditionally copying
			var newArray = self.ToArray();
			newArray[index] = item;

			return newArray;
		}

		public static Array<T> Remove<T>(this Array<T> self, int index) {
			if (self.Length == 0)
				throw new InvalidOperationException($"Can't remove items from an empty {nameof(Array<T>)}");
			if (index < 0 || index >= self.Length)
				throw new ArgumentOutOfRangeException(nameof(index) + $" must be in range [0, {self.Length}[");

			var newArray = new T[self.Length - 1];

			// Copy "items before" and "items after" should be faster
			// than conditionally copying

			for (int i = 0; i < index; i++)
				newArray[i] = self[i];

			var oldIndex = index + 1;
			var newIndex = index;

			for (;
				oldIndex < self.Length;
				oldIndex++, newIndex++
				) {
				newArray[newIndex] = self[oldIndex];
			}

			return newArray;
		}

		public static string ToReadableString(this Array<float> readOnlyArray) {
			var builder = new StringBuilder();

			for (int i = 0; i < readOnlyArray.Length; i++) {
				builder.Append(readOnlyArray[i]);
				builder.Append(" ");
			}

			return builder.ToString();
		}
	}
}