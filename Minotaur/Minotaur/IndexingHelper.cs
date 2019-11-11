namespace Minotaur {
	using Minotaur.Collections;

	public static class IndexingHelper {

		public static T[] CopyIndexedItems<T>(Array<int> indices, Array<T> items) {
			var indexedItems = new T[indices.Length];

			for (int i = 0; i < indexedItems.Length; i++)
				indexedItems[i] = items[indices[i]];

			return indexedItems;
		}
	}
}
