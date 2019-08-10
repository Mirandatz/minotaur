namespace Minotaur.Collections {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using Newtonsoft.Json;
	using Minotaur.ExtensionMethods.SystemArray;

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
}