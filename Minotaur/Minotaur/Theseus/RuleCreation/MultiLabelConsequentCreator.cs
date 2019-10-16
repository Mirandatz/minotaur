namespace Minotaur.Theseus.RuleCreation {
	using System;
	using Minotaur.Collections.Dataset;

	public sealed class MultiLabelConsequentCreator: IConsequentCreator {

		public Dataset Dataset { get; }
		private readonly float _threshold;

		public MultiLabelConsequentCreator(Dataset dataset, float threshold) {
			if (threshold < 0 || threshold > 1)
				throw new ArgumentOutOfRangeException(nameof(threshold));

			Dataset = dataset;
			_threshold = threshold;
		}

		public ILabel CreateConsequent(ReadOnlySpan<int> indicesOfInstances) {
			// @Todo: add safety / sanity checks

			var classCount = Dataset.ClassCount;
			var trueCount = new int[classCount];
			for (int i = 0; i < indicesOfInstances.Length; i++) {
				var index = indicesOfInstances[i];
				UpdateTrueCount(trueCount, index);
			}

			var instanceCount = indicesOfInstances.Length;
			var averageLabels = ComputeAverageLabels(instanceCount, classCount, trueCount);
			return new MultiLabel(averageLabels);
		}

		private void UpdateTrueCount(int[] trueCount, int instanceIndex) {
			var labels = (MultiLabel) Dataset.GetInstanceLabel(instanceIndex);
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
