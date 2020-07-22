namespace Minotaur.ExtensionMethods.SystemArray {
	using System.Collections.Generic;

	public static class SystemArrayExtensions {

		public static IEnumerator<T> GetGenericEnumerator<T>(this T[] array) {
			return ((IEnumerable<T>) array).GetEnumerator();
		}
	}
}
