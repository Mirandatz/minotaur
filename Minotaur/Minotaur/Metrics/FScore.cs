namespace Minotaur.EvolutionaryAlgorithms.Metrics {
	using System;
	using Minotaur.Classification;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.EvolutionaryAlgorithms.Population;

	/// <summary>
	/// Wrote to be compatible with MULAN:
	/// https://github.com/tsoumakas/mulan/blob/f56e8148812dde357178a053cb09877515b538ad/mulan/src/main/java/mulan/evaluation/measure/InformationRetrievalMeasures.java
	/// </summary>
	public sealed class FScore: IMetric {
		private readonly Dataset _dataset;

		public string Name => nameof(FScore);

		public FScore(Dataset dataset) {
			if (dataset.ClassificationType != ClassificationType.MultiLabel)
				throw new InvalidOperationException();

			_dataset = dataset;
		}

		public float EvaluateAsMaximizationTask(Individual individual) {
			var predictions = individual.Predict(_dataset);
			var confusionMatrix = PseudoConfusionMatrix.Create(
				actual: _dataset.InstanceLabels,
				predicted: predictions);

			var tp = confusionMatrix.TruePositive;
			var fp = confusionMatrix.FalsePositive;
			var fn = confusionMatrix.FalseNegative;

			if (tp + fp + fn == 0) {
				return 1;
			}

			var beta = 1;
			var beta2 = beta * beta;
			return ((beta2 + 1) * tp) / ((beta2 + 1) * tp + beta2 * fn + fp);
		}

		/// <summary>
		/// This class is meant to keep track of how the labels predicted
		/// by a multi-label classifier match up with the real labels
		/// of a multi-label dataset.
		/// 
		/// About the name of the class: his is not a real confusion matrix,
		/// but since it stores values like truepositives, falsenegatives, etc., 
		/// it kinda makes sense to call it 'confusion matrix'.
		/// </summary>
		private sealed class PseudoConfusionMatrix {
			public readonly float TruePositive;
			public readonly float FalsePositive;
			public readonly float TrueNegative;
			public readonly float FalseNegative;

			private PseudoConfusionMatrix(float truePositive, float falsePositive, float trueNegative, float falseNegative) {
				if (!(truePositive >= 0 && truePositive <= 1))
					throw new ArgumentOutOfRangeException(nameof(truePositive) + " must be between [0, 1]");

				if (!(falsePositive >= 0 && falsePositive <= 1))
					throw new ArgumentOutOfRangeException(nameof(falsePositive) + " must be between [0, 1]");

				if (!(trueNegative >= 0 && trueNegative <= 1))
					throw new ArgumentOutOfRangeException(nameof(trueNegative) + " must be between [0, 1]");

				if (!(falseNegative >= 0 && falseNegative <= 1))
					throw new ArgumentOutOfRangeException(nameof(falseNegative) + " must be between [0, 1]");

				TruePositive = truePositive;
				FalsePositive = falsePositive;
				TrueNegative = trueNegative;
				FalseNegative = falseNegative;
			}

			public static PseudoConfusionMatrix Create(Array<ILabel> actual, Array<ILabel> predicted) {
				if (actual.Length != predicted.Length)
					throw new ArgumentException();

				int truePositive = 0;
				int falsePositive = 0;
				int trueNegative = 0;
				int falseNegative = 0;

				var instanceCount = actual.Length;
				var featureCount = ((MultiLabel) actual[0]).Length;

				for (int i = 0; i < instanceCount; i++) {
					var actualLabels = (MultiLabel) actual[i];
					var predictedLabels = (MultiLabel) predicted[i];

					if (actualLabels.Length != predictedLabels.Length)
						throw new InvalidOperationException();

					UpdateCounts(
						actualLabels: actualLabels,
						predictedLabels: predictedLabels,
						featureCount: featureCount,
						truePositive: ref truePositive,
						falsePositive: ref falsePositive,
						trueNegative: ref trueNegative,
						falseNegative: ref falseNegative);
				}

				return new PseudoConfusionMatrix(
					truePositive: truePositive / ((float) (instanceCount * featureCount)),
					falsePositive: falsePositive / ((float) (instanceCount * featureCount)),
					trueNegative: trueNegative / ((float) (instanceCount * featureCount)),
					falseNegative: falseNegative / ((float) (instanceCount * featureCount)));
			}

			private static void UpdateCounts(MultiLabel actualLabels, MultiLabel predictedLabels, int featureCount, ref int truePositive, ref int falsePositive, ref int trueNegative, ref int falseNegative) {
				for (int i = 0; i < featureCount; i++) {
					var actualValue = actualLabels[i];
					var predictedValue = predictedLabels[i];

					if (predictedValue) {
						if (actualValue)
							truePositive += 1;
						else
							falsePositive += 1;
					} else {
						if (actualValue)
							falseNegative += 1;
						else
							trueNegative += 1;
					}
				}
			}
		}
	}
}
