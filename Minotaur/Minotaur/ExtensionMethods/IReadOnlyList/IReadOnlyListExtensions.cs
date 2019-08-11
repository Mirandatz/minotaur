namespace Minotaur.ExtensionMethods.IReadOnlyList {
	using System.Collections.Generic;

	public static class IReadOnlyListExtensions {

		public static bool ContainsNulls<T>(this IReadOnlyList<T> list) {
			for (int i = 0; i < list.Count; i++) {
				if (list[i] == null)
					return true;
			}

			return false;
		}
	}
}
