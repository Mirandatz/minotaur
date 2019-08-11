namespace Minotaur.Collections {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using Newtonsoft.Json;
	using Minotaur.ExtensionMethods.SystemArray;
	using System.Text;

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class ReadOnlyArray<T>: IEnumerable<T> {

		[JsonProperty] public readonly int Length;
		[JsonProperty] private readonly T[] _items;

		private ReadOnlyArray(T[] items) {
			_items = items;
			Length = _items.Length;
		}

		[JsonConstructor]
		private ReadOnlyArray(int length, T[] items) {
			Length = length;
			_items = items;
		}

		public static ReadOnlyArray<T> Wrap(T[] array) {
			if (array == null)
				throw new ArgumentNullException(nameof(array));

			return new ReadOnlyArray<T>(array);
		}

		public bool IsEmpty => Length == 0;

		public T this[int index] => _items[index];

		public ReadOnlySpan<T> Span => new ReadOnlySpan<T>(_items);

		public ReadOnlyMemory<T> Memory => new ReadOnlyMemory<T>(_items);

		//public ReadOnlyArray<T> Swap(int index, T newItem) {
		//	if (index < 0 || index > Length)
		//		throw new ArgumentOutOfRangeException(nameof(index) + " must be >= 0 and < " + nameof(Length));

		//	var copy = new T[_items.Length];
		//	Array.Copy(
		//		sourceArray: _items,
		//		destinationArray: copy,
		//		length: _items.Length);

		//	copy[index] = newItem;
		//	return copy;
		//}

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

		public static implicit operator ReadOnlyArray<T>(T[] mutableArray) => Wrap(mutableArray);

		public static implicit operator ReadOnlySpan<T>(ReadOnlyArray<T> readOnlyArray) => readOnlyArray.Span;
	}

	public static class ReadOnlyArrayExtensions {

		public static bool ContainsNulls<T>(this ReadOnlyArray<T> readOnlyArray) where T : class {
			for (int i = 0; i < readOnlyArray.Length; i++)
				if (readOnlyArray[i] == null)
					return true;

			return false;
		}

		public static bool ContainsNaNs(this ReadOnlyArray<float> readOnlyArray) {
			for (int i = 0; i < readOnlyArray.Length; i++)
				if (float.IsNaN(readOnlyArray[i]))
					return true;

			return false;
		}

		public static bool SequenceEquals<T>(this ReadOnlyArray<T> self, ReadOnlyArray<T> other) where T : IEquatable<T> {
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

		public static ReadOnlyArray<T> Append<T>(this ReadOnlyArray<T> self, T item) {
			var newArray = new T[self.Length + 1];

			for (int i = 0; i < self.Length; i++)
				newArray[i] = self[i];

			newArray[newArray.Length - 1] = item;

			return newArray;
		}

		public static ReadOnlyArray<T> Swap<T>(this ReadOnlyArray<T> self, T item, int index) {
			if (index < 0 || index >= self.Length)
				throw new ArgumentOutOfRangeException(nameof(index) + $" must be in range [0, {self.Length}[");

			// Copy everyone and overwrite should be faster 
			// than conditionally copying
			var newArray = self.ToArray();
			newArray[index] = item;

			return newArray;
		}

		public static ReadOnlyArray<T> Remove<T>(this ReadOnlyArray<T> self, int index) {
			if (self.Length == 0)
				throw new InvalidOperationException($"Can't remove items from an empty {nameof(ReadOnlyArray<T>)}");
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

		public static string ToReadableString(this ReadOnlyArray<float> readOnlyArray) {
			var builder = new StringBuilder();

			for (int i = 0; i < readOnlyArray.Length; i++) {
				builder.Append(readOnlyArray[i]);
				builder.Append(" ");
			}

			return builder.ToString();
		}
	}
}