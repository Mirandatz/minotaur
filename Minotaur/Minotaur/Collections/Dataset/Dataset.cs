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
		private readonly float[][] _sortedFeatureValues;
		private readonly float[][] _sortedUniqueFeatureValues;
		private readonly Dictionary<float, int>[] _featureValueFrequencies;
		private readonly IDimensionInterval[] _dimensionIntervals;

		private Dataset(
			FeatureType[] featureTypes,
			Matrix<float> data,
			Matrix<float> dataTransposed,
			float[][] sortedFeatureValues,
			float[][] sortedUniqueFeatureValues,
			Dictionary<float, int>[] featureValueFrequencies,
			Matrix<bool> labels,
			IDimensionInterval[] dimensionIntervals
			) {
			FeatureTypes = featureTypes;
			Data = data;
			DataTransposed = dataTransposed;
			_sortedFeatureValues = sortedFeatureValues;
			_sortedUniqueFeatureValues = sortedUniqueFeatureValues;
			_featureValueFrequencies = featureValueFrequencies;

			InstanceCount = data.RowCount;
			FeatureCount = data.ColumnCount;
			ClassCount = labels.ColumnCount;
			InstanceLabels = labels;
			_dimensionIntervals = dimensionIntervals;
		}

		public static Dataset CreateFromMutableObjects(
			FeatureType[] mutableFeatureTypes,
			MutableMatrix<float> mutableData,
			MutableMatrix<bool> mutableLabels
			) {
			if (mutableFeatureTypes == null)
				throw new ArgumentNullException(nameof(mutableFeatureTypes));
			if (mutableData == null)
				throw new ArgumentNullException(nameof(mutableData));
			if (mutableLabels == null)
				throw new ArgumentNullException(nameof(mutableLabels));

			if (mutableFeatureTypes.Length != mutableData.ColumnCount)
				throw new ArgumentException("featureTypes.Length must be equal to  data.ColumnCount");
			if (mutableData.RowCount != mutableLabels.RowCount)
				throw new ArgumentException("label.RowCount must be equal to data.RowCount");

			var featureTypes = mutableFeatureTypes.ToArray();
			var data = mutableData.ToMatrix();
			var dataTransposed = mutableData.Transpose().ToMatrix();
			var labels = mutableLabels.ToMatrix();
			var labelsTranposed = mutableLabels.Transpose().ToMatrix();

			var featuresCount = featureTypes.Length;

			var sortedFeatureValues = new float[featuresCount][];
			var sortedUniqueFeatureValues = new float[featuresCount][];
			var featureValueFrequencies = new Dictionary<float, int>[featuresCount];
			var dimensionIntervals = new IDimensionInterval[featuresCount];

			Parallel.For(
				fromInclusive: 0,
				toExclusive: featuresCount,
				body: featureIndex => {

					var featureValues = dataTransposed.GetRow(featureIndex).ToArray();

					if (featureValues.Any(v => !float.IsFinite(v)))
						throw new InvalidOperationException(nameof(Dataset) + " only supports finite values.");

					sortedUniqueFeatureValues[featureIndex] = featureValues
					.Distinct()
					.OrderBy(v => v)
					.ToArray();

					if (sortedUniqueFeatureValues[featureIndex].Length == 1) {
						switch (featureTypes[featureIndex]) {

						case FeatureType.Categorical:
						featureTypes[featureIndex] = FeatureType.CategoricalButTriviallyValued;
						break;

						case FeatureType.Continuous:
						featureTypes[featureIndex] = FeatureType.ContinuousButTriviallyValued;
						break;

						default:
						throw new InvalidOperationException();
						}
					}

					sortedFeatureValues[featureIndex] = featureValues
					.OrderBy(v => v)
					.ToArray();

					var counts = featureValues
					.GroupBy(v => v)
					.ToDictionary(
						keySelector: g => g.Key,
						elementSelector: g => g.Count());

					featureValueFrequencies[featureIndex] = counts;

					dimensionIntervals[featureIndex] = CreateDimensionInterval(
						featureIndex: featureIndex,
						featureType: featureTypes[featureIndex],
						sortedUniqueValues: sortedUniqueFeatureValues[featureIndex]);
				});

			return new Dataset(
				featureTypes: featureTypes,
				data: data,
				dataTransposed: dataTransposed,
				sortedFeatureValues: sortedFeatureValues,
				sortedUniqueFeatureValues: sortedUniqueFeatureValues,
				featureValueFrequencies: featureValueFrequencies,
				labels: labels,
				dimensionIntervals: dimensionIntervals);
		}

		private static IDimensionInterval CreateDimensionInterval(
			int featureIndex,
			FeatureType featureType,
			float[] sortedUniqueValues
			) {

			switch (featureType) {

			case FeatureType.Categorical:
			return CategoricalDimensionInterval.FromSortedUniqueValues(
				dimensionIndex: featureIndex,
				sortedUniqueValues: sortedUniqueValues);

			case FeatureType.CategoricalButTriviallyValued:
			return CategoricalDimensionInterval.FromSingleValue(
				dimensionIndex: featureIndex,
				value: sortedUniqueValues[0]);

			case FeatureType.Continuous: {
				var start = DimensionBound.CreateStart(sortedUniqueValues[0]);
				var end = DimensionBound.CreateEnd(sortedUniqueValues[^1]);
				return new ContinuousDimensionInterval(
					dimensionIndex: featureIndex,
					start: start,
					end: end);
			}

			case FeatureType.ContinuousButTriviallyValued:
			return ContinuousDimensionInterval.FromSingleValue(
				dimensionIndex: featureIndex,
				value: sortedUniqueValues[0]);

			default:
			throw new InvalidOperationException(ExceptionMessages.UnknownFeatureType);
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
			if (!IsFeatureIndexValid(featureIndex))
				return false;

			var featureType = GetFeatureType(featureIndex);
			switch (featureType) {

			case FeatureType.Categorical: {
				var categorical = dimensionInterval as CategoricalDimensionInterval;
				if (categorical == null)
					return false;

				var possibleValues = _sortedUniqueFeatureValues[featureIndex];
				var actualValues = categorical.SortedValues;

				// @Improve performance of the look-up operation.
				// Maybe use HashSet to store the unique values?
				for (int i = 0; i < actualValues.Length; i++) {
					var valueIndex = Array.BinarySearch(
						array: possibleValues,
						value: actualValues[i]);

					if (valueIndex < 0)
						return false;
				}

				return true;
			}

			case FeatureType.Continuous: {
				var continuous = dimensionInterval as ContinuousDimensionInterval;
				if (continuous == null)
					return false;

				var possibleValues = _sortedUniqueFeatureValues[featureIndex];
				var lowerBound = continuous.Start.Value;

				// @Improve performance of the look-up operation.
				// Maybe use HashSet to store the unique values?
				if (!float.IsNegativeInfinity(lowerBound)) {
					var valueIndex = Array.BinarySearch(
						array: possibleValues,
						value: lowerBound);

					if (valueIndex < 0)
						return false;
				}

				// @Improve performance of the look-up operation.
				// Maybe use HashSet to store the unique values?
				var upperBound = continuous.End.Value;
				if (!float.IsPositiveInfinity(upperBound)) {
					var valueIndex = Array.BinarySearch(
						array: possibleValues,
						value: upperBound);

					if (valueIndex < 0)
						return false;
				}

				return true;
			}

			default:
			throw new InvalidOperationException($"Unknown / unsupported value for {nameof(FeatureType)}.");
			}
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

		public Array<bool> GetInstanceLabels(int instanceIndex) {
			if (!IsInstanceIndexValid(instanceIndex))
				throw new ArgumentOutOfRangeException(nameof(instanceIndex) + $" must be between [0, {InstanceCount}[.");

			return InstanceLabels.GetRow(instanceIndex);
		}

		public IDimensionInterval GetDimensionInterval(int featureIndex) {
			if (!IsFeatureIndexValid(featureIndex))
				throw new ArgumentOutOfRangeException(nameof(featureIndex));

			return _dimensionIntervals[featureIndex];
		}
	}
}
