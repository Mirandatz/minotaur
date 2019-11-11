namespace Minotaur.Collections {
	using System;

	public static class IndexingHelper {

		public static T[] CopyIndexedItems<T>(Array<int> indices, Array<T> items) {
			var alreadyCopied = new bool[indices.Length];
			var indexedItems = new T[indices.Length];

			for (int i = 0; i < indexedItems.Length; i++) {
				if (alreadyCopied[i])
					throw new InvalidOperationException();

				indexedItems[i] = items[indices[i]];
				alreadyCopied[i] = true;
			}

			return indexedItems;
		}

		public static IndexedItem<T>[] IndexItems<T>(Array<T> items) where T : class {
			var indexedItems = new IndexedItem<T>[items.Length];

			for (int i = 0; i < indexedItems.Length; i++)
				indexedItems[i] = new IndexedItem<T>(i, items[i]);

			return indexedItems;
		}

		public static int[] ExtractIndices<T>(Array<IndexedItem<T>> indexedItems) where T : class {
			var indices = new int[indexedItems.Length];
			for (int i = 0; i < indices.Length; i++)
				indices[i] = indexedItems[i].Index;

			return indices;
		}
	}
}
