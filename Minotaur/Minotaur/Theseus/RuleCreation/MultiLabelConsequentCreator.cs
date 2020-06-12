namespace Minotaur.Theseus.RuleCreation {
	using System;
	using Minotaur.Classification;
	using Minotaur.Classification.Rules;
	using Minotaur.Collections.Dataset;

	public sealed class MultiLabelConsequentCreator: IConsequentCreator {

		private readonly Dataset _dataset;
		private readonly float _threshold;

		public MultiLabelConsequentCreator(Dataset dataset, float threshold) {
			if (threshold < 0 || threshold > 1)
				throw new ArgumentOutOfRangeException(nameof(threshold));

			_dataset = dataset;
			_threshold = threshold;
		}

		public Consequent CreateConsequent(ReadOnlySpan<int> indicesOfInstances) {
			// @Todo: add safety / sanity checks

			var classCount = _dataset.ClassCount;
			var trueCount = new int[classCount];
			for (int i = 0; i < indicesOfInstances.Length; i++) {
				var index = indicesOfInstances[i];
				UpdateTrueCount(trueCount, index);
			}

			var instanceCount = indicesOfInstances.Length;
			var averageLabels = ComputeAverageLabels(instanceCount, classCount, trueCount);
			var label = new MultiLabel(averageLabels);

			return new Consequent(label);
		}

		private void UpdateTrueCount(int[] trueCount, int instanceIndex) {
			var labels = (MultiLabel) _dataset.GetInstanceLabel(instanceIndex);
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
