namespace Minotaur.Theseus {
	using System;
	using System.Collections.Generic;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.ExtensionMethods.SystemArray;

	public sealed class DatasetCoverage {
		public readonly Dataset Dataset;
		public readonly Array<bool> InstancesCovered;
		public readonly Array<int> IndicesOfCoveredInstances;
		public readonly Array<int> IndicesOfUncoveredInstances;

		public float CoverageRatio => ((float) IndicesOfCoveredInstances.Length) / InstancesCovered.Length;

		public DatasetCoverage(Dataset dataset, Array<bool> instancesCovered) {
			Dataset = dataset;
			InstancesCovered = instancesCovered;

			// @Improve performance
			var covered = new List<int>(capacity: instancesCovered.Length);
			var uncovered = new List<int>(capacity: instancesCovered.Length);

			for (int i = 0; i < instancesCovered.Length; i++) {
				if (instancesCovered[i])
					covered.Add(i);
				else
					uncovered.Add(i);
			}

			IndicesOfCoveredInstances = covered.ToArray();
			IndicesOfUncoveredInstances = uncovered.ToArray();
		}

		public static DatasetCoverage CombineCoveragesBinaryOr(Array<DatasetCoverage> coverages) {
			if (coverages == null)
				throw new ArgumentNullException(nameof(coverages));
			if (coverages.ContainsNulls())
				throw new ArgumentException(nameof(coverages) + " can't contain nulls.");
			if (coverages.Length == 0)
				throw new ArgumentException(nameof(coverages) + " can't be empty.");

			// @Improve checks (e.g. all coverages have the same dataset, etc)

			var datasetInstaceCount = coverages[0]
				.InstancesCovered
				.Length;

			var finalCoverage = new bool[datasetInstaceCount];

			for (int i = 0; i < coverages.Length; i++)
				BinaryOr(finalCoverage, coverages[i]);

			return new DatasetCoverage(
				dataset: coverages[0].Dataset,
				instancesCovered: finalCoverage);

		}

		private static void BinaryOr(bool[] finalCoverage, DatasetCoverage ruleCoverage) {
			var lhs = finalCoverage;
			var rhs = ruleCoverage.InstancesCovered;

			// Improve exception description
			if (lhs.Length != rhs.Length)
				throw new ArgumentException(nameof(ruleCoverage));

			for (int i = 0; i < lhs.Length; i++)
				lhs[i] = lhs[i] || rhs[i];
		}
	}
}

