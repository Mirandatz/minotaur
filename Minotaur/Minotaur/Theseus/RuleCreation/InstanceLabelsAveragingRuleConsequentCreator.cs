namespace Minotaur.Theseus.RuleCreation {
	using System;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;

	public sealed class InstanceLabelsAveragingRuleConsequentCreator {

		public Dataset Dataset { get; }
		private readonly float _threshold;

		public InstanceLabelsAveragingRuleConsequentCreator(Dataset dataset, float threshold) {
			if (threshold < 0 || threshold > 1)
				throw new ArgumentOutOfRangeException(nameof(threshold));

			Dataset = dataset;
			_threshold = threshold;
		}

		public Array<bool> CreateConsequent(ReadOnlySpan<int> indicesOfInstances) {
			// @Todo: add safety / sanity checks

			var instanceCount = indicesOfInstances.Length;
			var classCount = Dataset.ClassCount;

			var trueCount = new int[classCount];

			for (int i = 0; i < indicesOfInstances.Length; i++) {
				var index = indicesOfInstances[i];
				UpdateTrueCount(trueCount, index);
			}

			return ComputeAverageLabels(instanceCount, classCount, trueCount);
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
