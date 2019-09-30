namespace Minotaur.Theseus.TestCreation {
	using System;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;

	public sealed class AveragingRuleConsequentCreator {

		public Dataset Dataset { get; }
		private readonly float _threshold;

		public AveragingRuleConsequentCreator(Dataset dataset, float threshold) {
			if (threshold < 0 || threshold > 1)
				throw new ArgumentOutOfRangeException(nameof(threshold));

			Dataset = dataset;
			_threshold = threshold;
		}

		public Array<bool> CreateConsequent(ReadOnlySpan<int> indicesOfInstances) {
			// @Todo: add safety / sanity checks

			var instanceCount = indicesOfInstances.Length;
			var featureCount = Dataset.FeatureCount;

			var trueCount = new int[featureCount];

			for (int i = 0; i < indicesOfInstances.Length; i++) {
				var index = indicesOfInstances[i];
				UpdateTrueCount(trueCount, index);
			}

			return ComputeAverageLabels(instanceCount, featureCount, trueCount);
		}

		private void UpdateTrueCount(int[] trueCount, int instanceIndex) {
			var labels = Dataset.GetInstanceLabels(instanceIndex);
			for (int i = 0; i < labels.Length; i++) {
				if (labels[i])
					trueCount[i] += 1;
			}
		}

		private bool[] ComputeAverageLabels(int instanceCount, int featureCount, int[] trueCount) {
			var averageLabels = new bool[featureCount];

			for (int i = 0; i < averageLabels.Length; i++) {
				var averageTrue = ((float) trueCount[i] / instanceCount);
				averageLabels[i] = averageTrue >= _threshold;
			}

			return averageLabels;
		}
	}
}
