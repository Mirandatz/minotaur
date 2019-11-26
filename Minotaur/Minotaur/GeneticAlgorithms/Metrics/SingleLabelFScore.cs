namespace Minotaur.GeneticAlgorithms.Metrics {
	using System;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;

	public sealed class SingleLabelFScore: IMetric {
		public string Name => nameof(SingleLabelFScore);

		private readonly Dataset _dataset;

		public SingleLabelFScore(Dataset dataset) {
			_dataset = dataset;
		}

		public float EvaluateAsMaximizationTask(Individual individual) {
			var predicted = individual.Predict(_dataset);
			var actual = _dataset.InstanceLabels;

			var confusionMatrix = SingleLabelConfusionMatrix.Create(
				actualLabels: actual,
				predictedLabels: predicted,
				classCount: _dataset.ClassCount);

			var tps = ComputeTruePositives(confusionMatrix);
			var fps = ComputeFalsePositives(confusionMatrix);
			var fns = ComputeFalseNegatives(confusionMatrix);
			var fscores = ComputeFScores(tps, fps, fns);
			var absoluteClassFrequencies = _dataset.ClassFrequencies;

			var instanceCount = _dataset.InstanceCount;
			var relativeClassFrequencies = new float[absoluteClassFrequencies.Length];
			for (int i = 0; i < relativeClassFrequencies.Length; i++)
				relativeClassFrequencies[i] = ((float) absoluteClassFrequencies[i]) / instanceCount;

			return WeightedMean(
				values: fscores,
				weights: relativeClassFrequencies);
		}

		private int[] ComputeTruePositives(Matrix<int> confusionMatrix) {
			var classCount = confusionMatrix.RowCount;

			var tps = new int[classCount];
			for (int i = 0; i < tps.Length; i++)
				tps[i] = confusionMatrix.Get(i, i);

			return tps;
		}

		private int[] ComputeFalsePositives(Matrix<int> confusionMatrix) {
			var classCount = confusionMatrix.RowCount;
			var fps = new int[classCount];

			for (int i = 0; i < classCount; i++) {
				fps[i] = ComputeFalsePositive(
					classIndex: i,
					confusionMatrix: confusionMatrix);
			}

			return fps;

			static int ComputeFalsePositive(int classIndex, Matrix<int> confusionMatrix) {
				var classPredictions = 0;
				var classCount = confusionMatrix.RowCount;
				for (int i = 0; i < classCount; i++) {
					classPredictions += confusionMatrix.Get(
						rowIndex: i,
						columnIndex: classIndex);
				}

				var tp = confusionMatrix.Get(classIndex, classIndex);
				return classPredictions - tp;
			}
		}

		private int[] ComputeFalseNegatives(Matrix<int> confusionMatrix) {
			var classCount = confusionMatrix.RowCount;

			var fns = new int[classCount];
			for (int i = 0; i < classCount; i++) {
				fns[i] = ComputeFalseNegative(
					classIndex: i,
					confusionMatrix: confusionMatrix);
			}

			return fns;

			static int ComputeFalseNegative(int classIndex, Matrix<int> confusionMatrix) {
				var classPredictions = 0;
				var classCount = confusionMatrix.RowCount;

				for (int i = 0; i < classCount; i++) {
					classPredictions += confusionMatrix.Get(
						rowIndex: classIndex,
						columnIndex: i);
				}

				var tp = confusionMatrix.Get(classIndex, classIndex);
				return classPredictions - tp;
			}
		}

		private float[] ComputeFScores(int[] truePositives, int[] falsePositives, int[] falseNegatives) {
			var fscores = new float[truePositives.Length];

			for (int i = 0; i < fscores.Length; i++) {
				fscores[i] = ComputeFScore(
					truePositives: truePositives[i],
					falsePositives: falsePositives[i],
					falseNegatives: falseNegatives[i]);
			}

			return fscores;

			static float ComputeFScore(float truePositives, float falsePositives, float falseNegatives) {
				if (truePositives + falsePositives + falseNegatives == 0)
					return 1;

				var beta = 1;
				var beta2 = beta * beta;
				return ((beta2 + 1) * truePositives) / ((beta2 + 1) * truePositives + beta2 * falseNegatives + falsePositives);
			}
		}

		private float WeightedMean(float[] values, Array<float> weights) {
			// @Sanity check
			if (values.Length != weights.Length)
				throw new InvalidOperationException();

			double sum = 0;
			for (int i = 0; i < values.Length; i++)
				sum += values[i] * weights[i];

			return (float) sum;
		}
	}
}
