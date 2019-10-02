namespace Minotaur.Math.Dimensions {
	using System;
	using Minotaur.Collections.Dataset;

	public sealed class HyperRectangleBuilder {

		public enum BinaryDimensionIntervalStatus {
			Undefined,
			ContainsOnlyTrue,
			ContainsOnlyFalse,
			ContainsTrueAndFalse
		}

		public readonly Dataset Dataset;

		private readonly BinaryDimensionIntervalStatus[] _binaryIntervalStatus;
		private readonly float[] _starts;
		private readonly float[] _ends;

		private HyperRectangleBuilder(Dataset dataset) {
			Dataset = dataset;

			var featureCount = Dataset.FeatureCount;
			_binaryIntervalStatus = new BinaryDimensionIntervalStatus[featureCount];
			_starts = new float[featureCount];
			_ends = new float[featureCount];

			for (int i = 0; i < featureCount; i++) {
				_binaryIntervalStatus[i] = BinaryDimensionIntervalStatus.Undefined;
				_starts[i] = float.NaN;
				_ends[i] = float.NaN;
			}
		}

		public static HyperRectangleBuilder InitializeWithSeed(Dataset dataset, int seedIndex) {
			var builder = new HyperRectangleBuilder(dataset);
			var seed = dataset.GetFeatureValues(seedIndex);
			var featureCount = dataset.FeatureCount;

			for (int i = 0; i < featureCount; i++) {
				switch (dataset.GetFeatureType(i)) {

				case FeatureType.Binary: {
					if (seed[i] == 0f) {
						builder.UpdateBinaryDimensionIntervalValue(
							dimensionIndex: i,
							status: BinaryDimensionIntervalStatus.ContainsOnlyFalse);

						break;
					}

					if (seed[i] == 1f) {
						builder.UpdateBinaryDimensionIntervalValue(
							dimensionIndex: i,
							status: BinaryDimensionIntervalStatus.ContainsOnlyTrue);
						break;
					}

					throw new InvalidOperationException();
				}

				case FeatureType.Continuous: {
					builder.UpdateContinuousDimensionIntervalStart(
						dimensionIndex: i,
						value: seed[i]);

					builder.UpdateContinuousDimensionIntervalEnd(
						dimensionIndex: i,
						value: seed[i]);

					break;
				}

				default:
				throw CommonExceptions.UnknownFeatureType;
				}
			}

			return builder;
		}

		public static HyperRectangleBuilder InitializeWithLargestRectangle(Dataset dataset) {
			var builder = new HyperRectangleBuilder(dataset);
			var featureCount = dataset.FeatureCount;

			for (int i = 0; i < featureCount; i++) {
				switch (dataset.GetFeatureType(i)) {

				case FeatureType.Binary:
				builder._binaryIntervalStatus[i] = BinaryDimensionIntervalStatus.ContainsTrueAndFalse;
				break;

				case FeatureType.Continuous:
				builder._starts[i] = float.NegativeInfinity;
				builder._ends[i] = float.PositiveInfinity;
				break;

				default:
				throw CommonExceptions.UnknownFeatureType;
				}
			}

			return builder;
		}

		public void UpdateBinaryDimensionIntervalValue(int dimensionIndex, BinaryDimensionIntervalStatus status) {
			if (Dataset.GetFeatureType(dimensionIndex) != FeatureType.Binary)
				throw new InvalidOperationException();

			_binaryIntervalStatus[dimensionIndex] = status;
		}

		public void UpdateContinuousDimensionIntervalStart(int dimensionIndex, float value) {
			if (Dataset.GetFeatureType(dimensionIndex) != FeatureType.Continuous)
				throw new InvalidOperationException();
			if (float.IsNaN(value))
				throw new InvalidOperationException();

			_starts[dimensionIndex] = value;
		}

		public void UpdateContinuousDimensionIntervalEnd(int dimensionIndex, float value) {
			if (Dataset.GetFeatureType(dimensionIndex) != FeatureType.Continuous)
				throw new InvalidOperationException();
			if (float.IsNaN(value))
				throw new InvalidOperationException();

			_ends[dimensionIndex] = value;
		}

		public HyperRectangle Build() {
			var dimensionCount = Dataset.FeatureCount;
			var intervals = new IDimensionInterval[dimensionCount];
			for (int i = 0; i < intervals.Length; i++)
				intervals[i] = BuildInterval(i);

			return new HyperRectangle(intervals);
		}

		private IDimensionInterval BuildInterval(int dimensionIndex) {
			return (Dataset.GetFeatureType(dimensionIndex)) switch
			{
				FeatureType.Binary => BuildBinaryInterval(dimensionIndex),
				FeatureType.Continuous => BuildContinuousInterval(dimensionIndex),

				_ => throw CommonExceptions.UnknownFeatureType
			};
		}

		private BinaryDimensionInterval BuildBinaryInterval(int dimensionIndex) {

			return (_binaryIntervalStatus[dimensionIndex]) switch
			{
				BinaryDimensionIntervalStatus.ContainsOnlyTrue => new BinaryDimensionInterval(
					dimensionIndex: dimensionIndex,
					containsFalse: false,
					containsTrue: true),

				BinaryDimensionIntervalStatus.ContainsOnlyFalse => new BinaryDimensionInterval(
					dimensionIndex: dimensionIndex,
					containsFalse: true,
					containsTrue: false),

				BinaryDimensionIntervalStatus.ContainsTrueAndFalse => new BinaryDimensionInterval(
					dimensionIndex: dimensionIndex,
					containsFalse: true,
					containsTrue: true),

				BinaryDimensionIntervalStatus.Undefined => throw new InvalidOperationException(),
				_ => throw new InvalidOperationException()
			};
		}

		private ContinuousDimensionInterval BuildContinuousInterval(int dimensionIndex) {
			var start = _starts[dimensionIndex];
			if (float.IsNaN(start))
				throw new InvalidOperationException();

			var end = _ends[dimensionIndex];
			if (float.IsNaN(end))
				throw new InvalidOperationException();

			if (start >= end)
				throw new InvalidOperationException();

			return new ContinuousDimensionInterval(
				dimensionIndex: dimensionIndex,
				start: start,
				end: end);
		}
	}
}