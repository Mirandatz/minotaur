namespace Minotaur.Collections.Dataset {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using Minotaur.Math;
	using Minotaur.Math.Dimensions;

	public sealed class Dataset {
		public readonly int InstanceCount;
		public readonly int FeatureCount;
		public readonly int ClassCount;
		public readonly ClassificationType ClassificationType;

		public readonly Array<ILabel> InstanceLabels;
		public readonly Array<FeatureType> FeatureTypes;
		public readonly Matrix<float> Data;

		// These are stored for performance reasons
		private readonly Matrix<double> _distanceMatrix;
		public readonly Array<int> ClassFrequencies;
		public readonly ILabel DefaultLabel;
		private readonly float[][] _sortedUniqueFeatureValues;

		public Dataset(int instanceCount, int featureCount, int classCount, ClassificationType classificationType, Array<ILabel> instanceLabels, Array<FeatureType> featureTypes, Matrix<float> data, Matrix<double> distanceMatrix, Array<int> classFrequencies, ILabel defaultLabel, float[][] sortedUniqueFeatureValues) {
			InstanceCount = instanceCount;
			FeatureCount = featureCount;
			ClassCount = classCount;
			ClassificationType = classificationType;
			InstanceLabels = instanceLabels;
			FeatureTypes = featureTypes;
			Data = data;
			_distanceMatrix = distanceMatrix;
			ClassFrequencies = classFrequencies;
			DefaultLabel = defaultLabel;
			_sortedUniqueFeatureValues = sortedUniqueFeatureValues;
		}

		public static Dataset CreateFromMutableObjects(
			FeatureType[] mutableFeatureTypes,
			MutableMatrix<float> mutableData,
			Array<ILabel> labels,
			bool isTrainDataset,
			ClassificationType classificationType
			) {
			if (mutableFeatureTypes.Length != mutableData.ColumnCount)
				throw new ArgumentException("featureTypes.Length must be equal to  data.ColumnCount");
			if (mutableData.RowCount != labels.Length)
				throw new ArgumentException("label.RowCount must be equal to data.RowCount");

			var featureTypes = mutableFeatureTypes.ToArray();
			var data = mutableData.ToMatrix();
			var dataTransposed = mutableData.Transpose().ToMatrix();

			var featuresCount = featureTypes.Length;

			var sortedFeatureValues = new float[featuresCount][];
			var sortedUniqueFeatureValues = new float[featuresCount][];
			var featureValueFrequencies = new Dictionary<float, int>[featuresCount];
			var dimensionIntervals = new IInterval[featuresCount];

			var distanceMatrixTask = Task.Run(() => Distance.ComputeEuclideanDistanceMatrix(data));

			Parallel.For(fromInclusive: 0, toExclusive: featuresCount, body: featureIndex => {

				var currentFeatureValues = dataTransposed.GetRow(featureIndex).ToArray();

				ThrowIfDatasetContainsNonFiniteValues(currentFeatureValues);

				var sufv = currentFeatureValues
				.Distinct()
				.OrderBy(v => v)
				.ToArray();

				sortedUniqueFeatureValues[featureIndex] = sufv;

				ThrowIfTrainDatasetContainsFeaturesWithSingleValue(isTrainDataset, featureIndex, sufv);

				sortedFeatureValues[featureIndex] = currentFeatureValues
				.OrderBy(v => v)
				.ToArray();

				var counts = currentFeatureValues
				.GroupBy(v => v)
				.ToDictionary(
					keySelector: g => g.Key,
					elementSelector: g => g.Count());

				featureValueFrequencies[featureIndex] = counts;
			});

			int classCount = classificationType switch
			{
				ClassificationType.SingleLabel => labels.Distinct().Count(),
				ClassificationType.MultiLabel => ((MultiLabel) labels[0]).Values.Length,
				_ => throw CommonExceptions.UnknownClassificationType,
			};

			var classFrequencies = classificationType switch
			{
				ClassificationType.SingleLabel => ComputeSingleLabelClassFrequencies(labels, classCount),
				ClassificationType.MultiLabel => ComputeMultiLabelClassFrequencies(labels, classCount),
				_ => throw CommonExceptions.UnknownClassificationType,
			};

			var defaultLabel = ComputeDefaultLabel(labels, classificationType);

			Task.WaitAll(distanceMatrixTask);
			var distanceMatrix = distanceMatrixTask.Result;

			return new Dataset(
				instanceCount: data.RowCount,
				featureCount: data.ColumnCount,
				classCount: classCount,
				classificationType: classificationType,
				instanceLabels: labels,
				featureTypes: featureTypes,
				data: data,
				distanceMatrix: distanceMatrix,
				classFrequencies: classFrequencies,
				defaultLabel: defaultLabel,
				sortedUniqueFeatureValues: sortedUniqueFeatureValues);

			static void ThrowIfDatasetContainsNonFiniteValues(float[] featureValues) {
				if (featureValues.Any(v => !float.IsFinite(v)))
					throw new InvalidOperationException(nameof(Dataset) + " only supports finite values.");
			}

			static void ThrowIfTrainDatasetContainsFeaturesWithSingleValue(bool isTrainDataset, int featureIndex, float[] sufv) {
				if (isTrainDataset && sufv.Length == 1) {
					var message = $"Feature (0-indexed) {featureIndex} contains a single value in the train dataset.";
					throw new InvalidOperationException(message);
				}
			}

			static ILabel ComputeDefaultLabel(Array<ILabel> labels, ClassificationType classificationType) {
				return classificationType switch
				{
					ClassificationType.SingleLabel => ComputeMajorityClass(labels),
					ClassificationType.MultiLabel => ComputeAverageLabels(labels),
					_ => throw CommonExceptions.UnknownClassificationType,
				};

				static ILabel ComputeMajorityClass(Array<ILabel> labels) {
					var mostCommon = labels
						.GroupBy(l => ((SingleLabel) l).Value)
						.OrderBy(g => g.Count())
						.Last()
						.Key;

					return new SingleLabel(mostCommon);
				}

				static ILabel ComputeAverageLabels(Array<ILabel> labels) {
					var classCount = ((MultiLabel) labels[0]).Values.Length;
					var frequencies = new int[classCount];

					for (int instanceIndex = 0; instanceIndex < labels.Length; instanceIndex++) {
						var instanceLabels = ((MultiLabel) labels[instanceIndex]).Values;

						for (int labelIndex = 0; labelIndex < classCount; labelIndex++) {
							if (instanceLabels[labelIndex])
								frequencies[labelIndex] += 1;
						}
					}

					var instanceCount = (float) labels.Length;
					var averageLabel = new bool[classCount];

					for (int i = 0; i < averageLabel.Length; i++)
						averageLabel[i] = frequencies[i] >= (instanceCount / 2);

					return new MultiLabel(averageLabel);
				}
			}

			static int[] ComputeSingleLabelClassFrequencies(Array<ILabel> labels, int classCount) {
				var frequencies = new int[classCount];

				for (int i = 0; i < labels.Length; i++) {
					var classIndex = ((SingleLabel) labels[i]).Value;
					frequencies[classIndex] += 1;
				}

				return frequencies;
			}

			static int[] ComputeMultiLabelClassFrequencies(Array<ILabel> labels, int classCount) {
				var frequencies = new int[classCount];

				for (int instanceIndex = 0; instanceIndex < labels.Length; instanceIndex++) {
					var instanceLabels = ((MultiLabel) labels[instanceIndex]).Values;

					for (int labelIndex = 0; labelIndex < classCount; labelIndex++) {
						if (instanceLabels[labelIndex])
							frequencies[labelIndex] += 1;
					}
				}

				return frequencies;
			}
		}

		public bool IsFeatureIndexValid(int featureIndex) {
			return featureIndex >= 0 && featureIndex < FeatureCount;
		}

		public bool IsInstanceIndexValid(int instanceIndex) {
			return instanceIndex >= 0 && instanceIndex < InstanceCount;
		}

		public FeatureType GetFeatureType(int featureIndex) {
			if (!IsFeatureIndexValid(featureIndex))
				throw new ArgumentOutOfRangeException(nameof(featureIndex) + $" must be between [0, {FeatureCount}[.");

			return FeatureTypes[featureIndex];
		}

		public Array<float> GetInstanceData(int instanceIndex) {
			if (!IsInstanceIndexValid(instanceIndex))
				throw new ArgumentOutOfRangeException(nameof(instanceIndex) + $" must be between [0, {InstanceCount}[.");

			return Data.GetRow(instanceIndex);
		}

		public ILabel GetInstanceLabel(int instanceIndex) {
			if (!IsInstanceIndexValid(instanceIndex))
				throw new ArgumentOutOfRangeException(nameof(instanceIndex) + $" must be between [0, {InstanceCount}[.");

			return InstanceLabels[instanceIndex];
		}

		public Array<float> GetSortedUniqueFeatureValues(int featureIndex) {
			if (!IsFeatureIndexValid(featureIndex))
				throw new ArgumentOutOfRangeException(nameof(featureIndex) + $" must be between [0, {FeatureCount}[.");

			return _sortedUniqueFeatureValues[featureIndex];
		}

		public double[] ComputeDistances(int targetInstanceIndex, Array<int> otherInstancesIndices) {
			if (!IsInstanceIndexValid(targetInstanceIndex))
				throw new ArgumentOutOfRangeException(nameof(targetInstanceIndex) + $" must be between [0, {InstanceCount}[.");

			var distances = new double[otherInstancesIndices.Length];

			for (int i = 0; i < otherInstancesIndices.Length; i++) {
				var rhsIndex = otherInstancesIndices[i];

				if (!IsInstanceIndexValid(rhsIndex))
					throw new ArgumentException(nameof(otherInstancesIndices) + " contains invalid indices.");

				distances[i] = _distanceMatrix.Get(
					rowIndex: targetInstanceIndex,
					columnIndex: rhsIndex);
			}

			return distances;
		}

		public (float Minimum, float Maximum) GetMinimumAndMaximumFeatureValues(int featureIndex) {
			if (!IsFeatureIndexValid(featureIndex))
				throw new ArgumentOutOfRangeException(nameof(featureIndex) + $" must be between [0, {FeatureCount}[.");

			// @Performance: store the minimum and maximum values as fields?

			var values = _sortedUniqueFeatureValues[featureIndex];
			return (Minimum: values[0], Maximum: values[^1]);
		}
	}
}