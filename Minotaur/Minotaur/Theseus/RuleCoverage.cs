namespace Minotaur.Theseus {
	using System;
	using System.Collections.Generic;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;

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
	}
}

