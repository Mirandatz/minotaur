namespace Minotaur.Collections {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Text;
	using Minotaur.ExtensionMethods.SystemArray;

	public sealed class Array<T>: IEnumerable<T> {

		public readonly int Length;
		private readonly T[] _items;

		private Array(T[] items) {
			_items = items;
			Length = _items.Length;
		}

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

		public T this[Index index] => _items[index];

		// Basic overrides
		public override string ToString() => $"[{string.Join(", ", _items)}]";

		// Implementation of IEnumerable<T>

		public IEnumerator<T> GetEnumerator() => _items.GetGenericEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

		// Conversion from System.Array to Array, again just for convenience

		public static implicit operator Array<T>(T[] mutableArray) => Wrap(mutableArray);

		// Below  here there are just some convenience methods

		public ReadOnlySpan<T> AsSpan() => new ReadOnlySpan<T>(_items);

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

		public Array<T> Append(T item) {
			var newArray = new T[_items.Length + 1];

			for (int i = 0; i < _items.Length; i++)
				newArray[i] = _items[i];

			newArray[^1] = item;

			return newArray;
		}

		public Array<T> Swap(int index, T newItem) {
			if (index < 0 || index >= _items.Length) {
				throw new ArgumentOutOfRangeException(
					nameof(index) + $" must be in range " +
					$"[0, {nameof(Array)}.{nameof(Array.Length)}[.");
			}

			// Copy everyone and overwrite should be faster 
			// than conditionally copying
			var newArray = Clone();
			newArray._items[index] = newItem;

			return newArray;
		}

		public Array<T> Remove(int index) {
			if (IsEmpty) {
				throw new InvalidOperationException($"Can't remove items from an empty {nameof(Array<T>)}.");
			}

			if (index < 0 || index >= _items.Length) {
				throw new ArgumentOutOfRangeException(
					nameof(index) + $" must be in range " +
					$"[0, {nameof(Array)}.{nameof(Array.Length)}[.");
			}

			var newArray = new T[_items.Length - 1];

			// Copy "items before" and "items after" should be faster
			// than conditionally copying

			for (int i = 0; i < index; i++)
				newArray[i] = _items[i];

			var oldIndex = index + 1;
			var newIndex = index;

			for (;
				oldIndex < _items.Length;
				oldIndex++, newIndex++
				) {
				newArray[newIndex] = _items[oldIndex];
			}

			return newArray;
		}
	}

	public static class IComparableExtensions {

		// <returns>
		// The index of the specified value in the specified array, if value is found; otherwise,
		// a negative number. If value is not found and value is less than one or more elements
		// in array, the negative number returned is the bitwise complement of the index
		// of the first element that is larger than value. If value is not found and value
		// is greater than all elements in array, the negative number returned is the bitwise
		// complement of (the index of the last element plus 1). If this method is called
		// with a non-sorted array, the return value can be incorrect and a negative number
		// could be returned, even if value is present in array.
		/// </returns>
		public static int BinarySearch<T>(this Array<T> self, T value) where T : IComparable<T> {
			return self.AsSpan().BinarySearch(value);
		}
	}

	public static class FloatArrayExtensions {

		public static bool ContainsNaNs(this Array<float> readOnlyArray) {
			for (int i = 0; i < readOnlyArray.Length; i++)
				if (float.IsNaN(readOnlyArray[i]))
					return true;

			return false;
		}

		public static string ToReadableString(this Array<float> readOnlyArray) {
			var builder = new StringBuilder();

			for (int i = 0; i < readOnlyArray.Length; i++) {
				builder.Append(readOnlyArray[i]);
				builder.Append(" ");
			}

			return builder.ToString();
		}

		// <returns>
		// The index of the first occurence of the specified value in the specified array,
		// if value is found; otherwise,
		// a negative number. If value is not found and value is less than one or more elements
		// in array, the negative number returned is the bitwise complement of the index
		// of the first element that is larger than value. If value is not found and value
		// is greater than all elements in array, the negative number returned is the bitwise
		// complement of (the index of the last element plus 1). If this method is called
		// with a non-sorted array, the return value can be incorrect and a negative number
		// could be returned, even if value is present in array.
		/// </returns>
		public static int BinarySearchFirstOccurence(this Array<float> self, float value) {
			var span = self.AsSpan();
			var binarySearchIndex = span.BinarySearch(value);

			if (binarySearchIndex <= 0)
				return binarySearchIndex;

			var lastIndex = binarySearchIndex;
			for (int linearProbeIndex = binarySearchIndex - 1;
				linearProbeIndex >= 0;
				linearProbeIndex--
				) {
				if (span[linearProbeIndex] == value)
					lastIndex = linearProbeIndex;
				else
					break;
			}

			return lastIndex;
		}

		// <returns>
		// The index of the last occurence of the specified value in the specified array,
		// if value is found; otherwise,
		// a negative number. If value is not found and value is less than one or more elements
		// in array, the negative number returned is the bitwise complement of the index
		// of the first element that is larger than value. If value is not found and value
		// is greater than all elements in array, the negative number returned is the bitwise
		// complement of (the index of the last element plus 1). If this method is called
		// with a non-sorted array, the return value can be incorrect and a negative number
		// could be returned, even if value is present in array.
		/// </returns>
		public static int BinarySearchLastOccurence(this Array<float> self, float value) {
			var span = self.AsSpan();
			var binarySearchIndex = span.BinarySearch(value);

			if (binarySearchIndex < 0)
				return binarySearchIndex;

			var lastIndex = binarySearchIndex;

			for (int linearProbeIndex = binarySearchIndex + 1;
				linearProbeIndex < span.Length;
				linearProbeIndex++
				) {
				if (span[linearProbeIndex] == value)
					lastIndex = linearProbeIndex;
				else
					break;
			}

			return lastIndex;
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

	public static class IEquatableArrayExtensions {

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
	}
}