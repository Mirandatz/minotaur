namespace Minotaur.Collections.Dataset {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using Minotaur.Math.Dimensions;

	public sealed class Dataset {
		public readonly int InstanceCount;
		public readonly int FeatureCount;
		public readonly int ClassCount;

		public readonly Matrix<bool> InstanceLabels;

		public readonly Array<FeatureType> FeatureTypes;
		public readonly Matrix<float> Data;

		// These are stored for performance reasons
		public readonly Matrix<float> DataTransposed;
		private readonly float[][] _sortedUniqueFeatureValues;
		private readonly Dictionary<float, int>[] _featureValueFrequencies;

		private Dataset(
			FeatureType[] featureTypes,
			Matrix<float> data,
			Matrix<float> dataTransposed,
			float[][] sortedUniqueFeatureValues,
			Dictionary<float, int>[] featureValueFrequencies,
			Matrix<bool> labels,
			Matrix<bool> labelsTransposed
			) {
			FeatureTypes = featureTypes;
			Data = data;
			DataTransposed = dataTransposed;
			_sortedUniqueFeatureValues = sortedUniqueFeatureValues;
			_featureValueFrequencies = featureValueFrequencies;

			InstanceCount = data.RowCount;
			FeatureCount = data.ColumnCount;
			ClassCount = labels.ColumnCount;
			InstanceLabels = labels;
		}

		public static Dataset CreateFromMutableObjects(
			FeatureType[] featureTypes,
			MutableMatrix<float> data,
			MutableMatrix<bool> labels
			) {
			if (featureTypes == null)
				throw new ArgumentNullException(nameof(featureTypes));
			if (data == null)
				throw new ArgumentNullException(nameof(data));
			if (labels == null)
				throw new ArgumentNullException(nameof(labels));

			if (featureTypes.Length != data.ColumnCount)
				throw new ArgumentException("featureTypes.Length must be equal to  data.ColumnCount");
			if (data.RowCount != labels.RowCount)
				throw new ArgumentException("label.RowCount must be equal to data.RowCount");

			var featureTypesCopy = featureTypes.ToArray();
			var dataCopy = data.ToMatrix();
			var dataTransposedCopy = data.Transpose().ToMatrix();
			var labelsCopy = labels.ToMatrix();
			var labelsTransposedCopy = labels.Transpose().ToMatrix();

			var featuresCount = featureTypes.Length;

			var sortedUniqueFeatureValues = new float[featuresCount][];
			var featureValueFrequencies = new Dictionary<float, int>[featuresCount];

			Parallel.For(
				fromInclusive: 0,
				toExclusive: featuresCount,
				body: featureIndex => {

					var featureValues = dataTransposedCopy.GetRow(featureIndex).ToArray();

					sortedUniqueFeatureValues[featureIndex] = featureValues
					.Distinct()
					.OrderBy(v => v)
					.ToArray();

					var counts = featureValues
					.GroupBy(v => v)
					.ToDictionary(
						keySelector: g => g.Key,
						elementSelector: g => g.Count());

					featureValueFrequencies[featureIndex] = counts;
				});

			return new Dataset(
				featureTypes: featureTypesCopy,
				data: dataCopy,
				dataTransposed: dataTransposedCopy,
				sortedUniqueFeatureValues: sortedUniqueFeatureValues,
				featureValueFrequencies: featureValueFrequencies,
				labels: labelsCopy,
				labelsTransposed: labelsTransposedCopy);
		}

		public static void ThrowIfTrainAndTestAreIncompatible(Dataset trainDataset, Dataset testDataset) {
			if (trainDataset == null)
				throw new ArgumentNullException(nameof(trainDataset));
			if (testDataset == null)
				throw new ArgumentNullException(nameof(testDataset));

			if (trainDataset.FeatureCount != testDataset.FeatureCount)
				throw new InvalidOperationException(nameof(trainDataset) + " and " + nameof(testDataset) + " must have the same feature count");
			if (trainDataset.ClassCount != testDataset.ClassCount)
				throw new InvalidOperationException(nameof(trainDataset) + " and " + nameof(testDataset) + " must have the same class count");

			for (int i = 0; i < trainDataset.FeatureCount; i++) {
				var trainFeatureType = trainDataset.GetFeatureType(i);
				var testFeatureType = testDataset.GetFeatureType(i);

				if (trainFeatureType != testFeatureType)
					throw new InvalidOperationException(nameof(trainDataset) + " and " + nameof(testDataset) + " must have the same feature types");
			}
		}

		public bool IsFeatureIndexValid(int featureIndex) {
			return featureIndex >= 0 && featureIndex < FeatureCount;
		}

		public bool IsInstanceIndexValid(int instanceIndex) {
			return instanceIndex >= 0 && instanceIndex < InstanceCount;
		}

		public bool IsDimesionIntervalValid(IDimensionInterval dimensionInterval) {
			if (dimensionInterval is null)
				throw new ArgumentNullException(nameof(dimensionInterval));

			var featureIndex = dimensionInterval.DimensionIndex;
			throw new NotImplementedException();
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

		public Array<bool> GetInstanceLabels(int instanceIndex) {
			if (!IsInstanceIndexValid(instanceIndex))
				throw new ArgumentOutOfRangeException(nameof(instanceIndex) + $" must be between [0, {InstanceCount}[.");

			return InstanceLabels.GetRow(instanceIndex);
		}
	}
}
