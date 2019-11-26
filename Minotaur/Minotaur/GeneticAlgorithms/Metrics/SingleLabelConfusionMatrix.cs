namespace Minotaur.GeneticAlgorithms.Metrics {
	using System;
	using Minotaur.Classification;
	using Minotaur.Collections;

	public sealed class SingleLabelConfusionMatrix {

		// @Assumption: labels are stored as a natural range; that is, their values e [0, #classes[
		/// <remarks>
		/// Predicted labels are stored in the columns.
		/// Actual labels are stored in the rows.
		/// </remarks>
		public static Matrix<int> Create(Array<ILabel> actualLabels, Array<ILabel> predictedLabels, int classCount) {
			if (actualLabels.Length != predictedLabels.Length)
				throw new InvalidOperationException();

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

			return absoluteConfusionMatrix.ToMatrix();
		}
	}
}
