namespace Minotaur.Collections {
	using System;

	public sealed class IndexedItem<T> where T : class {
		public readonly int Index;
		public readonly T Item;

		public IndexedItem(int index, T item) {
			if (index < 0)
				throw new ArgumentOutOfRangeException(nameof(index));

			Index = index;
			Item = item;
		}
	}
}
