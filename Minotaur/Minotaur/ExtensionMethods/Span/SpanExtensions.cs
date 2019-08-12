namespace Minotaur.ExtensionMethods.Span {
	using System;

	public static class SpanExtensions {

		public static void Swap<T>(this Span<T> span, int i, int j) {
			var temp = span[i];
			span[i] = span[j];
			span[j] = temp;
		}

		public static bool ContainsNulls<T>(this ReadOnlySpan<T> span) {
			for (int i = 0; i < span.Length; i++) {
				if (span[i] == null)
					return true;
			}

			return false;
		}

		public static bool ContainsNulls<T>(this Span<T> span) {
			for (int i = 0; i < span.Length; i++) {
				if (span[i] == null)
					return true;
			}

			return false;
		}

		public static T[] RemoveItem<T>(this ReadOnlySpan<T> span, int index) {
			if (index < 0 || index >= span.Length)
				throw new ArgumentOutOfRangeException(nameof(index) + $" must be between 0 and {span.Length}");

			var remainingItems = new T[span.Length - 1];
			var remainingItemsIndex = 0;

			for (int i = 0; i < span.Length; i++) {
				if (i == index)
					continue;

				remainingItems[remainingItemsIndex] = span[i];
				remainingItemsIndex += 1;
			}

			return remainingItems;
		}
	}
}
