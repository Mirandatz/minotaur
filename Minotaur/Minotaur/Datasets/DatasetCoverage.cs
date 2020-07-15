namespace Minotaur.Datasets {
	using System;
	using System.Collections.Generic;
	using Minotaur.Collections;

	public sealed class DatasetCoverage {

		public readonly Array<int> IndicesOfCoveredInstances;
		public readonly Array<int> IndicesOfUncoveredInstances;

		public DatasetCoverage(ReadOnlySpan<bool> coverageMap) {
			if (coverageMap.IsEmpty)
				throw new ArgumentException(nameof(coverageMap) + " can't be empty.");

			var covered = new List<int>(coverageMap.Length);
			var uncovered = new List<int>(coverageMap.Length);

			for (int i = 0; i < coverageMap.Length; i++) {
				if (coverageMap[i])
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
