namespace Minotaur.ExtensionMethods.IEnumerable {
	using System.Collections.Generic;

	public static class IEnumerableExtensions {

		public static IEnumerable<(int index, T item)> Enumerate<T>(this IEnumerable<T> iterable) {
			int index = 0;
			foreach (var item in iterable) {
				yield return (index, item);
				index += 1;
			}
		}
	}
}