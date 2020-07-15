namespace Minotaur.Collections {
	using System;

	public static class IndexingHelper {

		public static int[] CreateIndices(int count) {
			if (count <= 0)
				throw new ArgumentOutOfRangeException(nameof(count));

			var indices = new int[count];
			for (int i = 0; i < indices.Length; i++)
				indices[i] = i;

			return indices;
		}
	}
}
