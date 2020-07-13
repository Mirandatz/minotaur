namespace Minotaur.Datasets {
	using System;
	using System.Collections.Generic;
	using Minotaur.Collections;

	public sealed class DatasetCoverage {

		public readonly Array<int> IndicesOfCoveredInstances;
		public readonly Array<int> IndicesOfUncoveredInstances;

		public DatasetCoverage(ReadOnlySpan<bool> instaceIsCoveredMap) {
			if (instaceIsCoveredMap.IsEmpty)
				throw new ArgumentException(nameof(instaceIsCoveredMap) + " can't be empty.");

			var covered = new List<int>(instaceIsCoveredMap.Length);
			var uncovered = new List<int>(instaceIsCoveredMap.Length);

			for (int i = 0; i < instaceIsCoveredMap.Length; i++) {
				if (instaceIsCoveredMap[i])
					covered.Add(i);
				else
					uncovered.Add(i);
			}

			IndicesOfCoveredInstances = Array<int>.Wrap(covered.ToArray());
			IndicesOfUncoveredInstances = Array<int>.Wrap(uncovered.ToArray());
		}

		// Silly overrides
		public override string ToString() => throw new NotImplementedException();

		public override int GetHashCode() => throw new NotImplementedException();

		public override bool Equals(object? obj) => throw new NotImplementedException();
	}
}
