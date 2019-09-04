namespace Minotaur.Theseus {
	using System;
	using System.Collections.Generic;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.ExtensionMethods.SystemArray;

	public sealed class RuleCoverage {
		public readonly Dataset Dataset;
		public readonly Array<bool> InstancesCovered;
		public readonly Array<int> IndicesOfCoveredInstances;
		public readonly Array<int> IndicesOfUncoveredInstances;

		public float CoverageRatio => ((float) IndicesOfCoveredInstances.Length) / InstancesCovered.Length;

		public RuleCoverage(Dataset dataset, Array<bool> instancesCovered) {
			Dataset = dataset ?? throw new ArgumentNullException(nameof(dataset));
			InstancesCovered = instancesCovered ?? throw new ArgumentNullException(nameof(instancesCovered));

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

		public static RuleCoverage CombineCoveragesBinaryOr(Array<RuleCoverage> coverages) {
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
			
			return new RuleCoverage(
				dataset: coverages[0].Dataset,
				instancesCovered: finalCoverage);

		}

		private static void BinaryOr(bool[] finalCoverage, RuleCoverage ruleCoverage) {
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

