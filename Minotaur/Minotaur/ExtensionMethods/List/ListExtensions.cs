namespace Minotaur.ExtensionMethods.List {
	using System;
	using System.Collections.Generic;

	public static class ListExtensions {

		public static void AddMultipleTimes<T>(this List<T> list, T item, int count) {
			if (count < 0)
				throw new ArgumentOutOfRangeException(nameof(count) + " must be >= 0");

			for (int i = 0; i < count; i++)
				list.Add(item);
		}
	}
}
