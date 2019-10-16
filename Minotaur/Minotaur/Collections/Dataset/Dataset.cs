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
		public readonly Matrix<double> DistanceMatrix;
		public readonly Matrix<float> DataTransposed;
		private readonly float[][] _sortedFeatureValues;
		private readonly float[][] _sortedUniqueFeatureValues;
		private readonly Dictionary<float, int>[] _featureValueFrequencies;

		public readonly ILabel DefaultLabel;

		private Dataset(
			FeatureType[] featureTypes,
			Matrix<float> data,
			Matrix<float> dataTransposed,
			Matrix<double> distanceMatrix,
			float[][] sortedFeatureValues,
			float[][] sortedUniqueFeatureValues,
			Dictionary<float, int>[] featureValueFrequencies,
			Array<ILabel> labels,
			ClassificationType classificationType,
			int classCount,
			ILabel defaultLabel
			) {
			FeatureTypes = featureTypes;
			Data = data;
			DataTransposed = dataTransposed;
			DistanceMatrix = distanceMatrix;
			_sortedFeatureValues = sortedFeatureValues;
			_sortedUniqueFeatureValues = sortedUniqueFeatureValues;
			_featureValueFrequencies = featureValueFrequencies;

			InstanceCount = data.RowCount;
			FeatureCount = data.ColumnCount;
			ClassCount = classCount;
			InstanceLabels = labels;
			ClassificationType = classificationType;
			DefaultLabel = defaultLabel;
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

			Parallel.For(
				fromInclusive: 0,
				toExclusive: featuresCount,
				body: featureIndex => {

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

			Task.WaitAll(distanceMatrixTask);
			var distanceMatrix = distanceMatrixTask.Result;

			int classCount = classificationType switch
			{
				ClassificationType.SingleLabel => labels.Distinct().Count(),
				ClassificationType.MultiLabel => ((MultiLabel) labels[0]).Values.Length,
				_ => throw CommonExceptions.UnknownClassificationType,
			};

			var defaultLabel = ComputeDefaultLabel(labels, classificationType);

			return new Dataset(
				featureTypes: featureTypes,
				data: data,
				dataTransposed: dataTransposed,
				distanceMatrix: distanceMatrix,
				sortedFeatureValues: sortedFeatureValues,
				sortedUniqueFeatureValues: sortedUniqueFeatureValues,
				featureValueFrequencies: featureValueFrequencies,
				labels: labels,
				classificationType: classificationType,
				classCount: classCount,
				defaultLabel: defaultLabel);

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
					throw new NotImplementedException();
				}
			}
		}

		public bool IsFeatureIndexValid(int featureIndex) {
			return featureIndex >= 0 && featureIndex < FeatureCount;
		}

		public bool IsInstanceIndexValid(int instanceIndex) {
			return instanceIndex >= 0 && instanceIndex < InstanceCount;
		}

		public float GetDatum(int instanceIndex, int featureIndex) {
			if (!IsInstanceIndexValid(instanceIndex))
				throw new ArgumentOutOfRangeException(nameof(instanceIndex) + $" must be between [0, {InstanceCount}[.");
			if (!IsFeatureIndexValid(featureIndex))
				throw new ArgumentOutOfRangeException(nameof(featureIndex) + $" must be between [0, {FeatureCount}[.");

			return Data.Get(rowIndex: instanceIndex, columnIndex: featureIndex);
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

		public Array<float> GetFeatureValues(int featureIndex) {
			if (!IsFeatureIndexValid(featureIndex))
				throw new ArgumentOutOfRangeException(nameof(featureIndex) + $" must be between [0, {FeatureCount}[.");

			return DataTransposed.GetRow(featureIndex);
		}

		public Array<float> GetSortedFeatureValues(int featureIndex) {
			if (!IsFeatureIndexValid(featureIndex))
				throw new ArgumentOutOfRangeException(nameof(featureIndex) + $" must be between [0, {FeatureCount}[.");

			return _sortedFeatureValues[featureIndex];
		}

		public Array<float> GetSortedUniqueFeatureValues(int featureIndex) {
			if (!IsFeatureIndexValid(featureIndex))
				throw new ArgumentOutOfRangeException(nameof(featureIndex) + $" must be between [0, {FeatureCount}[.");

			return _sortedUniqueFeatureValues[featureIndex];
		}

		public int GetFeatureValueFrequency(int featureIndex, float featureValue) {
			if (!IsFeatureIndexValid(featureIndex))
				throw new ArgumentOutOfRangeException(nameof(featureIndex) + $" must be between [0, {FeatureCount}[.");

			return _featureValueFrequencies[featureIndex].GetValueOrDefault(
				key: featureValue,
				defaultValue: 0);
		}

		public Dictionary<float, int> GetFeatureValueFrequenciesDictionary(int featureIndex) {
			if (!IsFeatureIndexValid(featureIndex)) {
				throw new ArgumentOutOfRangeException(
				$"{nameof(featureIndex)} must be between [0, {FeatureCount}[.");
			}

			// @Performance
			var frequenciesDict = _featureValueFrequencies[featureIndex];
			return new Dictionary<float, int>(frequenciesDict);
		}

		public ILabel GetInstanceLabel(int instanceIndex) {
			if (!IsInstanceIndexValid(instanceIndex))
				throw new ArgumentOutOfRangeException(nameof(instanceIndex) + $" must be between [0, {InstanceCount}[.");

			return InstanceLabels[instanceIndex];
		}

		public double[] ComputeDistances(int targetInstanceIndex, Array<int> otherInstancesIndices) {
			if (!IsInstanceIndexValid(targetInstanceIndex))
				throw new ArgumentOutOfRangeException(nameof(targetInstanceIndex) + $" must be between [0, {InstanceCount}[.");

			var distances = new double[otherInstancesIndices.Length];

			for (int i = 0; i < otherInstancesIndices.Length; i++) {
				var rhsIndex = otherInstancesIndices[i];

				if (!IsInstanceIndexValid(rhsIndex))
					throw new ArgumentException(nameof(otherInstancesIndices) + " contains invalid indices.");

				distances[i] = DistanceMatrix.Get(
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