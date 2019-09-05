namespace Minotaur.GeneticAlgorithms.Metrics {
	using System;
	using Minotaur.Collections;

	/// <summary>
	/// This class is meant to keep track of how the labels predicted
	/// by a multi-label classifier match up with the real labels
	/// of a multi-label dataset.
	/// 
	/// About the name of the class: his is not a real confusion matrix,
	/// but since it stores values like truepositives, falsenegatives, etc., 
	/// it kinda makes sense to call it 'confusion matrix'.
	/// </summary>
	public sealed class PseudoConfusionMatrix {
		public readonly float TruePositive;
		public readonly float FalsePositive;
		public readonly float TrueNegative;
		public readonly float FalseNegative;

		private PseudoConfusionMatrix(float truePositive, float falsePositive, float trueNegative, float falseNegative) {
			if (float.IsNaN(truePositive) || float.IsInfinity(truePositive) || truePositive < 0 || truePositive > 1)
				throw new ArgumentOutOfRangeException(nameof(truePositive) + " must be between [0, 1]");

			if (float.IsNaN(falsePositive) || float.IsInfinity(falsePositive) || falsePositive < 0 || falsePositive > 1)
				throw new ArgumentOutOfRangeException(nameof(falsePositive) + " must be between [0, 1]");

			if (float.IsNaN(trueNegative) || float.IsInfinity(trueNegative) || trueNegative < 0 || trueNegative > 1)
				throw new ArgumentOutOfRangeException(nameof(trueNegative) + " must be between [0, 1]");

			if (float.IsNaN(falseNegative) || float.IsInfinity(falseNegative) || falseNegative < 0 || falseNegative > 1)
				throw new ArgumentOutOfRangeException(nameof(falseNegative) + " must be between [0, 1]");

			TruePositive = truePositive;
			FalsePositive = falsePositive;
			TrueNegative = trueNegative;
			FalseNegative = falseNegative;
		}

		public static PseudoConfusionMatrix Create(Matrix<bool> actual, Matrix<bool> predicted) {
			if (actual == null)
				throw new ArgumentNullException(nameof(actual));
			if (predicted == null)
				throw new ArgumentNullException(nameof(predicted));
			if (actual.ColumnCount != predicted.ColumnCount)
				throw new ArgumentException("Both arguments must have the same number of columns.");
			if (actual.RowCount != predicted.RowCount)
				throw new ArgumentException("Both arguments must have the same number of rows.");

			int truePositive = 0;
			int falsePositive = 0;
			int trueNegative = 0;
			int falseNegative = 0;

			var flattenedActual = actual.FlattenedValues;
			var flattenedPredicted = predicted.FlattenedValues;

			for (int i = 0; i < flattenedActual.Length; i++) {
				var actualValue = flattenedActual[i];
				var predictedValue = flattenedPredicted[i];

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

			float count = flattenedActual.Length;
			return new PseudoConfusionMatrix(
				truePositive: truePositive / count,
				falsePositive: falsePositive / count,
				trueNegative: trueNegative / count,
				falseNegative: falseNegative / count);
		}
	}
}
