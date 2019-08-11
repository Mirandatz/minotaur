namespace Minotaur.Collections.Dataset {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using Minotaur.ExtensionMethods.SystemArray;

	public sealed class Dataset {
		public readonly int InstanceCount;
		public readonly int FeatureCount;
		public readonly int ClassCount;

		public readonly Matrix<bool> InstanceLabels;

		// This is stored just for convenience
		public readonly Array<bool> DefaultClassificationLabels;

		private readonly FeatureType[] _featureTypes;
		private readonly Matrix<float> _data;

		// These are stored for performance reasons
		private readonly Matrix<float> _dataTransposed;
		private readonly Matrix<bool> _labelsTransposed;
		private readonly float[][] _sortedUniqueFeatureValues;
		private readonly Dictionary<float, int>[] _featureValueFrequencies;
		//private readonly ConstrainedFeatureSpace[] _constrainedFeatureSpaces;

		private Dataset(
			FeatureType[] featureTypes,
			Matrix<float> data,
			Matrix<float> dataTransposed,
			float[][] sortedUniqueFeatureValues,
			Dictionary<float, int>[] featureValueFrequencies,
			//ConstrainedFeatureSpace[] constrainedFeatureSpaces,
			Matrix<bool> labels,
			Matrix<bool> labelsTransposed
			) {
			_featureTypes = featureTypes;
			_data = data;
			_dataTransposed = dataTransposed;
			_labelsTransposed = labelsTransposed;
			_sortedUniqueFeatureValues = sortedUniqueFeatureValues;
			_featureValueFrequencies = featureValueFrequencies;
			//_constrainedFeatureSpaces = constrainedFeatureSpaces;

			InstanceCount = data.RowCount;
			FeatureCount = data.ColumnCount;
			ClassCount = labels.ColumnCount;
			InstanceLabels = labels;

			DefaultClassificationLabels = Enumerable
				.Repeat(false, ClassCount)
				.ToArray()
				.AsReadOnly();
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
			//var constrainedFeatureSpaces = new ConstrainedFeatureSpace[featuresCount];

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

					//var cfs = new ConstrainedFeatureSpace(
					//	featureIndex: featureIndex,
					//	featureType: featureTypes[featureIndex],
					//	values: featureValues);

					//constrainedFeatureSpaces[featureIndex] = cfs;
				});

			return new Dataset(
				featureTypes: featureTypesCopy,
				data: dataCopy,
				dataTransposed: dataTransposedCopy,
				sortedUniqueFeatureValues: sortedUniqueFeatureValues,
				featureValueFrequencies: featureValueFrequencies,
				//constrainedFeatureSpaces: constrainedFeatureSpaces,
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

		public FeatureType GetFeatureType(int featureIndex) {
			if (featureIndex < 0 || featureIndex >= FeatureCount)
				throw new ArgumentOutOfRangeException(nameof(featureIndex) + $" must be between [0, {FeatureCount}[.");

			return _featureTypes[featureIndex];
		}

		public ReadOnlySpan<float> GetInstanceData(int instanceIndex) {
			if (instanceIndex < 0 || instanceIndex >= InstanceCount)
				throw new ArgumentOutOfRangeException(nameof(instanceIndex) + $" must be between [0, {InstanceCount}[.");

			return _data.GetRow(instanceIndex);
		}

		public ReadOnlySpan<float> GetFeatureValues(int featureIndex) {
			if (featureIndex < 0 || featureIndex >= FeatureCount)
				throw new ArgumentOutOfRangeException(nameof(featureIndex) + $" must be between [0, {FeatureCount}[.");

			return _dataTransposed.GetRow(featureIndex);
		}

		public ReadOnlySpan<float> GetSortedUniqueFeatureValues(int featureIndex) {
			if (featureIndex < 0 || featureIndex >= FeatureCount)
				throw new ArgumentOutOfRangeException(nameof(featureIndex) + $" must be between [0, {FeatureCount}[.");

			return _sortedUniqueFeatureValues[featureIndex];
		}

		public int GetFeatureValueFrequency(int featureIndex, float featureValue) {
			if (featureIndex < 0 || featureIndex >= FeatureCount)
				throw new ArgumentOutOfRangeException(nameof(featureIndex) + $" must be between [0, {FeatureCount}[.");

			return _featureValueFrequencies[featureIndex].GetValueOrDefault(
				key: featureValue,
				defaultValue: 0);
		}

		public ReadOnlySpan<bool> GetInstanceLabels(int instanceIndex) {
			if (instanceIndex < 0 || instanceIndex >= InstanceCount)
				throw new ArgumentOutOfRangeException(nameof(instanceIndex) + $" must be between [0, {InstanceCount}[.");

			return InstanceLabels.GetRow(instanceIndex);
		}
	}
}