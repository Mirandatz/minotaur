namespace Minotaur.GeneticAlgorithms.Metrics {
	using System;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;

	/// <remarks>
	/// Predicted labels are stored in the columns.
	/// Actual labels are stored in the rows.
	/// </remarks>
	public sealed class SingleLabelConfusionMatrix {

		public readonly Matrix<int> AbsoluteValues;

		private SingleLabelConfusionMatrix(Matrix<int> absoluteValues) {
			AbsoluteValues = absoluteValues;
		}

		// @Assumption: labels are stored as a natural range; that is, their values e [0, #classes[
		public static SingleLabelConfusionMatrix Create(Array<ILabel> actualLabels, Array<ILabel> predictedLabels, Array<float> classWeights) {
			if (actualLabels.Length != predictedLabels.Length)
				throw new InvalidOperationException();

			var classCount = classWeights.Length;

			var absoluteConfusionMatrix = new MutableMatrix<int>(
				rowCount: classCount,
				columnCount: classCount);

			var instanceCount = actualLabels.Length;

			for (int instanceIndex = 0; instanceIndex < instanceCount; instanceIndex++) {

				var actual = ((SingleLabel) actualLabels[instanceIndex]).Value;
				var predicted = ((SingleLabel) predictedLabels[instanceIndex]).Value;

				var oldConfusionValue = absoluteConfusionMatrix.Get(
					rowIndex: actual,
					columnIndex: predicted);

				absoluteConfusionMatrix.Set(
					rowIndex: actual,
					columnIndex: predicted,
					oldConfusionValue + 1);
			}

			return new SingleLabelConfusionMatrix(absoluteConfusionMatrix.ToMatrix());
		}
	}
}
